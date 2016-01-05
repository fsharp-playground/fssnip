
type XRM = Common.XRM.TypeProvider.XrmDataProvider<"http://xrmserver:777/SQUIRRELS/XRMServices/2011/Organization.svc">
let dc = XRM.GetDataContext()
type testRecord = 
    { x : string; 
      y : string }

let x = { x = "John%"; y = ""}

let q =
    query { for s in dc.new_squirrel do                            
            where (s.new_name <>% x.x || s.new_age = 42)
            for f in s.``N:1 <- new_new_forest_new_squirrel`` do
            for o in f.``N:1 <- owner_new_forest`` do
            where (s.new_colour |=| [|"Pink";"Red"|])
            where (f.new_name = "Sherwood")
            where (o.name = "ROSS THE OWNER!")
            select (s, f.new_name, f, o) } 