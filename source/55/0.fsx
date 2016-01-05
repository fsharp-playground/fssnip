// Copyright 2010 Promitheia Technologies

namespace Megaton.Engine

open System
open System.IO
open System.Threading
open Megaton.Model
open Megaton.Serialization
type Dict<'k, 'v> = System.Collections.Generic.Dictionary<'k, 'v>

type CRecord = {
    Position : int64
    Data : RecordData
}

type Cluster (path, name) =
    let mutable c_path = Path.Combine (path, name + ".mt-c")
    let records = Dict<string, CRecord> ()
    let merge_lock = new ReaderWriterLockSlim ()
    let write_lock = obj ()
    let mutable c_st : Stream = null
    let mutable c_bw : BinaryWriter = null
    let mutable c_w = null
    let mutable c_id = (Guid.NewGuid ()).ToByteArray ()
    let c_read fn =
        merge_lock.EnterReadLock ()
        try
            fn ()
        finally
            merge_lock.ExitReadLock ()
    let c_write commit merge =
        lock write_lock
        <| fun _ ->
            let pos = c_st.Position
            merge_lock.EnterReadLock ()
            let res = commit ()
            merge_lock.ExitReadLock ()
            c_bw.Write (c_st.Position - pos)
            c_bw.Write c_id
            c_bw.Flush ()
            merge_lock.EnterWriteLock ()
            try
                merge res
            finally
                merge_lock.ExitWriteLock ()
    let create () =
        c_st <- File.Open (c_path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read)
        c_bw <- new BinaryWriter (c_st)
        c_w <- Writer (c_bw)
        c_bw.Write c_id
        c_bw.Flush ()
    let load () =
        let br = new BinaryReader (c_st)
        let sr = new Reader (br)
        let deld = ResizeArray<string> ()
        let rec follow pos =
            c_st.Seek (pos, SeekOrigin.Begin) |> ignore
            match br.ReadByte () with
            | 6uy ->
                sr.ReadString () |> ignore
                sr.ReadData ()
            | 12uy ->
                sr.ReadString () |> ignore
                let old_pos = br.ReadInt64 ()
                let data_pos = c_st.Position
                let old = follow old_pos
                c_st.Seek (data_pos, SeekOrigin.Begin) |> ignore
                old.Update (sr.ReadData ())
            | 13uy ->
                let ok = sr.ReadString ()
                deld.Add ok
                let nk = sr.ReadString ()
                let old_pos = br.ReadInt64 ()
                follow old_pos
            | _ -> failwith "This transaction is not valid in this context. Corruption."
        c_st.Seek (c_st.Length, SeekOrigin.Begin) |> ignore
        while c_st.Position > 16L do
            c_st.Seek (c_st.Position - 24L, SeekOrigin.Begin) |> ignore
            let len = br.ReadInt64 ()
            let mutable pos = c_st.Position - len - 8L
            c_st.Seek (pos, SeekOrigin.Begin) |> ignore
            match br.ReadByte () with
            | 6uy -> // Set
                let k = sr.ReadString ()
                if not <| records.ContainsKey k && not <| deld.Contains k then do
                    records.Add (k, { Position = pos; Data = sr.ReadData () })
            | 11uy -> // Delete
                let k = sr.ReadString ()
                if not <| records.ContainsKey k && not <| deld.Contains k then do
                    deld.Add k
            | 12uy -> // Update
                let k = sr.ReadString ()
                if not <| records.ContainsKey k && not <| deld.Contains k then do
                    let old_pos = br.ReadInt64 ()
                    let data_pos = c_st.Position
                    let old = follow old_pos
                    c_st.Seek (data_pos, SeekOrigin.Begin) |> ignore
                    records.Add (k, 
                        { Position = pos
                          Data = old.Update (sr.ReadData ()) })
            | 13uy -> // Move
                let ok = sr.ReadString ()
                let nk = sr.ReadString ()
                if not <| records.ContainsKey nk
                   && not <| deld.Contains nk
                   && not <| deld.Contains ok
                   then do
                    let old_pos = br.ReadInt64 ()
                    let old = follow old_pos
                    c_st.Seek (pos + 1L, SeekOrigin.Begin) |> ignore
                    records.Add (nk, { Position = pos; Data = old })
                    if not <| deld.Contains ok then do deld.Add ok
            | 18uy -> // Empty
                pos <- 16L
            | _ ->
                failwith "Unknown transaction. Corruption."
            c_st.Seek (pos, SeekOrigin.Begin) |> ignore
        c_st.Close ()
        c_st <- File.Open (c_path, FileMode.Open, FileAccess.Write, FileShare.Read)
        c_bw <- new BinaryWriter (c_st)
        c_w <- Writer (c_bw)
        c_st.Seek (c_st.Length, SeekOrigin.Begin) |> ignore
    let recover () =
        failwith "Recovery not implemented."
    let reattach () =
        c_st <- File.Open (c_path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
        let br = new BinaryReader (c_st)
        c_st.Seek (0L, SeekOrigin.Begin) |> ignore
        c_id <- br.ReadBytes 16
        c_st.Seek (c_st.Length - 16L, SeekOrigin.Begin) |> ignore
        let last = br.ReadBytes 16
        if last = c_id then do
            load ()
        else
            recover ()
    do
        if File.Exists c_path then do
            reattach ()
        else do
            create ()
    member self.ClusterDetach () =
        lock write_lock
        |> fun _ ->
            c_st.Close ()
    member self.ClusterDelete () =
        lock write_lock
        |> fun _ ->
            c_st.Close ()
            File.Delete c_path
    member self.ClusterMove n =
        lock write_lock
        |> fun _ ->
            c_st.Close ()
            let new_path = Path.Combine (path, n + ".mt-c")
            File.Move (path, new_path)
            c_path <- new_path
            c_st <- File.Open (c_path, FileMode.Append, FileAccess.ReadWrite, FileShare.Read)
    member self.Get k =
        c_read
        <| fun _ ->
            if records.ContainsKey k then
                Some { Key = k; Data = records.[k].Data }
            else
                None
    member self.Set r =
        c_write
        <| fun _ ->
            let pos = c_st.Position
            c_bw.Write 6uy
            c_w.WriteRecord r
            pos
        <| fun pos ->
            let cr = { Position = pos; Data = r.Data }
            if records.ContainsKey r.Key then do
                records.[r.Key] <- cr
            else do
                records.Add (r.Key, cr)
    member self.Has k =
        c_read
        <| fun _ ->
            records.ContainsKey k
    member self.Head k =
        c_read
        <| fun _ ->
            if records.ContainsKey k then
                let r = records.[k]
                Some {
                    Key = k
                    Type = r.Data.Type
                }
            else
                None
    member self.Count () =
        c_read <| fun _ -> records.Count
    member self.Delete k =
        c_write
        <| fun _ ->
            if records.ContainsKey k then do
                c_bw.Write 11uy
                c_w.WriteString k
        <| fun _ ->
            if records.ContainsKey k then do
                records.Remove k |> ignore
    member self.Update (r : Record) =
        c_write
        <| fun _ ->
            if not <| records.ContainsKey r.Key then failwith "Record not found."
            let pos = c_st.Position
            c_bw.Write 12uy
            c_w.WriteString r.Key
            c_bw.Write records.[r.Key].Position
            c_w.WriteData r.Data
            pos
        <| fun pos ->
            records.[r.Key] <- { Position = pos; Data = records.[r.Key].Data.Update r.Data }
    member self.Move k nk =
        c_write
        <| fun _ ->
            if not <| records.ContainsKey k then do failwith "Record not found."
            let r = records.[k]
            let pos = c_st.Position
            c_bw.Write 13uy
            c_w.WriteString k
            c_w.WriteString nk
            c_bw.Write r.Position
            pos
        <| fun pos ->
            let r = records.[k]
            records.Remove k |> ignore
            let nr = { Position = pos; Data = r.Data }
            if records.ContainsKey nk then do
                records.[nk] <- nr
            else do
                records.Add (nk, nr)
    member self.Dir () =
        c_read
        <| fun _ ->
            [ for r in records -> { Key = r.Key; Type = r.Value.Data.Type } ]
    member self.All () =
        c_read
        <| fun _ ->
            [ for r in records -> { Key = r.Key; Data = r.Value.Data } ]
    member self.Many ks =
        c_read
        <| fun _ ->
            let vks = ks |> List.filter (fun k -> records.ContainsKey k)
            [ for k in vks -> { Key = k; Data = records.[k].Data } ]
    member self.SetMany (rs : Record list) =
        c_write
        <| fun _ ->
            [ for r in rs do
                let pos = c_st.Position
                c_bw.Write 6uy
                c_w.WriteRecord r
                yield r.Key, { Position = pos; Data = r.Data }
            ]
        <| fun crs ->
            for k,cr in crs do
                if records.ContainsKey k then do
                    records.[k] <- cr
                else do
                    records.Add (k, cr)
    member self.Empty () =
        c_write
        <| fun _ -> c_bw.Write 18uy
        <| fun _ ->
            let c = records.Count
            records.Clear ()
            c
