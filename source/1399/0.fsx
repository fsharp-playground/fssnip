open Npgsql
type Email = { email : string ; teamId : int ; teamName : string ;rowId : int ; eid : int}

let c = db.cc "select a.id as id,a.email as email,a.teamid
                   as teamid,coalesce(t.name,'none') as teamname FROM
                   address a left outer join team t on a.teamid = t.id
                   order by a.id desc"
let r=c.ExecuteReader()
let addresses = seq {
                      while r.Read() do
                          yield { eid=r?id ; email = r?email:string ; teamId = 
                                  r?teamId ; teamName = r?teamname ; rowId = 0
                                }
                    } |> Array.ofSeq