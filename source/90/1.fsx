open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters.Binary
open System.Text

/// Extensions to dictionaries.
module Dict =
  open System.Collections.Generic
  let toSeq d = d |> Seq.map (fun (KeyValue(k,v)) -> (k,v))
  let toArray (d:IDictionary<_,_>) = d |> toSeq |> Seq.toArray
  let toList (d:IDictionary<_,_>) = d |> toSeq |> Seq.toList

/// Extensions to the Array module.
module Array =
  /// Slices out a portion of the array from the start index up to the stop index.
  let slice start stop (source:'a[]) =
    let stop' = ref stop
    if !stop' < 0 then stop' := source.Length + !stop'
    let len = !stop' - start
    [| for i in [0..(len-1)] do yield source.[i + start] |]

  [<System.Runtime.CompilerServices.Extension>]
  let Slice(arr, start, stop) = slice start stop arr

/// Initializes a new instance of the SeqStream class.
/// <see href="http://extensia.codeplex.com"/>
type SeqStream(data:seq<byte>) =
  inherit Stream()
  do Contract.Requires(data <> null)
  let d = data.GetEnumerator()

  interface IEnumerable<byte> with
    /// Gets the enumerator for the SeqStream.
    member this.GetEnumerator() = data.GetEnumerator()
    /// Gets the enumerator for the SeqStream.
    member this.GetEnumerator() = data.GetEnumerator() :> IEnumerator 

  override this.CanRead = true
  override this.CanSeek = true
  override this.CanWrite = false
  override this.Flush() = ()
  override this.Length = data |> Seq.length |> int64
  override this.Position with get() = raise (NotSupportedException())
                         and set(v) = raise (NotSupportedException())
  override this.Seek(offset, origin) = raise (NotSupportedException())
  override this.SetLength(value) = raise (NotSupportedException())
  override this.Write(buffer, offset, count) = raise (NotSupportedException())
  override this.Dispose(disposing) = d.Dispose()
                                     base.Dispose(disposing)
  override this.Read(buffer, offset, count) =
    Contract.Requires(buffer <> null)
    Contract.Requires(offset >= 0)
    Contract.Requires(count > 0)
    Contract.Requires(offset + count <= buffer.Length)

    let rec loop bytesRead =
      if d.MoveNext() && bytesRead < count
        then
          buffer.[bytesRead + offset] <- d.Current
          loop (bytesRead + 1)
        else bytesRead
    loop 0
    
  /// Returns the SeqStream as a UTF8 encoded string.
  override this.ToString() = Encoding.UTF8.GetString(data |> Seq.toArray)

  /// An empty SeqStream.
  static member Empty = new SeqStream(Seq.empty<byte>)

  /// Converts a string into a SeqStream.
  static member FromString(s:string) = new SeqStream(Encoding.UTF8.GetBytes(s))

  /// Converts a stream into a SeqStream.
  static member FromStream(stream:Stream, ?bufferSize) =
    let bufferSize = defaultArg bufferSize 1024
    Contract.Requires(stream <> null)
    Contract.Requires(bufferSize > 0)

    let buffer = Array.zeroCreate bufferSize
    let count = ref 0
    count := stream.Read(buffer, 0, buffer.Length)
    let bytes = seq {
      while !count > 0 do
        for i in [0..(!count-1)] do yield buffer.[i]
        count := stream.Read(buffer, 0, buffer.Length) }
    new SeqStream(bytes)

  /// Converts a FileInfo into a SeqStream.
  static member FromFileInfo(file:FileInfo) =
    Contract.Requires(file <> null)
    use stream = file.OpenRead()
    SeqStream.FromStream(stream, int stream.Length)

  /// Converts an object to a SeqStream.
  static member FromObject(ob) =
    let formatter = BinaryFormatter()
    use stream = new MemoryStream()
    try
      formatter.Serialize(stream, ob)
      SeqStream.FromStream(stream)
    with :? SerializationException as e -> SeqStream.Empty

  /// Converts a SeqStream into an object.
  member this.Cast<'a when 'a : null>() =
    let formatter = BinaryFormatter()
    use stream = new MemoryStream(data |> Seq.toArray)
    try formatter.Deserialize(stream) :?> 'a
    with e -> null

  /// Gets the enumerator for the SeqStream.
  member this.GetEnumerator() = data.GetEnumerator()

  /// Transfers the bytes of the SeqStream into the specified stream
  member this.TransferTo (stream:Stream) =
    Contract.Requires(stream <> null)
    data |> Seq.iter (fun x -> stream.WriteByte(x))
