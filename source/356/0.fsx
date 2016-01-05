let is_palindrome n = n = int(new string(n.ToString().ToCharArray() |> Array.rev))

let is_divisible_by_3_dig_num x = [100..999] |> Seq.exists(fun a -> x%a = 0)

let rec find_num x = if (is_palindrome x && is_divisible_by_3_dig_num x) then Some(x)
                     elif x > (100*100) then find_num(x-1)
                     else None

(999*999) |> find_num |> printfn "%A"