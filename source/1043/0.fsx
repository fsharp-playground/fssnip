let rec deleteFiles srcPath pattern includeSubDirs =
    
    if not <| System.IO.Directory.Exists(srcPath) then
        let msg = System.String.Format("Source directory does not exist or could not be found: {0}", srcPath)
        raise (System.IO.DirectoryNotFoundException(msg))

    for file in System.IO.Directory.EnumerateFiles(srcPath, pattern) do
        let tempPath = System.IO.Path.Combine(srcPath, file)
        System.IO.File.Delete(tempPath)

    if includeSubDirs then
        let srcDir = new System.IO.DirectoryInfo(srcPath)
        for subdir in srcDir.GetDirectories() do
            deleteFiles subdir.FullName pattern includeSubDirs
