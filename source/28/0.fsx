open System

/// Generates a friendly string describing a date relatively to the current
/// date and time. The function returns strings like "X mins ago" (or secs, 
/// hours, days, months, years) or "yesterday". It is not completely precise
/// (e.g. doesn't take leap years into account)
let formatFriendlyDate (dt:DateTime) = 
  let ts = DateTime.UtcNow - dt
  if ts.TotalSeconds < 60.0 then sprintf "%d secs ago" (int ts.TotalSeconds)
  elif ts.TotalMinutes < 60.0 then sprintf "%d mins ago" (int ts.TotalMinutes)
  elif ts.TotalHours < 24.0 then sprintf "%d hours ago" (int ts.TotalHours)
  elif ts.TotalHours < 48.0 then sprintf "yesterday"
  elif ts.TotalDays < 30.0 then sprintf "%d days ago" (int ts.TotalDays)
  elif ts.TotalDays < 365.0 then sprintf "%d months ago" (int ts.TotalDays / 30)
  else sprintf "%d years ago" (int ts.TotalDays / 365)