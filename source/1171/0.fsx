open System

open System.Runtime.InteropServices
module Win32 =
  [<DllImport "user32.dll">] 
  extern IntPtr FindWindow(string lpClassName,string lpWindowName)
  [<DllImport "user32.dll">] 
  extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName , string windowTitle)
  [<DllImport("user32.dll", CharSet=CharSet.Auto)>]
  extern IntPtr SendMessage(IntPtr hWnd, uint32 Msg, IntPtr wParam , IntPtr lParam)


let getChromeMainWindowUrl () =
  let ps = Diagnostics.Process.GetProcessesByName "chrome"
  seq { 
  for p in ps do
    let mainWnd = Win32.FindWindow("Chrome_WidgetWin_1", p.MainWindowTitle)
    let addrBar = Win32.FindWindowEx(mainWnd, 0n, "Chrome_OmniboxView", null)
    if addrBar <> 0n then
      let url = Marshal.AllocHGlobal 100
      let WM_GETTEXT = 0x000Du
      Win32.SendMessage (addrBar, WM_GETTEXT, 50n, url) |> ignore
      let url = Marshal.PtrToStringUni url
      yield url } 
  |> Seq.head 