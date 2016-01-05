let rec part = function
  | 1 -> [[1]]
  | n -> [yield [n]
          for i in 1..(n-1) do
            for ls in (part (n-i)) do
              if (List.head ls) <= i then
                yield i::ls]

let rec part' = function
  | 1 -> [[1]]
  | n -> [yield [n]
          for i in 1..(n-1) do
            for ls in (part' (n-i)) do
              ///if (List.head ls) <= i then
                yield i::ls]
  
List.length <| part 5
List.length <| part' 5