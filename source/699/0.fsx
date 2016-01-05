module Divided.ArcanumFiles

open System
open System.IO
open System.IO.Compression
open System.Text
open System.Collections.Generic

let private FileMagic = 827605316
let private CompressedMagic = 0x00000002
let private DirectoryMagic = 0x00000400
let private FooterSize = 28L

let private readSequence (array : byte []) (stream : Stream) pos len =
    let ret = stream.Read(array, pos, len)
    if ret <> len then
        EndOfStreamException() |> raise
    else
        ()

let inline private readByteOrDie (stream : Stream) =
    let ret = stream.ReadByte()
    if ret = -1 then
        EndOfStreamException() |> raise
    else
        ret

let private readInt32 (stream : Stream) =
    let u4 = readByteOrDie stream
    let u3 = readByteOrDie stream
    let u2 = readByteOrDie stream
    let u1 = readByteOrDie stream
    (u4 <<< 24) ||| (u3 <<< 16) ||| (u2 <<< 8) ||| u1

let private readLittleInt32 (stream : Stream) =
    let u1 = readByteOrDie stream
    let u2 = readByteOrDie stream
    let u3 = readByteOrDie stream
    let u4 = readByteOrDie stream
    (u4 <<< 24) ||| (u3 <<< 16) ||| (u2 <<< 8) ||| u1

type ArcanumFile =
    { Compressed: bool; CompressedSize: int32; Offset: int32; Filename: string; Size: int32 }
    member o.GetContents(stream : Stream) =
        let size = o.Size |> int
        stream.Position <- (int64 o.Offset) + (if o.Compressed then 2L else 0L)
        let buf = Array.zeroCreate size
        if o.Compressed then
            use stream = new DeflateStream(stream, CompressionMode.Decompress, true)
            readSequence buf stream 0 size
        else
            readSequence buf stream 0 size
        buf

type ArcanumDat(DatStream : Stream) =
    let _files =
        let sizeTotal = DatStream.Length
        let endOfMetadata = sizeTotal - FooterSize
        DatStream.Position <- endOfMetadata
        let buf16 = Array.zeroCreate 16
        readSequence buf16 DatStream 0 16
        let fileMagic = readInt32 DatStream
        if fileMagic <> FileMagic then
            IOException "bad magic" |> raise
        readInt32 DatStream |> ignore
        let dictSize = readLittleInt32 DatStream
        DatStream.Position <- sizeTotal - (int64 dictSize)
        let entryCount = readLittleInt32 DatStream
        let buf = Array.zeroCreate 1024 |> ref
        let dict = Dictionary(entryCount)
        for i in 0 .. entryCount - 1 do
            let len = readLittleInt32 DatStream
            if len > (!buf).Length then
                Array.Resize(buf, len)
            readSequence !buf DatStream 0 len
            let filename = Encoding.ASCII.GetString(!buf, 0, (len - 1)).ToLowerInvariant()
            readInt32 DatStream |> ignore
            let flags = readLittleInt32 DatStream
            let decompressedSize = readLittleInt32 DatStream
            let compressedSize = readLittleInt32 DatStream
            let offset = readLittleInt32 DatStream
            let compressed = (flags &&& CompressedMagic) <> 0
            if flags &&& DirectoryMagic = 0 then
                dict.Add(filename, { Compressed = compressed; CompressedSize = compressedSize; Offset = offset; Filename = filename; Size = decompressedSize })
        dict
    interface IDisposable with
        member o.Dispose() = DatStream.Close()
    new(filename : String) = new ArcanumDat(File.OpenRead(filename))
    member o.GetFilenames() = _files.Keys
    member o.Files with get filename = _files.[filename]
    override o.ToString() = String.Format("#<ArcanumDat {0:x} {1} total>", o.GetHashCode(), _files.Count)