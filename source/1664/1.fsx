let inline palindrome l = List.rev l = l

[1; 2; 3; 4; 5] |> palindrome = false
["r"; "a"; "c"; "e"; "c"; "a"; "r"] |> palindrome  = true
[1; 1; 3; 3; 1; 1] |> palindrome = true