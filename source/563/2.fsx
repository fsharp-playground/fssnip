open System

[<Measure>] 
type km
[<Measure>]
type liter

type Driver = string
type Distance = int<km>
type Tachymeter = int<km>
type Amount = float<liter>
type Substance = CNG | LPG | Diesel | Gasoline | Coal
type PricePerUnit = float32
type Discount = decimal

type Refueling = 
  | FillTank of Tachymeter * Substance * Amount * bool * PricePerUnit * Discount
  | FillCanister of Tachymeter * Substance * Amount * PricePerUnit * Discount
  | UseCanister of Tachymeter * Substance * Amount

type Journey = Journey of DateTime * TimeSpan * Driver * Distance * string * list<Refueling>

type JourneyBook = Book of Tachymeter * list<Journey>