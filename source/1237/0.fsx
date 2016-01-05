(* A way to trigger events given a DateTime instance and a trigger behavior.
   It allows you to trigger something every month for example *)

open System

type TriggerBehavior = 
| OneTime
| EveryMinute
| EveryHour
| EveryDay
| EveryWeek
| EveryMonth
| EveryYear

let rec Check (time : DateTime, triggerBehavior) =
    let now = DateTime.Now    
    match triggerBehavior with
    | EveryMinute -> true
    | EveryHour when now.Minute = time.Minute -> Check(time, EveryMinute)
    | EveryDay when now.Hour = time.Hour -> Check(time, EveryHour)
    | EveryWeek when now.DayOfWeek = time.DayOfWeek -> Check(time, EveryDay)
    | EveryMonth when now.Day = time.Day -> Check(time, EveryDay)
    | EveryYear when now.Month = time.Month -> Check(time, EveryMonth)
    | OneTime when abs((now - time).TotalMinutes) <= 1.0 -> true
    | _ -> false
   