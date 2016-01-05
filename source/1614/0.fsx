type tvar = int

let freshSource = ref 0
let fresh () : tvar = 
    let v = !freshSource
    freshSource := !freshSource + 1
    v


type monotype =     | TBool
                    | TArr of monotype * monotype
                    | TVar of tvar

type polytype = PolyType of int list * monotype

type exp = 
        | True
        | False
        | Var of int
        | App of exp * exp
        | Let of exp * exp
        | Fn of exp
        | If of exp * exp * exp

type info = | PolyTypeVar of polytype
            | MonoTypeVar of monotype

type context = info list

// We’ll also need to substitute a type variable for a type.
let rec subst ty' var ty =
    match ty with 
    | TVar var' -> if var = var' then ty' else TVar var'
    | TArr (l, r) -> TArr (subst ty' var l, subst ty' var r)
    | TBool -> TBool

// We also want to be able to find out all the free variables in a type.
let rec freeVars t =
    match t with
    | TVar v -> [v]
    | TArr (l, r) -> freeVars l @ freeVars r
    | TBool -> []

let rec dedup l = 
    match l with
    | [] -> []
    | (x :: xs) ->
        if List.exists (fun y -> x = y) xs
        then dedup xs
        else x :: dedup xs

let generalizeMonoType ctx ty =
    let notMem xs x = List.forall (fun y -> x <> y) xs
    let free m = 
        match m with
        | (MonoTypeVar m) -> freeVars m
        | (PolyTypeVar (PolyType (bs, m))) ->
            List.filter (notMem bs) (freeVars m)

    let ctxVars = List.concat (List.map free ctx)
    let polyVars = List.filter (notMem ctxVars) (freeVars ty)
    PolyType (dedup polyVars, ty)

let mintNewMonoType (PolyType (ls, ty)) =
    List.foldBack (fun v t -> subst (TVar (fresh ())) v t) ls ty 

exception UnboundVar of int
let lookupVar var ctx =
    try match List.nth ctx var with
        | PolyTypeVar pty -> mintNewMonoType pty
        | MonoTypeVar mty -> mty 
    with ex -> raise (UnboundVar var)

let applySol sol ty =
    List.foldBack (fun (v, ty) ty' -> subst ty v ty') sol ty 

let applySolCxt sol cxt =
    let applyInfo i =
        match i with
        | PolyTypeVar (PolyType (bs, m)) ->
                PolyTypeVar (PolyType (bs, (applySol sol m)))
        | MonoTypeVar m -> MonoTypeVar (applySol sol m)
    List.map applyInfo cxt

let addSol v ty sol = (v, applySol sol ty) :: sol

let occursIn v ty = List.exists (fun v' -> v = v') (freeVars ty)

// Not given in the source example
let substConstrs (tv: monotype) (i: tvar) (constrs: (monotype * monotype) list)  : (monotype * monotype) list = failwith "Not given"

exception UnificationError of monotype * monotype
let rec unify csl =
    match csl with
    | [] -> []
    | (c :: constrs) -> 
        match c with
        | (TBool, TBool) -> unify constrs
        | (TVar i, TVar j) ->
            if i = j
            then unify constrs
            else addSol i (TVar j) (unify (substConstrs (TVar j) i constrs))
        | ((TVar i, ty) | (ty, TVar i)) ->
            if occursIn i ty
            then raise (UnificationError c)
            else addSol i ty (unify (substConstrs ty i constrs))
        | (TArr (l, r), TArr (l', r')) ->
            unify ((l, l') :: (r, r') :: constrs)
        | _ -> raise (UnificationError c)

let (<+>) sol1 sol2 =
    let notInSol2 v = List.forall (fun (v', _) -> v <> v') sol2
    let sol1' = List.filter (fun (v, _) -> notInSol2 v) sol1
    List.map (fun (v, ty) -> (v, applySol sol1 ty)) sol2 @ sol1'
    
let rec constrain ctx v =
    match v with 
    | True -> (TBool, [])
    | False -> (TBool, [])
    | Var i -> (lookupVar i ctx, [])
    | Fn body ->
        let argTy = TVar (fresh ())
        let (rTy, sol) = constrain (MonoTypeVar argTy :: ctx) body
        (TArr (applySol sol argTy, rTy), sol) 
    | If (i, t, e) ->
        let (iTy, sol1) = constrain ctx i
        let (tTy, sol2) = constrain (applySolCxt sol1 ctx) t
        let (eTy, sol3) = constrain (applySolCxt (sol1 <+> sol2) ctx) e
        let sol = sol1 <+> sol2 <+> sol3
        let sol = sol <+> unify [ (applySol sol iTy, TBool); (applySol sol tTy, applySol sol eTy)]
        (tTy, sol)
    | App (l, r) ->
        let (domTy, ranTy) = (TVar (fresh ()), TVar (fresh ()))
        let (funTy, sol1) = constrain ctx l
        let (argTy, sol2) = constrain (applySolCxt sol1 ctx) r
        let sol = sol1 <+> sol2
        let sol = sol <+> unify [(applySol sol funTy, applySol sol (TArr (domTy, ranTy)));
                                 (applySol sol argTy, applySol sol domTy)]
        (ranTy, sol)
     | Let (e, body) ->
        let (eTy, sol1) = constrain ctx e
        let ctx' = applySolCxt sol1 ctx
        let eTy' = generalizeMonoType ctx' (applySol sol1 eTy)
        let (rTy, sol2) = constrain (PolyTypeVar eTy' :: ctx') body
        (rTy, sol1 <+> sol2)

let infer e =
    let (ty, sol) = constrain [] e
    generalizeMonoType [] (applySol sol ty) 