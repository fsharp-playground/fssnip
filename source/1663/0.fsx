    let json = JObject.Parse(s)
    let d : System.Collections.Generic.Dictionary<string,List<string>> = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, List<string>>(json["a"])


Error	1	This expression was expected to have type
    Collections.Generic.Dictionary<string,List<string>>    
but here has type
    bool	
