let generateHash text = 
    let getbytes : (string->byte[]) = System.Text.Encoding.UTF8.GetBytes
    use algorithm = new System.Security.Cryptography.SHA512Managed()
    text |> (getbytes >> algorithm.ComputeHash >> System.Convert.ToBase64String)


//use example:
let id, time, secretkey = "1", System.DateTime.Now.ToString("yyyyMMddhhmmss"), "mysecret"
generateHash (secretkey + id + time)
