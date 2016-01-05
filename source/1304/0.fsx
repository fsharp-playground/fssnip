open System.IO
open System.Diagnostics

let RunNodeJS nodePath initial (script:string) =
   let info = ProcessStartInfo(
               fileName = "node.exe",
               Arguments = sprintf "-e \"%s\"" initial,
               RedirectStandardError  = true,
               RedirectStandardOutput = true,
               RedirectStandardInput  = true,
               UseShellExecute        = false,
               CreateNoWindow         = true,
               WorkingDirectory = nodePath)
   use proc = new Process(StartInfo = info)
   proc.Start() |> ignore 
   proc.StandardInput.Write script
   proc.StandardInput.Flush()
   proc.StandardInput.Close()
   let output = proc.StandardOutput.ReadToEnd()
   proc.WaitForExit()
   if proc.ExitCode <> 0 then
      failwith ("The execution of the command failed: \r\n" + proc.StandardError.ReadToEnd())
   output

// usage example: stylus, css preprocessor

let RunStylus = 
   RunNodeJS @"C:\Program Files\nodejs\node_modules\npm\" 
        "var stylus = require('stylus');
         var stdin = process.openStdin();
         stdin.setEncoding('utf8');
         var acc = '';
         stdin.on('data', function (chunk) {
            acc = acc + chunk;
         });
         stdin.on('end', function () {
            stylus(acc, {}).render(function (err, res) {
               if (err) throw err;
               console.log(res);
            });
         });"

RunStylus "
#prompt
   position absolute
   top 150px
   left 50%
   width 200px
   margin-left -(@width / 2)"
|> printf "%s"