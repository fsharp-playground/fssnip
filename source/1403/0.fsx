open FSharp.Date

type Date = DateProvider<epoch=2010>

let today = Date.``2014``.``04``.``26``

let lastSaturdayOfApril =
   Date.``2014``.``04``.Saturday.``26``

let todayWithMonthName =
   Date.``2014``.April.``26``