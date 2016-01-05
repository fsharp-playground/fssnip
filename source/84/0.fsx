let REGISTRYSOFTWARE = "Software";
let REGISTRYMYPATH = "MySoftware";

let internal GetRegistryValue key =
    use path1 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGISTRYSOFTWARE)
    match path1 with
    | null -> failwith("Access failed to registry: hklm\\"+REGISTRYSOFTWARE)
    | keyhklmsw -> 
        use path2 = keyhklmsw.OpenSubKey(REGISTRYMYPATH)
        match path2 with
        | null -> failwith("Access failed to registry: " + REGISTRYMYPATH)
        | keyhklmswmypath -> 
            match keyhklmswmypath.GetValue(key, null) with
            | null -> failwith("Path not found: " + key)
            | gotkey -> gotkey

