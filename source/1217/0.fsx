//BNF-like for XML:
//XmlRoot         := XmlElement
//XmlElement      := '<' Name ' ' Attribute* '>' (XmlElement* | XmlValueContent) '<\' Name '>' | 
//                   '<' Name ' ' Attribute* '\>'
//Attribute       := Name '=' '"' Value '"'
//Value           := any char except "
//XmlValueContent := any char except <
//Name            := A-Za-z_[A-Za-z0-9_-]*

open System

[<CustomEquality; NoComparison>]
type XmlAttribute = 
    {Name : string; Value : string}
    override this.Equals(that) =
        match that with
        | :? XmlAttribute as other -> this.Name = other.Name
        | _ -> false
    override this.GetHashCode() = hash this.Name

type XmlElementBody =
    | StrValue of string
    | XmlValue of XmlElement array
and
    XmlElement   = {Name : string; Attributes : XmlAttribute array; Body : XmlElementBody}

let NullXmlAttribute = {Name = null; Value = null}

let NullXmlElement = {Name = null; Attributes = null; Body = XmlValue null}

let readWhitespace (s : string, idx) =
    let mutable idx1 = idx
    while idx1 < s.Length && (s.[idx1] = ' ' || s.[idx1] = '\r' || s.[idx1] = '\n' || s.[idx1] = '\t') do
        idx1 <- idx1 + 1
    (s, idx1)

let subString idxDelta (s : string, idx) =
    if idxDelta > idx
        then (s.Substring(idx, idxDelta - idx), (s, idxDelta))
        else (null, (s, idx))

let readName (name : string byref) (s : string, idx) =
    let checkValidChars (idx1) = 
        ('_' = s.[idx1]) || ('A' <= s.[idx1] && s.[idx1] <= 'Z') || ('a' <= s.[idx1] && s.[idx1] <= 'z')
    let mutable idx1 = idx
    if idx1 < s.Length && checkValidChars idx1
        then idx1 <- idx1 + 1
        else failwith (sprintf "Invalid Name at %d" idx1) 
    while idx1 < s.Length && (checkValidChars idx1 || ('0' <= s.[idx1] && s.[idx1] <= '9') || s.[idx1] = '-')
                do idx1 <- idx1 + 1
    let (name_, retVal) = subString idx1 (s, idx)
    name <- name_; retVal

let readXmlValueContent (content : string byref) (s : string, idx) =
    let mutable idx1 = idx
    while idx1 < s.Length && '<' <> s.[idx1] do idx1 <- idx1 + 1
    let (content_, retVal) = subString idx1 (s, idx)
    content <- content_; retVal

let readValue (value : string byref) (s : string, idx) =
    let mutable idx1 = idx
    while idx1 < s.Length && '\"' <> s.[idx1] do idx1 <- idx1 + 1
    let (value_, retVal) = subString idx1 (s, idx)
    value <- value_; retVal

let readFixed ch (s : string, idx) =
    if s.[idx] <> ch then failwith (sprintf "Invalid token at %d" idx)
    (s, idx + 1)

let checkFixed ch (s : string, idx) = s.[idx] = ch

let readAttribute (attr : XmlAttribute byref) (s : string, idx) =
    let mutable name : string = null
    let (_, idx1) = (s, idx) |> readName &name
    if name <> null then
        let mutable value : string = null
        let (_, idx2) = (s, idx1) |> readWhitespace |> readFixed '=' |> readWhitespace |>
                                     readFixed '\"' |> readValue &value |> readFixed '\"'
        attr <- {XmlAttribute.Name = name; Value = value}; (s, idx2)
    else
        attr <- NullXmlAttribute; (s, idx1)

let readAttributeList (attrs : XmlAttribute array byref) (s : string, idx) =
    let (_, idx1) = (s, idx) |> readWhitespace
    let mutable a : XmlAttribute list = []
    let mutable idx2 = idx1
    while (not (checkFixed '>' (s, idx2))) && (not (checkFixed '/' (s, idx2))) do
        let mutable attr = NullXmlAttribute
        let (_, idx3) = (s, idx2) |> readAttribute &attr |> readWhitespace
        if attr.Name <> null then a <- attr :: a
        if idx2 = idx3 then failwith (sprintf "Malformed XML at %d" idx2)
        idx2 <- idx3
    if a |> List.ofSeq |> Seq.distinct |> Seq.length <> (a |> List.length) then
        failwith "Attribute names must be unique"
    attrs <- a |> List.rev |> List.toArray; (s, idx2)

let readClosingElement (name : string byref) (s : string, idx) =
    let (_, idx1) = (s, idx) |> readFixed '<' |> readFixed '/' |> readWhitespace |>
                                readName &name |> readWhitespace |> readFixed '>' |> readWhitespace
    (s, idx1)

let rec readElement (elem : XmlElement byref) (s : string, idx) =
    let mutable name : string = null
    let mutable attrs : XmlAttribute array = null
    let (_, idx1) = (s, idx) |> readWhitespace |> readFixed '<' |> readName &name |>
                                readWhitespace |> readAttributeList &attrs |> readWhitespace
    if checkFixed '/' (s, idx1) then //'<' Name ' ' Attribute* '\>'
        let (_, idx2) = (s, idx1) |> readFixed '/' |> readFixed '>'
        elem <- {XmlElement.Name = name; Attributes = attrs; Body = StrValue String.Empty}; (s, idx2) //return
    else //'<' Name ' ' Attribute* '>' (XmlElement* | XmlValueContent) '<\' Name '>'
        let (_, idx2) = (s, idx1) |> readFixed '>' |> readWhitespace
        let (e, idx3) =
            if checkFixed '<' (s, idx2) then //XmlElement*
                if checkFixed '/' (s, idx2 + 1) then //no xml value content
                    (XmlValue [||], idx2)
                else //element list
                    let mutable elems : XmlElement array = null
                    let (_, idx3) = (s, idx2) |> readElementList &elems
                    (XmlValue elems, idx3)
            else //XmlValueContent
                let mutable content : string = null
                let (_, idx3) = (s, idx2) |> readXmlValueContent &content
                (StrValue content, idx3)
        let mutable name_ : string = null
        let (_, idx4) = (s, idx3) |> readClosingElement &name_
        if name <> name_ then failwith (sprintf "Closing element <%s> is missing" name)
        elem <- {XmlElement.Name = name; Attributes = attrs; Body = e}; (s, idx4) //return
and
    readElementList (elems : XmlElement array byref) (s : string, idx) =
        let mutable a : XmlElement list = []
        let mutable idx1 = idx
        while checkFixed '<' (s, idx1) && (not (checkFixed '/' (s, idx1 + 1))) do
            let mutable e : XmlElement = NullXmlElement
            let (_, idx2) = (s, idx1) |> readElement &e |> readWhitespace
            idx1 <- idx2
            a <-  e :: a
        elems <- a |> List.rev |> List.toArray; (s, idx1)

let readXmlFragment (s: string) =
    let mutable idx = 0
    let mutable a = []
    while (idx < s.Length) do
        let mutable e : XmlElement = NullXmlElement
        let (_, idx1) = (s, idx) |> readElement &e
        idx <- idx1
        a <- e :: a
    a |> List.rev |> List.toArray

let readXmlRoot (s: string) = readXmlFragment(s).[0]

//helper:
let print (node: XmlElement) =
    let printAttributes elem =
        for a in elem.Attributes do printf " @%s: %s" a.Name a.Value
    let rec rec_print (node: XmlElement, tab : int) =
        for i = 1 to tab do printf "  "
        printf "{%s: " node.Name
        match node.Body with
        | StrValue x -> 
            printf "%s" x
        | XmlValue xs -> 
            for x in xs do printfn ""; rec_print(x, tab + 1)
        printAttributes node
        printf "}"
    rec_print (node, 0); printfn ""

let printn node = print node; printfn ""

//test cases:
let test1 = "<note>
             <to>Tove</to>
             <from>Jani</from>
             <heading>Reminder</heading>
             <body>Don't forget me this weekend!</body>
            </note>"
let result1 = readXmlRoot(test1)
printn(result1)

let test2 = "<A>
                <B/>
                <C/>
                <D>
                    <E/>
                    <F/>
                    <G>
                        <H><I></I></H>
                    </G>
                    <J/>
                </D>
            </A>"
let result2 = readXmlRoot(test2)
printn(result2)

let test3 = "<bookstore>
                  <book category=\"COOKING\">
                    <title lang=\"en\">Everyday Italian</title>
                    <author>Giada De Laurentiis</author>
                    <year>2005</year>
                    <price>30.00</price>
                  </book>
                  <book category=\"CHILDREN\">
                    <title lang=\"en\">Harry Potter</title>
                    <author>J K. Rowling</author>
                    <year>2005</year>
                    <price>29.99</price>
                  </book>
                  <book category=\"WEB\">
                    <title lang=\"en\">Learning XML</title>
                    <author>Erik T. Ray</author>
                    <year>2003</year>
                    <price>39.95</price>
                  </book>
             </bookstore>"
let result3 = readXmlRoot(test3)
printn(result3)

let test4 = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">
                <SignedInfo>
                  <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />
                  <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />
                  <Reference URI=\"#object\">
                    <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />
                    <DigestValue>OPnpF/ZNLDxJ/I+1F3iHhlmSwgo=</DigestValue>
                  </Reference>
                </SignedInfo>
                <SignatureValue>nihUFQg4mDhLgecvhIcKb9Gz8VRTOlw+adiZOBBXgK4JodEe5aFfCqm8WcRIT8GLLXSk8PsUP4//SsKqUBQkpotcAqQAhtz2v9kCWdoUDnAOtFZkd/CnsZ1sge0ndha40wWDV+nOWyJxkYgicvB8POYtSmldLLepPGMz+J7/Uws=</SignatureValue>
                <KeyInfo>
                  <KeyValue>
                    <RSAKeyValue>
                      <Modulus>4IlzOY3Y9fXoh3Y5f06wBbtTg94Pt6vcfcd1KQ0FLm0S36aGJtTSb6pYKfyX7PqCUQ8wgL6xUJ5GRPEsu9gyz8ZobwfZsGCsvu40CWoT9fcFBZPfXro1Vtlh/xl/yYHm+Gzqh0Bw76xtLHSfLfpVOrmZdwKmSFKMTvNXOFd0V18=</Modulus>
                      <Exponent>AQAB</Exponent>
                    </RSAKeyValue>
                  </KeyValue>
                </KeyInfo>
                <Object Id=\"object\">some text
                  with spaces and CR-LF.</Object>
                </Signature>"
let result4 = readXmlRoot(test4)
printn(result4)

let test5 = "<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"en\">
             <head>
             <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />
             <title>XHTML 1.0 Strict Example</title>
             <script type=\"text/javascript\">             
             function loadpdf() {
                document.getElementById(\"pdf-object\").src=\"http://www.w3.org/TR/xhtml1/xhtml1.pdf\";
             }
             </script>
             </head>
             <body onload=\"loadpdf()\">
             <p>
             <p>This is an example of an Extensible HyperText Markup Language</p>
             <br />
             <img id=\"validation-icon\"
                src=\"http://www.w3.org/Icons/valid-xhtml10\"
                alt=\"Valid XHTML 1.0 Strict\" /><br />
             <object id=\"pdf-object\"
                name=\"pdf-object\"
                type=\"application/pdf\"
                data=\"http://www.w3.org/TR/xhtml1/xhtml1.pdf\"
                width=\"100%\"
                height=\"500\">
             </object>
             </p>
             </body>
            </html>"
let result5 = readXmlRoot(test5)
printn(result5)

Console.ReadLine() |> ignore