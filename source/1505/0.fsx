type Coffee =
  abstract member getCost: unit -> double
  abstract member getIngredients: unit -> string list
let SimpleCoffee = 
  { new Coffee with 
    member this.getCost() = 1.0
    member this.getIngredients() = ["Coffee"]
  }
let WithMilk(coffee:Coffee) =
  { new Coffee with 
    member this.getCost() = coffee.getCost() + 0.5
    member this.getIngredients() = "milk" :: coffee.getIngredients() }
let WithSyrup (syrup:string) (coffee:Coffee) =
  { new Coffee with 
    member this.getCost() = coffee.getCost() + 0.25
    member this.getIngredients() = syrup :: coffee.getIngredients() }

// Let's try it out
let myCoffee = WithMilk(SimpleCoffee)
// We can pipe them too