type Neg<'T when 'T : comparison> = NegativeInfinity | N of 'T
type Pos<'T when 'T : comparison> = P of 'T | PositiveInfinity

N "hello" > NegativeInfinity
P 2 < PositiveInfinity