// see example at the bottom
module dgml

open System
open System.Collections.Generic
open System.Xml
open System.Xml.Serialization

type Graph() = 
    [<DefaultValue>] val mutable public Nodes : Node[]
    [<DefaultValue>] val mutable public Links : Link[]    

and Node() =
    [<XmlAttribute>] member val Id = "" with get, set
    [<XmlAttribute>] member val Label = "" with get, set

and Link()  =
    [<XmlAttribute>] member val Source = "" with get, set
    [<XmlAttribute>] member val Target = "" with get, set
    [<XmlAttribute>] member val Label  = "" with get, set

type DGMLWriter() =
    let Nodes = new List<Node>()
    let Links = new List<Link>()
    member m.AddNode id label = Nodes.Add(new Node(Id=id, Label=label))
    member m.AddLink src  trg  label = Links.Add(new Link(Source=src, Target=trg, Label=label))
    member m.Write (filename : string) =
        let g = Graph(Nodes=Nodes.ToArray(), Links=Links.ToArray())
        let root = new XmlRootAttribute("DirectedGraph")
        root.Namespace <- "http://schemas.microsoft.com/vs/2009/dgml"
        let serializer = new XmlSerializer(typeof<Graph>, root)
        let settings = new XmlWriterSettings(Indent=true)
        use xmlWriter = XmlWriter.Create(filename, settings)
        serializer.Serialize(xmlWriter, g);

// F# translation of http://stackoverflow.com/questions/8199600/c-sharp-directed-graph-generating-library

// create a graph and write it as dgml. Open the graph with Visual Studio for Visualization. 
let w = DGMLWriter()
w.AddNode "bugs" "bugs bunny"
w.AddNode "babsi" "babsi bunny"
w.AddNode "toons" "loony toons"
w.AddLink "toons" "bugs" "contains"
w.AddLink "toons" "babsi" "contains"
w.Write "m:/a.dgml"
