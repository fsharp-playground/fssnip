let connect conStr = 
    printfn "connect to database: %s" conStr
let query queryStr = 
    printfn "query database %s" queryStr
let disconnect ()  = 
    printfn "disconnect"
type Template(connF, queryF, disconnF) =     
    member this.Execute(conStr, queryStr) = 
        this.TemplateF conStr queryStr
    member this.TemplateF 
        with get() = 
            let f conStr queryStr = 
                connF conStr
                queryF queryStr
                disconnF ()
            f

let template() = 
    let s = Template(connect, query, disconnect)    
    s.Execute("<connection string>", "select * from tableA")

template()