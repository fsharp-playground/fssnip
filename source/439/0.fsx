/// Given a two sequences and a comparator function, find the pairs of items for 
/// which the comparator returns true
let findPairs compare seqT seqU =
    seq {
            for t in seqT do
                for u in seqU do
                    if (compare t u) then
                        yield (t, u)
    }

/// Given a two sequences and a comparator function, find the first pair of items 
/// for which the comparator returns true
let tryFindFirstPair compare seqT seqU =
    let matches = findPairs compare seqT seqU
    if not (Seq.isEmpty matches) then
        Some(Seq.nth 0 matches)
    else
        None

//// A quick demo to generate lists of rhyming words:

/// Reverse a string (from TomasP):
let reverse (s:string) =
  let rec reverseAux idx acc =
    if (idx = s.Length) then acc
    else reverseAux (idx+1) ((s.[idx])::acc)
  new string(Array.ofList (reverseAux 0 []))

/// A crude way to work out the last syllable for a word:
let lastSyllable word =
    let vowels = [|'a';'e';'i';'o';'u';'y'|]
    let wordRev = reverse word
    let vowelPos = wordRev.IndexOfAny(vowels, 1)
    let lastSylRev = wordRev.Substring(0, vowelPos+1)
    reverse lastSylRev

/// Return true when words might rhyme based on their final syllables being 
/// the same
let mightRhyme wordA wordB =
    (wordA <> wordB) && (lastSyllable wordA = lastSyllable wordB)

/// Two steams of words, some of which might rhyme
let words1 = ["orange"; "purple"; "hubble"; "indicative"; "mandatory"]
let words2 = ["hurple"; "rhubarb"; "tory"; "bubble"]

/// Find the rhymes
findPairs mightRhyme words1 words2

// Output: seq [("purple", "hurple"); ("hubble", "bubble"); ("mandatory", "tory")]

/// Find the rhymes in a single stream of words:
let wordsAll = ["orange"; "purple"; "hubble"; "indicative"; "mandatory"; "hurple"; "rhubarb"; "tory"; "bubble"]
findPairs mightRhyme wordsAll wordsAll

// Output: seq
//    [("purple", "hurple"); ("hubble", "bubble"); ("mandatory", "tory");
//     ("hurple", "purple"); ...]