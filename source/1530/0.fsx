open System
[   "Наличие бомбурий",         ["Да";      "Да";        "Нет";       "Да";    "Нет"]
    "Количество клептиконов",   ["1";       "1";         "0";         "3";     "5"]
    "Цвет велория",             ["Красный"; "Оранжевый"; "Оранжевый"; "—";     "Синий"]
    "Наличие пумпеля",          ["Нет";     "Да";        "Да";        "—";     "—"]
    "Величина пумпеля",         ["—";       "Большой";   "Маленький"; "—";     "—"]
    "Возможность крокотания",   ["Нет";     "Нет";       "—";         "Да";    "Нет"]
    "Возможность бульботания",  ["Нет";     "Да";        "—";         "Да";    "Нет"]
    "Наличие дуков и труков",   ["—";       "—";         "—";         "—";     "Да"]
    "Цвет лемпелей",            ["Жёлтый";  "Жёлтый";    "Жёлтый";    "Белый"; "Белый"]
    "Наличие пильских трапков", ["Да";      "Да";        "Да";        "Да";    "Да"]]
|> List.fold( fun bugs (question, xs) -> 
    if bugs |> List.filter Option.isSome |> List.length < 2 then bugs else
    let replies = 
        Set.ofList xs |> Set.fold( fun s x ->
        sprintf "%s%s\\"%s\\"" s (if String.IsNullOrEmpty s then "" else ", ") x ) ""
    printfn "Введите \\"%s\\". Варианты ответа: %s либо \\"-\\" если Вы не знаете ответ" question replies
    let answer = System.Console.ReadLine()
    if answer="-" then bugs else
        xs |> List.zip bugs |> List.map( function  
            | Some bug, v when v=answer || v="-" -> Some bug
            | _ -> None) ) ( [  "Аурата сетуньская"
                                "Десятилиньята лепая"
                                "Семипунктата Коха"
                                "Популий грыжомельский"
                                "Гортикола филоперьевая" ] |> List.map Some )
|> List.choose (fun x -> x) |> function
| [] -> printfn "ответ не найден"
| [x] -> printfn "Ответ - %s" x
| xs -> 
    printfn "Ответы:"
    xs |> List.iter (printfn "%s")