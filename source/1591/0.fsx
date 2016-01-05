// [snippet:The combinator]
let inline matchExn (f : 'T -> 'S) (t : 'T) (onSuccess : 'S -> 'R) (onException : exn -> 'R) : 'R =
    let mutable exn : System.Exception = null
    let mutable s = Unchecked.defaultof<'S>
    try s <- f t with e -> exn <- e // e cannot be null
    if obj.ReferenceEquals(exn, null) then onSuccess s
    else
        onException exn

// [/snippet]

// [snippet:Simple example]
let test () =
    matchExn (fun i -> 1 / i) 0
        (fun r -> string r)
        (fun e -> e.Message)

// Codegen (Release build)
//
//public static string test2()
//{
//	Exception ex = null;
//	int r = 0;
//	try
//	{
//		r = 1 / 0;
//	}
//	catch (object arg_0D_0)
//	{
//		Exception ex2 = (Exception)arg_0D_0;
//		ex = ex2;
//	}
//	if (object.ReferenceEquals(ex, null))
//	{
//		return Exn.onSuccess@38-1(r);
//	}
//	return ex.Message;
//}
// [/snippet]

// [snippet: Tail recursive, exception safe iteration]
let countIters (f : int -> bool) =
    let rec aux i =
        matchExn f i
            (function true -> aux (i+1) | false -> i)
            (fun _ -> i)

    aux 0

// Codegen (Release build)
//
//public static int countIters(FSharpFunc<int, bool> f)
//{
//	return Exn.aux@54(f, 0);
//}
//internal static int aux@54(FSharpFunc<int, bool> f, int i)
//{
//	Exception objA = null;
//	bool b = false;
//	try
//	{
//		b = f.Invoke(i);
//	}
//	catch (object arg_11_0)
//	{
//		Exception ex = (Exception)arg_11_0;
//		objA = ex;
//	}
//	if (object.ReferenceEquals(objA, null))
//	{
//		return Exn.onSuccess@38-2(f, i, b);
//	}
//	return i;
//}
// [/snippet]