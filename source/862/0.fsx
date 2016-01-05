module Configuration

open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.Runtime.InteropServices

module kernel =
    [<DllImport(@"kernel32")>]
    extern int64 WritePrivateProfileString(string section, string key, string v, string filePath);
    [<DllImport("kernel32")>]
    extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

type public IniFile(iNIPath : string) =
    let mutable path = iNIPath
    member X.Path
        with get() = path
        and set v = path <- v
    member X.IniWriteValue section key value =
        kernel.WritePrivateProfileString(section, key, value, X.Path)
    member X.IniReadValue section key =
        let temp = new StringBuilder(255)
        let i = kernel.GetPrivateProfileString(section, key, "", temp, 255, X.Path)
        temp.ToString()