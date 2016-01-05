module S3
open System
open System.Text
open System.Web
open System.Net
open System.Security.Cryptography
open System.IO
open System.Xml
open Microsoft.FSharp.Data.TypeProviders
open System.ServiceModel
open System.Threading.Tasks

type s3 = WsdlService<"http://s3.amazonaws.com/doc/2006-03-01/AmazonS3.wsdl">
let aws_key_id ="<your key id>"
let aws_sec = "<your secret>"

let signString (str:string) =
    let key = Encoding.UTF8.GetBytes(aws_sec)
    let alg = HMACSHA1.Create()
    alg.Key <- key
    let sigBytes = Encoding.UTF8.GetBytes(str)
    let hash = alg.ComputeHash(sigBytes)
    let hashStr = Convert.ToBase64String(hash)
    hashStr

let sigStr verb bucket subResource ts =
    let canRes = sprintf "/%s/%s" bucket subResource
    let ts_header = sprintf "x-amz-date:%s" ts
    String.Join("\n",
        [|
            verb
            ""//md5
            ""//content type
            ""//date
            ts_header
            canRes
        |])

let soapSig op = 
    let dtn = DateTime.UtcNow
    let dt = new DateTime(dtn.Year,dtn.Month,dtn.Day,dtn.Hour,dtn.Minute,dtn.Second,1)
    let ts = XmlConvert.ToString(dt,XmlDateTimeSerializationMode.Utc)
    let str = sprintf "AmazonS3%s%s" op ts
    let sigStr = signString str
    sigStr,dt

let AsyncAwaitVoidTask (task : Task) =
    Async.AwaitIAsyncResult(task) |> Async.Ignore

let putAsync (inStream:Stream) bucket subResource =
    async {
            let mthd = "PUT"
            let ts = DateTime.UtcNow.ToString("r")
            let sigStr = sigStr mthd bucket subResource ts
            let hashStr = signString sigStr
            let authHeader = sprintf "AWS %s:%s" aws_key_id hashStr
            let url = sprintf "https://%s.s3.amazonaws.com/%s" bucket subResource
            let wc = HttpWebRequest.CreateHttp(url)
            wc.Method <- mthd
            wc.Headers.Add("x-amz-date",ts)
            wc.Headers.Add(HttpRequestHeader.Authorization,authHeader)
            let! outStream = wc.GetRequestStreamAsync() |> Async.AwaitTask
            do! AsyncAwaitVoidTask (inStream.CopyToAsync(outStream))
            outStream.Flush()
            outStream.Close()
            let resp = wc.GetResponse()
            resp.Close()
        }

let getAsync bucket subResource =
    async {
            let mthd = "GET"
            let ts = DateTime.UtcNow.ToString("r")
            let sigStr = sigStr mthd bucket subResource ts
            let hashStr = signString sigStr
            let authHeader = sprintf "AWS %s:%s" aws_key_id hashStr
            let url = sprintf "https://%s.s3.amazonaws.com/%s" bucket subResource
            let wc = HttpWebRequest.CreateHttp(url)
            wc.Method <- mthd
            wc.Headers.Add("x-amz-date",ts)
            wc.Headers.Add(HttpRequestHeader.Authorization,authHeader)
            let! resp = wc.GetResponseAsync() |> Async.AwaitTask
            return resp
        }

let private runOp op f  =
    let b = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
    b.MaxReceivedMessageSize <- 10000000L
    let epr = new EndpointAddress("https://s3.amazonaws.com/soap")
    use ser = new s3.ServiceTypes.AmazonS3Client(b,epr)
    try
        let sign,ts = soapSig op
        let resp = f ser sign ts
        ser.Close()
        resp
    with ex -> 
        ser.Abort()
        raise ex

let listBucket bucket =
    runOp "ListBucket" <|
        fun ser sign ts -> ser.ListBucket(bucket,null,null,1000,null,aws_key_id,ts,sign,null)

let deleteObj bucket file =
    runOp "DeleteObject" <|
        fun ser sign ts -> ser.DeleteObject(bucket,file,aws_key_id,ts,sign,null)


let downloadRaw bucket file dnldDir = 
    if Directory.Exists dnldDir |> not then
        Directory.CreateDirectory dnldDir |> ignore
    let path = Path.Combine(dnldDir,file)
    async {
        use outStream = File.Create path 
        use! resp = getAsync bucket file
        use inStream = resp.GetResponseStream()
        do! AsyncAwaitVoidTask (inStream.CopyToAsync(outStream))
        printfn "download completed %s" file
        }

