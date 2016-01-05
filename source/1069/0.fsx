// [snippet:Implementation]
// We need to disable some warnings, because the programming style is a bit tricky
#nowarn "686"   // specifying type parameters explicitly for createElement
#nowarn "20"    // ignoring returned value
#r "System.Xml.Linq.dll"

open System.Xml.Linq
open System.Collections.Generic

let xn s = XName.Get(s)

// Various types of HTML elements are represented as classes 

// This is used only for specifying content templates for
// placeholders when creating parameterized views
// See also: 'h.content' in the last example
type ContentTemplate() =
  member x.Zero() = ()
  member x.Delay(f : unit -> unit) = f

// This represents any HTML element that we're creating
// HTML elements that do not allow content (e.g. <br />, <link ... />)
// should inherit from this type
type Element() =
  let mutable name : string = ""
  let mutable pg : Page = Unchecked.defaultof<_>
  let mutable el : XElement = null

  member x.Page = pg
  member x.Init(n, p, e) =
    name <- n; pg <- p; el <- e 
  member x.AddAttr n v =
    el.Add(XAttribute(xn n, v))

// Represents HTML element with default attributes (id, style, class)
and ElementDef() = 
  inherit Element()
  member x.set(?id:string, ?style:string, ?cssclass:string) = 
    id |> Option.iter (x.AddAttr "id")
    style |> Option.iter (x.AddAttr "style")
    cssclass |> Option.iter (x.AddAttr "class")

// Represents HTML element <link> with some other required attributes
and ElementLink() = 
  inherit Element()
  member x.set(rel:string, href:string, typ:string, ?id:string, ?style:string, ?cssclass:string) = 
    x.AddAttr "rel" rel
    x.AddAttr "href" href
    x.AddAttr "type" typ
    id |> Option.iter (x.AddAttr "id")
    style |> Option.iter (x.AddAttr "style")
    cssclass |> Option.iter (x.AddAttr "class")

// TODO: We need to add other types of HTML elements...

// This represents HTML element that can contain some other elements (e.g. <div>)
// It is written as a computation builder, so we can place the content in curly braces
// All HTML elements that can contain content should inherit from this
and Container() =
  inherit Element()
  member x.Zero() = ()
  member x.Run(c) = x.Page.PopStack(); c
  member x.For(sq, b) = for e in sq do b e

// Default HTML element that can contain content (provides basic attributes only)
and ContainerDef() = 
  inherit Container()
  member x.set(?id:string, ?style:string, ?cssclass:string) = 
    id |> Option.iter (x.AddAttr "id")
    style |> Option.iter (x.AddAttr "style")
    cssclass |> Option.iter (x.AddAttr "class")
    x

// Represents HTML element <a> with requried attribute 'href'
and ContainerA() = 
  inherit Container()
  member x.set(href:string, ?id:string, ?style:string, ?cssclass:string) = 
    x.AddAttr "href" href
    id |> Option.iter (x.AddAttr "id")
    style |> Option.iter (x.AddAttr "style")
    cssclass |> Option.iter (x.AddAttr "class")
    x

// TODO: We need to add other content HTML elements here...

// This is the main computation builder that represents Page. It also stores
// all the state as the computation builders run and produce our HTML.
and Page() as this =
  let root = new XDocument()
  let mutable current = root :> XContainer
  let stack = new Stack<_>()

  // Creates element that cannot contain content
  let createElement name = 
    let el = new XElement(xn name)
    current.Add(el)
    let res = new 'T()
    (res :> Element).Init(name, this, el)
    res

  // Creataes element that can contain content
  let createContainerElement name = 
    let el = new XElement(xn name)
    current.Add(el)
    stack.Push(current)
    current <- el
    let res = new 'T()
    (res :> Container).Init(name, this, el)
    res

  member (* friend Container *) x.Current = current
  member (* friend Container *) x.PopStack() = current <- stack.Pop()

  // Returns the constructed XML document (at the end of execution)
  member x.Document = root

  // Various members for creating html elements 
  member x.title = createContainerElement<ContainerDef> "title"
  member x.html = createContainerElement<ContainerDef> "html"
  member x.link = createElement<ElementLink> "html" 
  member x.head = createContainerElement<ContainerDef> "head"
  member x.h1 = createContainerElement<ContainerDef> "h1"
  member x.h2 = createContainerElement<ContainerDef> "h2"
  member x.ul = createContainerElement<ContainerDef> "ul"
  member x.li = createContainerElement<ContainerDef> "li"
  member x.strong = createContainerElement<ContainerDef> "strong"
  member x.p = createContainerElement<ContainerDef> "p"
  member x.div = createContainerElement<ContainerDef> "div"
  member x.a = createContainerElement<ContainerA> "a"
  member x.hr = createElement<ElementDef> "hr"

  // Used for creating text content (called by the % operator)
  member x.text(str:string) = current.Add(str)
  // Used for creating content templates - to be passed as 
  // arguments to a parameterized tempalte
  member x.content = new ContentTemplate()


// Initialize global value of the Page (yes, it has to be global) and helper
// operator '%' for creating text elements in the document

let h = Page()
let (~%) str = h.text(str)

// Just a shortcut to make the syntax more succinct
let fm = sprintf

// Master template taking 'title' and 'content' as arguments

let masterTemplate title content = 
  h.html {
    h.head {
      h.title { 
        title()
        %" - View Engine Sample" 
      }
      h.link.set(typ="text/css", rel="stylesheet", href="/styles/main.css")
    }
    h.h1 { title() }
    h.hr.set(cssclass="heading")
    content()
    h.hr
    h.div.set(id="footer") {
      %"This is an example of using"
      h.a.set(href="http://fsharp.net") { %"the amazing F# language" }
      % @"for writing a simple an elegant view engine 
          for the ASP.NET MVC framework"
    }
  }
// [/snippet]


// [snippet:Examples]
// A page that reads products and displays them using the 'masterTemplate'

let products = 
  [ "Tea", 2.3M; 
    "Coffee", 5.0M; 
    "Lemonade", 1.5M ]

masterTemplate
  (h.content { % "Product Listing" })
  (h.content {
    h.div { 
      h.ul.set(cssclass="listing") {
        for name, price in products do
          h.li { 
            h.strong { % name }
            % fm " - Price: $%f" price }
      }
    }
  })

// Prints the constructed XML
h.Document.ToString()
// [/snippet]