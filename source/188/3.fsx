//Perl style regex match operator, eg: foo =~ "foo"
let (=~) target regex =
  System.Text.RegularExpressions.Regex.IsMatch(target, regex)


//Example
let x = "lool";
if x =~ "lo+l" then
  printf "matched"
else
  printf "nope"