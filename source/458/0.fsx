type IA2<'T> =
    abstract member Do : 'T -> unit
 
type CompositeNode<'T> = 
    | Node of 'T
    | Tree of 'T * CompositeNode<'T> * CompositeNode<'T>
    with 
        member this.InOrder f = 
            match this with
            | Tree(n, left, right) -> 
                left.InOrder(f)
                f(n)
                right.InOrder(f)
            | Node(n) -> f(n)
        member this.PreOrder f =
            match this with
            | Tree(n, left, right) ->                 
                f(n)
                left.PreOrder(f)
                right.PreOrder(f)
            | Node(n) -> f(n)
        member this.PostOrder f =
            match this with
            | Tree(n, left, right) -> 
                left.PostOrder(f)
                right.PostOrder(f)
                f(n)
            | Node(n) -> f(n)
       

let Invoke() = 
    let tree = Tree(1, Tree(11, Node(12), Node(13)), Node(2))
    let wrapper = 
        let result = ref 0
        ({ new IA2<int> with                
                member this.Do(n) = 
                    result := !result + n                
        }, result)
    tree.PreOrder (fst wrapper).Do

    printfn "result = %d" !(snd wrapper)
