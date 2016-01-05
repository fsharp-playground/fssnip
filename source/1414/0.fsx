let hangman = [ """ 
____
|/   |
|   
|    
|    
|    
|
|_____
""";
"""
 ____
|/   |
|   (_)
|    
|    
|    
|
|_____
""";
"""
 ____
|/   |
|   (_)
|    |
|    |    
|    
|
|_____
""";
"""
 ____
|/   |
|   (_)
|   \|
|    |
|    
|
|_____
""";
"""
 ____
|/   |
|   (_)
|   \|/
|    |
|    
|
|_____
""";
"""
 ____
|/   |
|   (_)
|   \|/
|    |
|   / 
|
|_____
""";
"""
 ____
|/   |
|   (_)
|   \|/
|    |
|   / \
|
|_____
""";
"""
 ____
|/   |
|   (_)
|   /|\
|    |
|   | |
|
|_____
"""]


open System

let words = ["PENCIL";"CHALK";"CRAYON";"BRUSH"]

let toPartialWord (word:string) (used:char seq) =
   word |> String.map (fun c -> 
      if Seq.exists ((=) c) used then c else '_'
   )

let isGuessValid (used:char seq) (guess:char) =
   Seq.exists ((=) guess) ['A'..'Z'] &&
   not (used |> Seq.exists ((=) guess))

let rec readGuess used =
   let guess = Console.ReadKey(true).KeyChar |> Char.ToUpper
   if isGuessValid used guess then guess
   else readGuess used

let getGuess used =
   Console.Write("Guess: ")
   let guess = readGuess used
   Console.WriteLine(guess)
   guess

let rec play word used tally =
   Console.Write(hangman.[tally])
   let word' = toPartialWord word used
   Console.WriteLine(word')
   if word = word' then 
      Console.WriteLine("CORRECT")
   elif tally = hangman.Length-1 then 
      Console.WriteLine("HANGMAN")
   else
      let guess = getGuess used
      let used = guess::used
      if word |> String.exists ((=) guess)
      then play word used tally
      else play word used (tally+1)

let word = words.[Random().Next(words.Length)]
do play word [] 0