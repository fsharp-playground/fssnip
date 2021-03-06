open System.Text
open System.IO
open System.Net

// URL of a simple page that takes two HTTP POST parameters. See the
// form that submits there: http://www.snee.com/xml/crud/posttest.html
let url = "http://www.snee.com/xml/crud/posttest.cgi"

// Create & configure HTTP web request
let req = HttpWebRequest.Create(url) :?> HttpWebRequest 
req.ProtocolVersion <- HttpVersion.Version10
req.Method <- "POST"

// Encode body with POST data as array of bytes
let postBytes = Encoding.ASCII.GetBytes("fname=Tomas&lname=Petricek")
req.ContentType <- "application/x-www-form-urlencoded";
req.ContentLength <- int64 postBytes.Length
// Write data to the request
let reqStream = req.GetRequestStream() 
reqStream.Write(postBytes, 0, postBytes.Length);
reqStream.Close()

// Obtain response and download the resulting page 
// (The sample contains the first & last name from POST data)
let resp = req.GetResponse() 
let stream = resp.GetResponseStream() 
let reader = new StreamReader(stream) 
let html = reader.ReadToEnd()