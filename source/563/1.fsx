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
type PricePerUnit = decimal

type Refueling = 
  | FillTank of PricePerUnit * Tachymeter * Amount * Substance * bool
  | FillCanister of PricePerUnit * Amount * Substance
  | UseCanister of Amount * Substance

type Journey = Journey of DateTime * TimeSpan * Driver * Distance * string * list<Refueling>

type JourneyBook = Book of Tachymeter * list<Journey>