type Foo =
    static member Do1 = 1
    static member Do2 = 1 + Foo.Do1
    static member Do3 = 1 + Do1 // doesn't compile