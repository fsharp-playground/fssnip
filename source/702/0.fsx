module Test1 =
   let x = 5
   let get_clo() = fun () -> x

module Test2 =
   let x = 10
   let f = Test1.get_clo()

Test2.f() /// 5

