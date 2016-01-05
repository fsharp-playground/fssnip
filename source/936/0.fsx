open System

/// Returns a value indicating whether the specified phrase occurs within the given text
let containsPhrase (phrase:string) (text:string) =
    let rec contains index =
        if index <= text.Length - phrase.Length then compare index
        else false
    and compare index =        
        if String.Compare(text, index, phrase, 0, phrase.Length) <> 0
        then nextWord index
        else true
    and nextWord index =
        let index = text.IndexOf(' ', index)
        if index >= 0 then contains (index+1)
        else false     
    contains 0

open NUnit.Framework

[<TestFixture>]
module ``Sample Tests`` =

    [<TestCase("", "")>]
    [<TestCase("", " ")>]
    [<TestCase("", "  ")>]
    let [<Test>] ``Match empty phrase`` (phrase:string, text:string) =
        Assert.IsTrue( containsPhrase phrase text )

    [<TestCase("A", "A")>]
    [<TestCase("B", " B")>]
    [<TestCase("C", "C ")>]
    [<TestCase("C", " C ")>]
    [<TestCase("C", "  C ")>]
    let [<Test>] ``Finds single character phrase`` (phrase:string, text:string) =
        Assert.IsTrue( containsPhrase phrase text )

    [<TestCase("apple", "apple")>]
    [<TestCase("orange", " orange")>]
    [<TestCase("pear", "pear ")>]
    [<TestCase("banana", " banana ")>]
    [<TestCase("lemon", "  lemon ")>]
    let [<Test>] ``Finds multiple character phrase`` (phrase:string, text:string) =
        Assert.IsTrue( containsPhrase phrase text )

    [<TestCase("green apple", "green apple")>]
    [<TestCase("orange orange", " orange orange")>]
    [<TestCase("pair of pears", "pair of pears ")>]
    [<TestCase("yellow banana", " yellow banana ")>]
    [<TestCase("zesty lemon", "  zesty lemon ")>]
    let [<Test>] ``Finds multiple word phrase`` (phrase:string, text:string) =
        Assert.IsTrue( containsPhrase phrase text )

    [<TestCase("tea", "steak")>]
    let [<Test>] ``Only finds exact phrases`` (phrase:string, text:string) =
        Assert.IsFalse( containsPhrase phrase text )