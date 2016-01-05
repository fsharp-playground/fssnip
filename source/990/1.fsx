open System
open System.IO
open System.Reflection
open System.Reflection.Emit
open System.Collections.Generic

let opcodeMap =
  let fields = typeof<OpCodes>.GetFields(BindingFlags.Public ||| BindingFlags.Static)
  
  fields 
  |> Array.map (fun x -> 
     let opcode = x.GetValue null :?> OpCode
     opcode.Value, opcode)
  |> Map.ofArray

let findOpcode opc = 
  Map.tryFind opc opcodeMap

type OpCodeReturn =
  | IL_Nop
  | IL_Int32    of int
  | IL_Int16    of int16
  | IL_Float    of float
  | IL_Int64    of int64
  | IL_Sbyte    of sbyte
  | IL_Byte     of byte
  | IL_Switch   of int array
  | IL_Sig      of byte array
  | IL_String   of string
  | IL_Field    of MemberInfo
  | IL_LocalVar of LocalVariableInfo
  | IL_Param    of ParameterInfo


//module binary =
let readInt32 (ms : BinaryReader) =
  ms.ReadInt32() 

let readInt16AsInt (ms : BinaryReader) = 
  ms.ReadInt16() |> int

let readInt64 (ms : BinaryReader) = 
  ms.ReadInt64() 

let readFloat (ms : BinaryReader) =
  ms.ReadDouble()

let readByteAsInt (ms : BinaryReader) = 
  ms.ReadByte() |> int

let readBytes (ms : BinaryReader) n =
  ms.ReadBytes n

let streamPos (ms : BinaryReader) = 
  ms.BaseStream.Position |> int

let streamLen (ms : BinaryReader) = 
  ms.BaseStream.Length |> int

let getType (f : MethodBase) = 
  f.GetType() 

let isStatic (f : MethodBase) =
  f.IsStatic

// thank you to the below author, i wouldn't have been able to get this without copying his code
// ref: https://github.com/jbevain/mono.reflection/blob/master/Mono.Reflection/MethodBodyReader.cs

// can be 1byte or 2byte opcode, 2 byte opcodes ALWAYS start with 0xfe.
let readOpcode (ms : BinaryReader) = 
  let op = readByteAsInt ms
  if  op <> 254 then
      int16 op |> findOpcode 
  else 
      BitConverter.ToInt16(Array.append [| 0xfeuy |] (readBytes ms 1), 0)
      |> findOpcode

let getInlineSwitch (ms : BinaryReader) = 
  let length = readInt32 ms
  let offset = streamPos ms + (4 * length)
  [| for i in 0 .. length - 1 -> readInt32 ms + offset |]

let walkBranches (ms : BinaryReader) =
  let len = readInt32 ms
  [| 0 .. len - 1 |]
  |> Array.map (fun x ->
     readInt32 ms |> fun offset -> streamPos ms + offset)

let checkByteSignLd (ms : BinaryReader) opcode =
  if opcode = OpCodes.Ldc_I4_S then
    readByteAsInt ms + streamPos ms |> sbyte |> IL_Sbyte
  else
    readByteAsInt ms |> byte |> IL_Byte

type instructionModel = {
  func   : MethodBase
  mbytes : byte array
  modulx : Module
  locals : LocalVariableInfo array
  paramx : ParameterInfo array
  gener1 : Type []
  gener2 : Type []
}

let createInstructionModel (f : MethodBase) =
  { func   = f
    mbytes = f.GetMethodBody().GetILAsByteArray()
    modulx = f.Module
    locals = f.GetMethodBody().LocalVariables |> Seq.toArray
    paramx = f.GetParameters()
    gener1 = 
      if getType f <> typeof<ConstructorInfo> then 
        f.GetGenericArguments() else [||]

    gener2 = 
      if f.DeclaringType <> null then 
        f.DeclaringType.GetGenericArguments() else [||]
  }

let resolveToken IM token =
  match IM.gener1, IM.gener2 with
  | ([||],[||]) -> IM.func.Module.ResolveMember  token
  | (a,b)       -> IM.func.Module.ResolveMember (token,a,b)

let resolveVariable IM (opcode : OpCode) index =
 
  let p n = IM.paramx.[index-n] |> IL_Param
  let l _ = IM.locals.[index  ] |> IL_LocalVar

  if opcode.Name.Contains "loc" then l ()
  else    if isStatic IM.func then p 0
  else    if index <> 0 then p 1
  else       IL_Nop    

let test (f : MethodBase) = 
  
  let IM = createInstructionModel f

  use ms = BinaryReader (MemoryStream IM.mbytes)

  let parseOpcode (opcode : OpCode) =
    match opcode.OperandType  with
    | OperandType.InlineNone          -> IL_Nop
    | OperandType.InlineSwitch        -> walkBranches  ms |> IL_Switch
    | OperandType.ShortInlineBrTarget -> readByteAsInt ms + streamPos ms |> IL_Int32
    | OperandType.InlineBrTarget      -> readInt32 ms     + streamPos ms |> IL_Int32
    | OperandType.ShortInlineI        -> checkByteSignLd ms opcode
    | OperandType.InlineI             -> readInt32 ms  |> IL_Int32
    | OperandType.ShortInlineR        
    | OperandType.InlineR             -> readFloat ms  |> IL_Float
    | OperandType.InlineI8            -> readInt64 ms  |> IL_Int64
    | OperandType.InlineSig           -> readInt32 ms  |> IM.modulx.ResolveSignature |> IL_Sig
    | OperandType.InlineString        -> readInt32 ms  |> IM.modulx.ResolveString    |> IL_String
    | OperandType.InlineTok 
    | OperandType.InlineType
    | OperandType.InlineMethod
    | OperandType.InlineField         -> resolveToken IM (readInt32 ms) |> IL_Field
    | OperandType.ShortInlineVar      -> resolveVariable  IM opcode (readByteAsInt ms)
    | OperandType.InlineVar           -> resolveVariable  IM opcode (readInt16AsInt ms)

  let rec loop L = 
    if streamPos ms = streamLen ms then L
    else
      match readOpcode ms  with
      | Some code -> loop ((code, parseOpcode code)::L)
      | None      -> loop ((OpCodes.Nop, IL_Nop)::L)

  loop [] 

let print_instruction_fields ( xs : (OpCode * OpCodeReturn) list ) =
  xs |> Seq.iter (fun (opcode,mapping) -> printfn " %-5s | %A" opcode.Name mapping)

let disassemble f =
  (f.GetType().GetMethod "Invoke").MethodHandle
  |> MethodBase.GetMethodFromHandle
  |> test


(* testing*)
let myfunc1() = 
  let testfunc = "this is a test"
  testfunc ^ " abc"

disassemble myfunc1 |> List.rev |> print_instruction_fields
(* 
 nop   | IL_Nop
 ldstr | IL_String "this is a test"
 ldstr | IL_String " abc"
 call  | IL_Field System.String Concat(System.String, System.String)
 ret   | IL_Nop
 *)