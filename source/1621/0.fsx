let values = ResizeArray()
let newValues = 
  for x in [ 1; 2; ] do 
  for y in [ 10; 20; ] do
    values.Add(x + y)
  for z in [ 100; 200; ] do
    values.Add(z)
  Seq.toList values