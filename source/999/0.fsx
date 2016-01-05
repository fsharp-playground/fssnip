type XRM = Common.XRM.TypeProvider.XrmDataProvider<"http://lonvscrm001t:777/GREENERGY/XRMServices/2011/Organization.svc">

let dc = XRM.GetDataContext()

let VISIT = 
    query { for system in dc.gry_greenergysystem do
            where (system.gry_name = "VISIT")
            select system } |> Seq.head

let drivers = 
    let haulierDrivers = 
        let haulier = 
            query { for haulier in dc.gry_haulier do
                    where (haulier.gry_hauliercode = "Sucklings")
                    select haulier } |> Seq.head
        query { for driverHaulier in dc.gry_driverhaulier do
                where (driverHaulier.gry_haulier_lk.Id = haulier.Id)
                select driverHaulier } 
    [for hd in haulierDrivers ->            
        query{ for driver in dc.gry_driver do
               where (driver.gry_driverid = hd.gry_driver_lk.Id)
               select driver } |> Seq.head  ]

let driverConnections =
    [for driver in drivers ->
        (driver,  
            // find all the driver's objectreference connections
            let objectReferenceRefs =
                query{ for connection in dc.connection do
                       where (connection.record1id.Id = driver.gry_driverid)
                       select connection} 
                       |> Seq.filter( fun conn -> conn.record2id.LogicalName = "gry_objectreference") 
                       |> Seq.toList
            // lookup the objectreference that is linked to visit and return the "referencedas" field 
            // which will show the driver's ID in visit
            objectReferenceRefs |> List.pick( fun orefref -> 
                match query{ for oref in dc.gry_objectreference do
                             where (oref.gry_objectreferenceid = orefref.record2id.Id 
                                    && oref.gry_greenergysystem_lk.Id = VISIT.gry_greenergysystemid )
                             select oref }  |> Seq.toList with 
                | [] -> None | xs -> Some((xs.Head).gry_referencedas))) ]