module Helpers

open System

let dispose(obj : #IDisposable) =
    obj.Dispose()


// example usage

type ADisposable() =
    interface IDisposable with
        member this.Dispose() = ()

type SomeObject() =
    let quit = new ADisposable()

    interface IDisposable with
        member this.Dispose() =
            dispose quit
            //(quit :> IDisposable).Dispose()