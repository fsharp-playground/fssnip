
open NUnit.Framework

type Calculator() = 
    member __.Add x y = x + y

let ``Given I have entered`` firstNumber continuation = 
    let calculator = Calculator()
    continuation (firstNumber, calculator)

let ``into the calculator`` (state,calculator) continuation = 
    continuation (state, calculator)

let ``And I have entered`` (firstNumber, calculator) secondNumber continuation = 
    continuation ((firstNumber, secondNumber), calculator)

let ``When I press add`` ((firstNumber, secondNumber), calculator:Calculator) continuation = 
    let actual = calculator.Add firstNumber secondNumber
    continuation actual

let ``Then the result should be`` actual (expected:int) continuation = 
    Assert.AreEqual(expected, actual)
    continuation

let ``on the screen`` = ()
   

[<Test>]
let testAdd ()= 
    ``Given I have entered`` 50 ``into the calculator`` 
     ``And I have entered`` 70 ``into the calculator`` 
     ``When I press add`` 
     ``Then the result should be`` 120 ``on the screen``
 