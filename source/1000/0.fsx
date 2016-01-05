type XRM = Common.XRM.TypeProvider.XrmDataProvider<"http://lonvscrm001t:777/GREENERGY/XRMServices/2011/Organization.svc">

let dc = XRM.GetDataContext()
let time = DateTime.Now

let VISIT = 
    query { for system in dc.gry_greenergysystem do
            where (system.gry_name = "VISIT")
            select system 
            exactlyOne } 

let drivers = 
    let haulier = // find Haulier ID
        query { for haulier in dc.gry_haulier do
                where (haulier.gry_hauliercode = "Sucklings")
                select haulier
                exactlyOne }         
    [for dh in query { for driverHaulier in dc.gry_driverhaulier do // get all driver IDs from the cross reference table
                       where (driverHaulier.gry_haulier_lk.Id = haulier.Id)
                       select driverHaulier }  ->            
                            query { for driver in dc.gry_driver do // load each driver (can be improved)
                                    where (driver.gry_driverid = dh.gry_driver_lk.Id)
                                    select driver
                                    exactlyOne }  ]

// create a list of drivers with their VISIT id
let visitDrivers =
    [for driver in drivers ->
        (driver,  
            // find all the driver's objectreference connections
            query{ for connection in dc.connection do
                    where (connection.record1id.Id = driver.gry_driverid)
                    select connection} 
                    // XRM won't allow you to query this property in the expression tree
                    |> Seq.filter( fun conn -> conn.record2id.LogicalName = "gry_objectreference") 
                     // lookup the objectreference that is linked to visit and return the "referencedas" field which will show the driver's ID in visit            
                    |> Seq.pick( fun orefref ->
                        match query { for oref in dc.gry_objectreference do     
                                      // this could potentially be improved with "In" but it exits as soon as it finds the correct record so would depend on the order.
                                      where (oref.gry_objectreferenceid = orefref.record2id.Id &&
                                             oref.gry_greenergysystem_lk.Id = VISIT.gry_greenergysystemid )
                                      // we only expect one result here but it is possible that it doesn't exist - otherwise exactlyOne would be used
                                      select oref }  |> Seq.toList with 
                        | [] -> None | xs -> Some((xs.Head).gry_referencedas))) ]
