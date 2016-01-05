module OAuthVerify
//verifies the OAuth SWT (simple web token) issued by Azure ACS
//The SWT may be obtained by many methods; one way is:
// - "How to: Request a Token from ACS via the OAuth WRAP Protocol"
//   (http://msdn.microsoft.com/en-us/library/windowsazure/hh674475.aspx)
//(Note I used the userid/password method to obtain the token on behalf of a 'service identity' set up in ACS)
//The token is normally verifed by a 'relying party' such as an ASP.Net website hosting a Web API
//General ACS documentation is here: http://msdn.microsoft.com/en-us/library/gg429788.aspx
open System
open System.Web
open System.Text
open System.Security.Cryptography

//the shared symmetric key for the Relying Party that is setup in Azure ACS. 
//ACS signs the token with this key
let private key = "YHgUsjzZ618nWrh76p8S1kY7taOSJw2tebA9fcRqUfo=" 

let split cs (s:string) = s.Split(cs |> Seq.toArray)
let join sep (xs:string array) = String.Join(sep,xs)
let epoch = DateTime (1970,1,1,0,0,0,  DateTimeKind .Utc)

let validate oauthSWT = 
    let signedParts =
        oauthSWT
        |> HttpUtility.UrlDecode
        |> split ['&']
        |> Array.rev
        |> Seq.skip 1
        |> Seq.toArray 
        |> Array.rev
        |> join "&"

    let hmac256Sig =
        oauthSWT
        |> HttpUtility.UrlDecode
        |> split ['&']
        |> Array.rev
        |> Seq.nth 0
        |> split ['=']
        |> Seq.nth 1
        |> HttpUtility.UrlDecode
        |> Convert.FromBase64String

    use hmacVerify = new HMACSHA256(Convert.FromBase64String key)
    let computedSig = hmacVerify.ComputeHash(Encoding.UTF8.GetBytes signedParts)
    if  hmac256Sig <> computedSig then failwith "computed signnature does not match token signature"

    let assertions = signedParts |> split ['&'] |> Array.map (split ['=']) |> Array.map (fun xs -> xs.[0], xs.[1]) |> Map.ofArray
    //check assertions - only expiry is checked here
    let expires = assertions.["ExpiresOn"] |> float 
    let expireTime = epoch.AddSeconds expires
    if DateTime.UtcNow > expireTime then failwith "token is expired"