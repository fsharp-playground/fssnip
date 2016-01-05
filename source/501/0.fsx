open System
open System.Xml
open System.Xml.XPath


let ToDateTime str =
  match DateTime.TryParse str with
    | true, date -> date
    | false, _ -> DateTime.MinValue

// Select nodes.
let inline (+/) (nav: XPathNavigator) (path: string) =
  let iter = nav.Select(path)
  seq { while iter.MoveNext() do yield iter.Current }

// Select a single node.
let inline (+//) (nav: XPathNavigator) (path: string) =
  nav.SelectSingleNode(path)

// Get the value of a node.
let inline (+//>) (nav: XPathNavigator) (path: string) =
  nav.SelectSingleNode(path).Value

// Get the value of specified attribute for the current node.
let inline (+//>>)  (nav: XPathNavigator) (attr: string) =
  nav.GetAttribute(attr, String.Empty)

let parseAtomEntry xnav =
  let title = xnav +//> @"entry/title"
  let url = xnav +// @"entry/link" +//>> "href"
  let updated = xnav +//> @"entry/updated" |> ToDateTime
  let formType = xnav +// @"entry/category" +//>> "term"
  let id = xnav +//> "entry/id"
  { Title = title
    Url = url
    Updated = updated
    FormType = "4"
    Id = id }

let parseAtomFeed feed =
  let entries =
    feed +/ @"/feed/entry"
    |> Seq.map parseAtomEntry
    |> List.ofSeq

  ()