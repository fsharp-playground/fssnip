// VS2010: This works as expected (PRE-SP1)
// Note:  The types are mutually recursive with "and"
//        plus we're NOT using the implicit constructor
type SuperType() =
  abstract Foo : int -> int
  default x.Foo a = a + 1

and SubType1() =
  inherit SuperType()
  override x.Foo a = base.Foo(a)

// VS2010-SP1: Works as expected
// Note:  The types are not mutually recursive with "and"
type SuperType2() =
  abstract Foo : int -> int
  default x.Foo a = a + 1

type SubType2 =
  inherit SuperType2
  new () = { inherit SuperType2() }
  override x.Foo a = base.Foo(a)

// VS2010-SP1: Works as expected
// Note:  We're using the implicit constructor
type SuperType3() =
  abstract Foo : int -> int
  default x.Foo a = a + 1

type SubType3() =
  inherit SuperType3()
  override x.Foo a = base.Foo(a)

// VS2010-SP1: This will not work as expected
// Note:  The types are mutually recursive with "and"
//        plus we're NOT using the implicit constructor
//        this causes the base reference to fail to compile
type SuperType4() =
  abstract Foo : int -> int
  default x.Foo a = a + 1

and SubType4 =
  inherit SuperType4
  new () = { inherit SuperType4() }

  // With visual studio SP1 this will not compile
  // with the following error:
  // 
  // Error	1	'base' values may only be used to make direct calls 
  // to the base implementations of overridden members	<file> <line>
  override x.Foo a = base.Foo(a)