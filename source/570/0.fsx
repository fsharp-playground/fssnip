module Serializer.Tests
open Serializer
open System
open System.IO
open System.Text

let private test datum =
    let stream = new MemoryStream()
    serialize datum stream
    stream.Position <- 0L
    deserialize stream |> printfn "%A = %A" datum

let private caption string = printfn "-- %s --" string

caption "simple discriminated union test"

type SimpleDU =
    | One
    | Two of int
    | Three of int * string * float

test One
test <| Two(42)
test <| Three(69, "bleh", 37.0101)

caption "simple discriminated union test with generic arguments"

type 'a GenericContainer(foo : 'a) =
    member val foo = foo with get, set
    private new () = GenericContainer(Unchecked.defaultof<'a>)
    override x.ToString() = foo.ToString()

type 'a AnotherGenericContainer(bar : 'a) =
    inherit GenericContainer<'a>(bar)
    private new () = AnotherGenericContainer(Unchecked.defaultof<'a>)

type 'a SimpleGenericDU =
    | One of 'a GenericContainer
    | Two of int GenericContainer
    | Three of 'a AnotherGenericContainer
    | Four of string AnotherGenericContainer

test <| One(new GenericContainer<float>(42.69))
test <| Two(new GenericContainer<int>(37))
test <| Three(new AnotherGenericContainer<string>("r00fles!"))
test <| Four(new AnotherGenericContainer<string>("meow"))

caption "discriminated union tree"

type Datum =
    | Cons of Datum * Datum
    | Symbol of string
    | Nil

test <| Cons(Symbol("Java"), Cons(Symbol("sucks"), Cons(Symbol("bigtime"), Nil)))

Console.ReadLine () |> ignore