namespace global

type Int32 private () =
    static member tryParse(s:string,startIndex:int,length:int) =
        let inline isWhiteSpace c = System.Char.IsWhiteSpace c
        let inline isDigit c = System.Char.IsDigit c
        let mutable i = 0
        let inline at i  = s.[startIndex+i]
        while i < length && isWhiteSpace(at i) do i <- i + 1
        if i = length
        then 
            None
        else
            let c = at i
            let sign = 
                if c = '-' then 
                    i <- i + 1
                    -1
                else 
                    if c = '+' then i <- i + 1
                    1
            let mutable acc = 0
            let c = ref '_'
            while i < length && (c := at i; isDigit !c) do
                let digit = int (!c) - int '0'
                acc <- acc * 10 + digit
                i <- i + 1
            while i < length && isWhiteSpace(at i) do
                i <- i + 1
            if i = length 
            then
                Some (acc * sign)
            else
                None
    static member tryParse(s:string) =
        Int32.tryParse(s,0,s.Length)