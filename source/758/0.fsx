open System.Linq
let n, m= 4, 4
let field = [|"*..."; "...."; ".*.."; "...."|]
let s = Array2D.init n m (fun y x -> field.[y].[x])
// Tweet
s|>Array2D.mapi(fun x y->function '*'->'*'|_->'0'+char([for i in 0..8->x+i%3-1,y+i/3-1].Count(fun(x,y)->x>=0&&x<n&&y>=0&&y<m&&s.[x,y]='*')))
