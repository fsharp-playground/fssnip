let IsPalindrom (str:string)=
  let rec fn(a,b)=a>b||str.[a]=str.[b]&&fn(a+1,b-1)
  fn(0,str.Length-1)