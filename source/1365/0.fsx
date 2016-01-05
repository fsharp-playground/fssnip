let rec infer (ctx : stype list) (u : term) : stype option = maybe {
  match u with
  | App { Left = s; Right = t } -> let! a = infer ctx s
                                   let! b = infer ctx t
                                   let  c = newvar ()
                                   do! unify a (Func (b,c)) |> Option.map ignore
                                   return c
  | Lam { Body = t }            -> let  a = newvar ()
                                   let! b = infer (a :: ctx) t
                                   return Func (a,b)
  | Sym c                       -> return newvar ()
  | Var i                       -> return dict.[i]
}