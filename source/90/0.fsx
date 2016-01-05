open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts
open System.IO

type EnumerableStream(data:seq<byte>) =
  inherit Stream()
  do Contract.Requires(data <> null)
  let d = data.GetEnumerator()

  interface IEnumerable<byte> with
    member this.GetEnumerator() = d
    member this.GetEnumerator() = d :> IEnumerator 

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