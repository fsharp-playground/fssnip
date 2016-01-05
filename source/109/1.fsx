type ReflectiveListBuilder = 
    static member BuildList<'a> (args: obj list) = 
        [ for a in args do yield a :?> 'a ] 
    static member BuildTypedList lType (args: obj list) = 
        typeof<ReflectiveListBuilder>
            .GetMethod("BuildList")
            .MakeGenericMethod([|lType|])
            .Invoke(null, [|args|])

//Also, here's a caching version because reflection is slow.  System.Type doesn't
//support the comparison constraint so I just use the the full name of the type.

type CachingReflectiveListBuilder = 
    static member ReturnTypedListBuilder<'a> () : obj list -> obj = 
        let createList (args : obj list) = [ for a in args do yield a :?> 'a ] :> obj
        createList
    static member private builderMap = ref Map.empty<string, obj list -> obj>
    static member BuildTypedList (lType: System.Type) =
        let currentMap = !CachingReflectiveListBuilder.builderMap
        if Map.containsKey (lType.FullName) currentMap then
            currentMap.[lType.FullName]
        else
           let builder = typeof<CachingReflectiveListBuilder>
                            .GetMethod("ReturnTypedListBuilder")
                            .MakeGenericMethod([|lType|])
                            .Invoke(null, null) 
                            :?> obj list -> obj
           CachingReflectiveListBuilder.builderMap := Map.add lType.FullName builder currentMap
           builder  