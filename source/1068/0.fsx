
[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module internal TreeList = 
    let Treelist_indexing_error = errors.Invalid_state "There was a strange indexing error involving a binary tree."
    let Node_without_children = errors.Invalid_state "The program tried to get the children of a leaf node."
    type Size = int
    
    type BinaryTree<'a> =
        internal 
        | Leaf of 'a
        | Parent of BinaryTree<'a> * BinaryTree<'a> * Size (* The sk*)
        | ValuedParent of 'a * BinaryTree<'a> * BinaryTree<'a> * Size
        with

        member this.Value = 
            match this with
            | Leaf(v) | ValuedParent(v,_,_,_) -> v
            | Parent _ -> failwith "This node has no value."

        member this.HasValue = 
            match this with
            | Parent _ -> false
            | _ -> true

        member this.Left = 
            match this with
            | Parent(left,_,_) | ValuedParent(_,left,_,_) -> left
            | Leaf v -> raise Node_without_children
          

        member this.Right = 
            match this with
            | Parent(_,right,_) | ValuedParent(_,_,right,_) -> right
            | Leaf v -> raise Node_without_children

        
        member this.Count = 
            match this with
            | Leaf(_) -> 1 | Parent(_,_,c) | ValuedParent(_,_,_,c) -> c 

        member this.Get i =  
            let rec get tree i = 
                (* This algorithm is trivial. Every node knows how many value children it has. *)
                match tree with
                | _ when this.Count <= i -> failwith "The index is not found in this tree."
                | ValuedParent(v,_,_,_) | Leaf(v) when i = 0 -> v

                | Parent(left,_,_) when left.Count > i -> get left i
                | Parent(left,right,_ ) ->
                    get right (i - left.Count)
                | ValuedParent(_,left,right,_) when 1 + left.Count > i -> get left (i - 1)
                | ValuedParent (_,left,right,_) ->
                    get right (i - left.Count - 1)
                | _ -> raise Treelist_indexing_error
            get this i

        member this.First = 
            match this with
            | Leaf(v) | ValuedParent(v,_,_,_) -> v
            | Parent(left,_,_) -> left.First
    
    module public Tree =   
        let inline left (tree : ^s) : ^s = 
            ( ^s : ( member Left : ^s) tree)

        let inline right (tree : ^s) : ^s = 
            ( ^s : ( member Right : ^s) tree)

        let inline value (tree : ^s) : ^a = 
            ( ^s : (member Value : ^a) tree)

        let inline isEmpty ( tree : ^s) : bool = 
            (^s : (member IsEmpty : bool) tree)

        let inline empty () = 
            (^s : (static member empty : ^s) () )

     
        let newLeaf v = Leaf v
            
        let newParent left right = Parent(left,right,left.Count + right.Count)

        let newValParent v left right = ValuedParent(v,left,right,left.Count + right.Count + 1)
             

    
    
    open Tree
    type TreeList<'a> = 
    | Nil
    | (::) of BinaryTree<'a> * TreeList<'a> 
    (*Fun fact: the cons union case, (::), can be defined. Allows us to pattern match list-like structures that are not lists.*)
    with
        static member Empty = Nil
        
        member this.Head = 
            match this with
            | Nil -> failwith "Empty list."
            | h::_ -> h.First

        member this.IsEmpty = 
            match this with
            | Nil -> true | _ -> false

        member private this.fix (s : TreeList<'a>) : TreeList<'a> = 
            match s with
            | ValuedParent(_,_,_,c_1)::ValuedParent(_,_,_,c_2)::_ & a::b::tail when c_1 = c_2->
                newParent a b :: tail
            | _ -> s

        member this.Cons v : TreeList<'a> =
            match this with
            | Nil -> newLeaf(v)::Nil (* Trivial case *)
            | ValuedParent _ :: _ -> 
                let fx = this.fix this
                match fx with
                | Parent(left,right,_)::tail -> newValParent v left right::(tail)
                | _ -> newLeaf v :: this
            | Leaf _ as head::tail -> (newParent (newLeaf v) (head))::tail
            | Parent(left,right,_)::tail -> (newValParent v left right) :: tail
  

        member this.Uncons = 
            match this with
            | Nil -> failwith "The list is empty."
            | Leaf _::tail -> tail
            | ValuedParent(_,left,right,_)::tail -> newParent left right :: tail
            | Parent(Leaf _, leaf,_)::tail -> leaf::tail
            | Parent(ValuedParent(_,left_left,left_right,_),right,_)::tail -> 
                newParent left_left left_right :: right :: tail
            | _ -> failwith "Invalid."
        
        member this.Length =
            let mutable count = 0
            let mutable lst = this
            while lst.IsEmpty |> not do
                match lst with
                | head::tail ->
                    count <- count + head.Count
                    lst <- tail
                | Nil -> raise errors.Contradiction
            count

        member this.Get i =
            let rec get lst i = 
                match lst with
                | Nil -> raise errors.Is_empty
                | head::tail when head.Count > i -> head.Get i
                | head::tail -> get tail (i - head.Count)
            get this i
                

    

