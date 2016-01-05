module Seq =
    let private proxyGen = Castle.DynamicProxy.ProxyGenerator ()
    
    let cast2 (t : System.Type) (sq : System.Collections.IEnumerable) =
        let t1 = (typedefof<seq<_>>).MakeGenericType [| t |]
        let t2 = (typedefof<System.Collections.Generic.IEnumerator<_>>).MakeGenericType [| t |]
        proxyGen.CreateInterfaceProxyWithoutTarget (t1,
            { new IInterceptor with
                member self.Intercept (inv : IInvocation) =
                    let enum = sq.GetEnumerator ()
                    inv.ReturnValue <-
                        proxyGen.CreateInterfaceProxyWithoutTarget (t2,
                            { new IInterceptor with
                                member self.Intercept (inv : IInvocation) =
                                    match inv.Method.Name with
                                    | "MoveNext" -> inv.ReturnValue <- enum.MoveNext ()
                                    | "Reset" -> enum.Reset ()
                                    | "get_Current" -> inv.ReturnValue <- enum.Current
                                    | _ -> failwith "inconceivable"
                            })
            })
