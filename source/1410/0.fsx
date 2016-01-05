open Microsoft.FSharp.Control

type Reply<'T> =
    | Success of 'T
    | Error of exn
with
    member e.Value =
        match e with
        | Success t -> t
        | Error e -> raise e

and ReplyChannel<'T> internal (rc : AsyncReplyChannel<Reply<'T>>) =
    member __.Reply (t : 'T) = rc.Reply <| Success t
    member __.ReplyWithError (e : exn) = rc.Reply <| Error e

and MailboxProcessor<'T> with
    member m.PostAndReply (msgB : ReplyChannel<'R> -> 'T) =
        m.PostAndReply(fun ch -> msgB (new ReplyChannel<_>(ch))).Value