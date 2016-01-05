///
module Target = 
    
  /// There are three types of compilation
  /// targets. Eval for code compiled through
  /// the eval function, Global for code
  /// that is compiled in the global scope
  /// and Function for code inside function bodies
  type Mode
    = Eval
    | Global
    | Function

  /// Record that represents a compilation target
  /// which is a grouping of the following properties:
  /// 
  /// * Ast - The syntax tree to compile
  /// * Mode - The target mode (eval, global or function)
  /// * DelegateType - The target delegate signature we're targeting
  /// * ParameterTypes - The parameter types of the delegate signature's invoke method
  /// * Environment - The IronJS environment object we're compiling for
  type T = {
    Ast: Ast.Tree
    Mode: Mode
    DelegateType: Type option
    ParameterTypes: Type array
    Environment: Env
  }

  /// The amount of parameters for this target
  let parameterCount (t:T) =
    t.ParameterTypes.Length

  /// Extracts the parameter types from a delegate
  let getParameterTypes = function
    | None -> [||]
    | Some(delegateType:Type) -> 
      delegateType 
      $ FSharp.Reflection.getDelegateParameterTypes 
      $ FSharp.Array.skip 2

  /// Creates a new T record
  let create ast mode delegateType env =
    {
      Ast = ast
      Mode = mode
      Environment = env
      DelegateType = delegateType
      ParameterTypes = delegateType |> getParameterTypes
    }
    
  /// Creates a new T record with Eval mode
  let createEval ast env =
    env |> create ast Mode.Eval None

  /// Creates a new T record with Global mode
  let createGlobal ast env =
    env |> create ast Mode.Global None