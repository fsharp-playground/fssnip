open System
open System.IO

type FileUnit = 
    | Empty 
    | FileElem of string * FileAttributes
    | DirectoryElem of string * FileAttributes * FileUnit
    | FDList of seq<FileUnit>


let rec getFiles path =
    match DirectoryInfo(path).Exists with
    | false -> Empty
    | _     -> match DirectoryInfo(path).Attributes &&& FileAttributes.Directory with
               | FileAttributes.Directory -> let dflst = 
                                                        match Directory.GetFiles(path).Length, Directory.GetDirectories(path).Length with
                                                        |0, 0 -> Empty
                                                        |1, 0 -> let tpath = Directory.GetFiles(path).[0]
                                                                 FileElem(tpath, DirectoryInfo(tpath).Attributes)
                                                        |0, 1 -> let tpath = Directory.GetDirectories(path).[0]
                                                                 tpath |> getFiles
                                                        |_, _ -> FDList(seq{for elem in Directory.GetFiles(path) do
                                                                                yield FileElem(elem, DirectoryInfo(elem).Attributes)
                                                                            for elem in Directory.GetDirectories(path) do
                                                                                yield elem |> getFiles})
                                             DirectoryElem(path, DirectoryInfo(path).Attributes, dflst)
               | _ -> FileElem(path, DirectoryInfo(path).Attributes)

let rec copyfile (sourcepath:string) (destpath:string) ignorelst plst= 
    match plst with
    |Empty -> ()
    |FileElem(path, _) -> File.Copy(path, path.Replace(sourcepath, destpath), true)
    |DirectoryElem(path, _, lst) ->
                 if (not (Seq.exists (fun elem -> elem = DirectoryInfo(path).Name.ToLower()) ignorelst)) then
                    Directory.CreateDirectory(path.Replace(sourcepath, destpath)) |> ignore
                    copyfile sourcepath destpath ignorelst lst
    |FDList lst -> for elem in lst do
                       copyfile sourcepath destpath ignorelst elem

let backupfile (sourcepath:string) (destpath:string) ignorelst=
    getFiles sourcepath
    |> copyfile sourcepath destpath ignorelst

let auto_backup() =
    let Today = DateTime.Now.Date
    let Dest =  "d:/Backup/" + String.Format("{0}-{1}-{2}", Today.Year, Today.Month, Today.Day)
    Console.WriteLine("{0}", Dest.ToString())
    backupfile "d:/Some" Dest ["database"]