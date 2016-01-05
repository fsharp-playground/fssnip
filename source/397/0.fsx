let clockAngle (h, m) =
    let hourAngle = (float (h%12) + (float m)/60.0) / 12.0
    let minuteAngle = (float m)/60.0

    let diffDeg = abs (hourAngle - minuteAngle) * 360.0

    if diffDeg > 180.0 then 360.0 - diffDeg else diffDeg

clockAngle (12,0) // 12:00 should be 0
clockAngle (12,5) // 12:05 should be 27.5 deg
clockAngle (12,59) // 12:59 should be 35.5 deg
clockAngle (1, 0) // 1:00 should be 30 deg
clockAngle (12, 60) // aka 1:00!! should be 30 deg