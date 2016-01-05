#r "System.Xml.Linq"
open System.Xml.Linq

type Xml = E | A

let (?) v name (content: obj) =
    let name = XName.Get name
    match v with
    | E -> XElement(name, content) :> XObject
    | A -> XAttribute(name, content) :> _

E?Dataset [
    E?Page [
        A?Type "HTML Web Site"
        E?html [
            E?head []
            E?body [
                E?div []
                E?p []
            ]
        ]
    ]
]
|> string