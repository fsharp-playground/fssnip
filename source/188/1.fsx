//Perl style regex match operator, eg: foo =~ "foo"
let (=~) target regex =
  System.Text.RegularExpressions.Regex.Match(target, regex).Success


//Example
let x = "lool";
x =~ "lo+l"