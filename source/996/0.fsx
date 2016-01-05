                        let rec filterExpression exp (filter:FilterExpression) =
                            let logicalType (node:Expression) =(if node.NodeType = ExpressionType.AndAlso then LogicalOperator.And else LogicalOperator.Or) 
                            match exp with
                            | AndAlsoOrElse(AndAlsoOrElse(_,_) as left,Condition(c) as cond) as outer ->
                                filter.AddCondition(c)
                                let f = filter.AddFilter (logicalType left)
                                filterExpression left f      
                            | AndAlsoOrElse(AndAlsoOrElse(_,_) as left, (AndAlsoOrElse(_,_) as right)) as outer ->                                
                                let f1 = filter.AddFilter (logicalType left)
                                filterExpression left f1 |> ignore
                                let f2 = filter.AddFilter (logicalType right)
                                filterExpression right f2 |> ignore
                                filter                                         
                            | AndAlsoOrElse(Condition(c1) ,Condition(c2) ) as outer ->                                
                                filter.AddCondition(c1)
                                filter.AddCondition(c2)
                                filter                                                  
                            | Condition(c) -> 
                                filter.AddCondition(c)
                                filter
                            