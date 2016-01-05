let calc t =
    let (h, m) = t
    let conversion = abs(h - m) * 5.0 * 6.0

    if conversion  > 180.0
        then 360.0 - conversion
    else
        conversion

(12.0, 0.0) |> calc // 12:00 - should be 0
(12.0, 1.0) |> calc // 12:05 - should be 30
(12.0, 2.0) |> calc // 12:10 - should be 60
(12.0, 3.0) |> calc // 12:15 - should be 90
(12.0, 4.0) |> calc // 12:20 - should be 120
(12.0, 5.0) |> calc // 12:25 - should be 150
(12.0, 6.0) |> calc // 12:30 - should be 180
(12.0, 7.0) |> calc // 12:35 - should be 150
(12.0, 8.0) |> calc // 12:40 - should be 120
(12.0, 9.0) |> calc // 12:45 - should be 90
(12.0, 10.0) |> calc // 12:50 - should be 60
(12.0, 11.0) |> calc // 12:55 - should be 30
(12.0, 12.0) |> calc // 12:00 - should be 0