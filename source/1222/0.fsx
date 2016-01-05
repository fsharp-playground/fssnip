// Given a type 'SomeType' and a discriminated union with
// multiple cases that reference SomeType:

type SomeType = { Name: string }

type Union =
  | Foo of SomeType
  | Bar of SomeType
  | Zoo of int

// We can avoid code duplication when we need to get SomeType
// from the Foo and Bar case by merging them into a single case:

type SomeKind = 
  | Foo | Bar

type Union2 =
  | Some of SomeType
  | Zoo of int