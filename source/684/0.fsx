(** Найти лист наименьшей глубины. **)
(**   Используем поиск в ширину.   **)
type 't tree = Leaf of 't | Node of 't*('t tree list)

let upper_leaf tree =
    (* Шаг 1: поиском в глубину приписываем каждому узлу метку с его уровнем *)
    let rec nodes_with_level tree level = match tree with
        | Leaf(x) -> Leaf((x, level))
        | Node(x, children) -> Node((x, level), (List.map (fun child -> nodes_with_level child (level+1)) children))

    (* Шаг 2: поиском в ширину идем до самого "высокого" листа. *)
    let rec bfs node_queue = match node_queue with
        | h::t -> match h with
                    | Leaf((x, level)) -> (x, level); (* Нашли лист. Возвращаем tuple (значение, уровень) *)
                    | Node((x, level), children) -> bfs (t@children) (* Фигачим детей в конец листа, идем дальше *)
    
    (* Запуск *)
    bfs [(nodes_with_level tree 0)]

(* Samples *)
upper_leaf (Leaf(1))
upper_leaf (Node(1,
                [Node(2,
                    [Node(3,
                        [Leaf(4)])
                    ]);
                 Leaf(5)
                ])
           )
upper_leaf (Node(1,
                [Node(2,
                    [Node(3,
                        [Leaf(4)])]);
                Node(5,
                    [Node(6,
                        [Leaf(7)])])
                ])
            )