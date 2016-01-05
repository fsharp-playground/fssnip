open System.IO
    type ICommandListener =
        abstract member Execute : unit -> unit
              
    type Application() =
        member x.OpenFile() = 
             printfn "[Application] OpenFile"
        //Implementation
    //Other Methods

    type OpenFileCommand(app: Application) =  //implied constructor
        member x._app = app
        interface ICommandListener with
            member x.Execute() = 
             printfn "[OpenFileCommand] Execute"
             x._app.OpenFile()

    type Widget(listener : ICommandListener) =
        member x._listener = listener
        member x.SendClick() = 
             printfn "[Widget] SendClick"
             x._listener.Execute()
    
let main() = 
    let app = new Application()
    let cmd = new OpenFileCommand(app)    
    let w = new Widget(cmd)
  
    //Simulate a button click
    w.SendClick()
    printfn "done"

main()
