let stringRepeat n input =
  if System.String.IsNullOrEmpty input then
    input

  else
    let result = new System.Text.StringBuilder(input.Length * n)
    result.Insert(0, input, n).ToString()

"-" |> stringRepeat 10