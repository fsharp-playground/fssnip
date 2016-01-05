// How many semitones in Western music:
let SEMITONES_PER_OCTAVE = 12

// Flat, Natural or Sharp:
type TAccidental = Flat | Natural | Sharp
// A note name, accidental and chromatic index but without specifying the octave it is in - eg. G#, A, or Bb:
type TNoteLabel = {name : char; accidental : TAccidental; chromaticIndex : int}
// A note including its specific octave - eg. G# in octave 2 (from an arbitrary lowest octave):
type TNote = {label : TNoteLabel; octave : int}
// An array of the chromatic indices of notes forming a chord type:
type TChordIndexes = array<int>
// Whether a chord is major or minor:
type TScaleType = Major | Minor
// A Relative Chord in terms of the chromatic indices of its notes - eg. [|0; 4; 7; 12|]; Major is an ordinary
// major chord:
type TChordType = {chordIndexes : TChordIndexes; scaleType : TScaleType}
// An Absolute Chord - eg. C major:
type TChord = array<TNote>
// When translating an index to a readable note, determines whether to prefer the sharp version
// or the flat version when encountering an enharmonic:
type TEnharmonicPreference = PreferFlat | PreferSharp | PreferNeither

// A way of noting which notes have been covered during the generation of a chord:
type TNoteMapping = {mappedNote : TNote; mutable useCount : int}

// How an instrument is tuned - eg. conventional guitar tuning is EADGBE', 'dropped D' tuning is DADGBE'.
type TTuning = array<TNote>

// A combination of string and fret - the actual location of a fingering on the neck:
type TFretting = {stringIndex : int; fretIndex : int}
// A fretting plus the note that would result from that fretting:
type TFrettedNote = {fretting : TFretting; note : TNote}

/// Any fretted instrument:
type TInstrument (fretCount : int, tuning : TTuning) = 
    member this.fretCount = fretCount // Includes nut at 0
    member this.tuning = tuning
    // TODO ensure tuning is the same size as strings

/// All human readable note names, including enharmonics (but not bothering with double-flats, double-sharps):
let allNotes =
    [|
        {name = 'A'; accidental = Natural; chromaticIndex = 0}
        {name = 'A'; accidental = Sharp; chromaticIndex = 1}

        {name = 'B'; accidental = Flat; chromaticIndex = 1}
        {name = 'B'; accidental = Natural; chromaticIndex = 2}
        {name = 'B'; accidental = Sharp; chromaticIndex = 3}

        {name = 'C'; accidental = Flat; chromaticIndex = 2}
        {name = 'C'; accidental = Natural; chromaticIndex = 3}
        {name = 'C'; accidental = Sharp; chromaticIndex = 4}

        {name = 'D'; accidental = Flat; chromaticIndex = 4}
        {name = 'D'; accidental = Natural; chromaticIndex = 5}
        {name = 'D'; accidental = Sharp; chromaticIndex = 6}

        {name = 'E'; accidental = Flat; chromaticIndex = 6}
        {name = 'E'; accidental = Natural; chromaticIndex = 7}
        {name = 'E'; accidental = Sharp; chromaticIndex = 8}

        {name = 'F'; accidental = Flat; chromaticIndex = 7}
        {name = 'F'; accidental = Natural; chromaticIndex = 8}
        {name = 'F'; accidental = Sharp; chromaticIndex = 9}

        {name = 'G'; accidental = Flat; chromaticIndex = 9}
        {name = 'G'; accidental = Natural; chromaticIndex = 10}
        {name = 'G'; accidental = Sharp; chromaticIndex = 11}

        {name = 'A'; accidental = Flat; chromaticIndex = 11}
    |]

/// Some useful constants for notes:
let ANat = {name = 'A'; accidental = Natural; chromaticIndex = 0}
let ASharp = {name = 'A'; accidental = Sharp; chromaticIndex = 1}

let BFlat = {name = 'B'; accidental = Flat; chromaticIndex = 1}
let BNat = {name = 'B'; accidental = Natural; chromaticIndex = 2}
let BSharp = {name = 'B'; accidental = Sharp; chromaticIndex = 3}

let CFlat = {name = 'C'; accidental = Flat; chromaticIndex = 2}
let CNat = {name = 'C'; accidental = Natural; chromaticIndex = 3}
let CSharp = {name = 'C'; accidental = Sharp; chromaticIndex = 4}

let DFlat = {name = 'D'; accidental = Flat; chromaticIndex = 4}
let DNat = {name = 'D'; accidental = Natural; chromaticIndex = 5}
let DSharp = {name = 'D'; accidental = Sharp; chromaticIndex = 6}

let EFlat = {name = 'E'; accidental = Flat; chromaticIndex = 6}
let ENat = {name = 'E'; accidental = Natural; chromaticIndex = 7}
let ESharp = {name = 'E'; accidental = Sharp; chromaticIndex = 8}

let FFlat = {name = 'F'; accidental = Flat; chromaticIndex = 7}
let FNat = {name = 'F'; accidental = Natural; chromaticIndex = 8}
let FSharp = {name = 'F'; accidental = Sharp; chromaticIndex = 9}

let GFlat = {name = 'G'; accidental = Flat; chromaticIndex = 9}
let GNat = {name = 'G'; accidental = Natural; chromaticIndex = 10}
let GSharp = {name = 'G'; accidental = Sharp; chromaticIndex = 11}

let AFlat = {name = 'A'; accidental = Flat; chromaticIndex = 11}

/// An operator to concatenate two arrays:
let (@@) a b =
    Array.concat [a; b]

// Common Relative Chords, in terms of the chromatic indexes of their constituent tones
let major : TChordType = {chordIndexes = [|0; 4; 7; 12|]; scaleType = Major}
let minor : TChordType = {chordIndexes = [|0; 3; 7; 12|]; scaleType = Minor}
let diminished : TChordType = {chordIndexes = [|0; 3; 6; 12|]; scaleType = Minor}
let augmented : TChordType = {chordIndexes = [|0; 5; 8; 12|]; scaleType = Major} // TODO check
let seventh : TChordType = {chordIndexes = major.chordIndexes @@ [|10|]; scaleType = Major}
let majorSeventh : TChordType = {chordIndexes = major.chordIndexes @@ [|11|]; scaleType = Major}
let minorSeventh : TChordType = {chordIndexes = minor.chordIndexes @@ [|10|]; scaleType = Minor}

/// Work out the human-readable note name for a particular chromatic index
let indexToName (index : int) (enharmonicPreference : TEnharmonicPreference) =
    allNotes
    |> Array.sortBy (fun item -> 
                            match enharmonicPreference with
                            | PreferFlat ->
                                            match item.accidental with
                                            | Flat -> 0
                                            | Natural -> 1
                                            | Sharp -> 2
                            | PreferSharp ->
                                            match item.accidental with
                                            | Sharp -> 0
                                            | Natural -> 1
                                            | Flat -> 2
                            | PreferNeither ->
                                            match item.accidental with
                                            | Natural -> 0
                                            | Flat -> 1
                                            | Sharp -> 2
                    )
    |> Array.find (fun item -> item.chromaticIndex = index)

/// Work out the chromatic index of a given note
let noteToIndex (note : TNote) =
    (note.octave * SEMITONES_PER_OCTAVE) + note.label.chromaticIndex

/// Work out into what octave a particular semitone falls (from an arbitrary lower basis)
let indexToOctave (index : int) =
    index / SEMITONES_PER_OCTAVE

/// Work out the human-readable note (including octave) for a particular semitone
let indexToNote (index : int) (enharmonicPreference : TEnharmonicPreference) =
    {label = indexToName (index % SEMITONES_PER_OCTAVE) enharmonicPreference; octave = indexToOctave index}

/// An operator to detect whether one note is the same as or is an enharmonic of another (ignoring octave)
let (=~) note1 note2 =
    note1.chromaticIndex = note2.chromaticIndex

/// As =~ but the compared notes must also be the same octave
let (==~) note1 note2 =
    (note1.label =~ note2.label)
    && (note1.octave = note2.octave)

/// Return a note which is n semitones higher than the input note
let addSemitones note semitones enharmonicPreference =
    let newIndex = noteToIndex(note) + semitones
    indexToNote newIndex enharmonicPreference 

/// Return the note which results from playing the specified string at the specified fret:
let frettingToNote (instrument : TInstrument) (fretting : TFretting) : TNote =
    let openNote = instrument.tuning.[fretting.stringIndex]
    let frettedNote = addSemitones openNote fretting.fretIndex PreferNeither
    frettedNote

/// Given the root note and scale type of a chord, work out whether any sharps/flats in the chord
/// should be expressed using the sharp or the flat of an enharmonic pair 
let rootNoteToEnharmonicPreference (rootNote : TNote) (scaleType : TScaleType) =
    match (rootNote.label.name, rootNote.label.accidental) with
        // TODO many of these need checking
        | ('A', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferFlat
        | ('A', Sharp) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('B', Flat) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferFlat
        | ('B', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferFlat
        | ('C', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferFlat
        | ('C', Sharp) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('D', Flat) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferFlat
        | ('D', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferFlat
        | ('D', Sharp) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('E', Flat) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferFlat
        | ('E', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferFlat
        | ('F', Natural) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferSharp
        | ('F', Sharp) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('G', Flat) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferFlat
        | ('G', Natural) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('G', Sharp) -> 
            match scaleType with
                | Major -> PreferSharp
                | Minor -> PreferSharp
        | ('A', Flat) -> 
            match scaleType with
                | Major -> PreferFlat
                | Minor -> PreferFlat
        | _ -> failwith "rootNoteToEnharmonicPreference: Unexpected root note: %A" rootNote

/// Takes a root note and chord type and returns the actual notes of the chord:
let chordOf (rootNote : TNote) (chordType : TChordType) =
    chordType.chordIndexes
    |> Array.map (fun index -> addSemitones rootNote index (rootNoteToEnharmonicPreference rootNote chordType.scaleType) )

/// List all the notes you could play on a given string between a lowest and highest fret
let notesBetween (instrument : TInstrument) (stringIndex : int) (lowestFret : int) (highestFret : int) : seq<TFrettedNote> =
    let frettedNotes = 
        seq {
            for fret in lowestFret..highestFret do 
                let aFretting = {stringIndex = stringIndex; fretIndex = fret}
                let aNote = frettingToNote instrument aFretting
                yield {fretting = aFretting; note = aNote}
        } 
    frettedNotes

/// As notesBetween but also including the note produced by the same string played open
let notesBetweenAndOpen (instrument : TInstrument) (stringIndex : int) (lowestFret : int) (highestFret : int) : seq<TFrettedNote> =
    (notesBetween instrument stringIndex 0 0) |> Seq.append <| (notesBetween instrument stringIndex lowestFret highestFret)

/// List all the notes which could be played in a range of frets, including notes available by playing a string open
let notesInBox (instrument : TInstrument) (lowestFret : int) (highestFret : int) =
    [|0..(Array.length instrument.tuning)-1|]
    |> Array.map (fun stringIndex -> notesBetweenAndOpen instrument stringIndex lowestFret highestFret)

/// Given two sequences and a comparator function, find the pairs of items for which the comparator returns true
let findPairs compare seqT seqU =
    seq {
            for t in seqT do
                for u in seqU do
                    if (compare t u) then
                        yield (t, u)
    }

/// Given a two sequences and a comparator function, find the first pair of items for which the comparator returns true
let tryFindFirstPair compare seqT seqU =
    let matches = findPairs compare seqT seqU
    if not (Seq.isEmpty matches) then
        Some(Seq.nth 0 matches)
    else
        None

/// Reverse a sequence (don't use on infinite seq's!)
let reverseSeq s =
    s |> Array.ofSeq |> Array.rev |> Seq.ofArray

/// Like skipWhile, but skips items from the point where the function first returns false.
/// (Don't use on infinite seq's!)
let skipAfter f s =
    s
    |> reverseSeq
    |> Seq.skipWhile (fun elem -> f elem)
    |> reverseSeq

/// Print the frettings for a shape
let printShape (label : string) (shape : seq<TFrettedNote>) =
    printfn "%s" label
    shape
    |> Seq.iter (fun fretting -> printfn "String %i fret %i" (fretting.fretting.stringIndex+1) (fretting.fretting.fretIndex) )
    |> ignore

/// For a given chord, search through the notes which could be played within a given range of frets to try and
/// provide a set of frettings which plays the notes for the chord:
let findShape (instrument : TInstrument) (lowestFret : int) (highestFret : int) (chord : TChord) =
    // Create a map of required notes and how many times they have been found (initially all 0):
    let foundNotes = chord |> Array.map (fun note -> {mappedNote = note; useCount = 0})
    // A way to update the map to indicate that this note of the chord has been successfully provided:
    let markDone note =
        let mapIndex = foundNotes |> Array.findIndex (fun item -> item.mappedNote = note)
        foundNotes.[mapIndex].useCount <- foundNotes.[mapIndex].useCount + 1
    // Create a list of notes are needed in priority order, where priority is defined as
    // first, prioritise notes which haven't been found at all; and second, prioritise notes in low-to-high order:
    // (Defined as a function() so that when a note is marked as provided it goes to the back of the queue.)
    let notesInPriorityOrder() = 
        foundNotes 
        |> Array.sortBy (fun item -> (item.useCount*100) + item.mappedNote.label.chromaticIndex)
        |> Array.map (fun item -> item.mappedNote)
        |> Seq.ofArray
    // Go through the strings from low to high listing the notes that string could play:
    let frettings =
        seq {for availableNotesForString in (notesInBox instrument lowestFret highestFret) do
                // Search the available notes and the required notes (the latter in priority order) finding the first
                // instance (if any) on this string where the required note can be played. (Since we use the =~ this might be in
                // any octave.)
                let hit = tryFindFirstPair (fun availNote wantedNote -> availNote.note.label =~ wantedNote.label) 
                                                availableNotesForString (notesInPriorityOrder()) 
                // If we found a note:
                if hit <> None then 
                    // Flag that this note has been found:
                    markDone (snd(hit.Value))
                    // Yield the note together with its fretting:
                    yield fst(hit.Value) 
        }
    // We MUST have the root note at the bottom, so delete any frettings off the end of the sequence 
    // after the root note:
    let rootNote = chord.[0]
    frettings |> skipAfter (fun fretting -> not (fretting.note ==~ rootNote))

// Ordinary 6-string with EADGBE tuning:
let testGuitar = new TInstrument(
                                19, // Including nut
                                [|  // Treble side
                                    {label=ENat; octave=2}
                                    {label=BNat; octave=2}
                                    {label=GNat; octave=1}
                                    {label=DNat; octave=1}
                                    {label=ANat; octave=1}
                                    {label=ENat; octave=0}
                                    // Bass side
                                |]
                              )

// Some chords to play:
let CMaj = chordOf {label=CNat; octave=0} major
let DMaj = chordOf {label=DNat; octave=0} major
let DMin = chordOf {label=DNat; octave=0} minor
let D7 = chordOf {label=DNat; octave=0} seventh
let DMaj7 = chordOf {label=DNat; octave=0} majorSeventh
let EMaj = chordOf {label=ENat; octave=0} major
let EMin = chordOf {label=ENat; octave=0} minor
let AMaj = chordOf {label=ANat; octave=0} major
let AMin = chordOf {label=ANat; octave=0} minor
let ADim = chordOf {label=ANat; octave=0} diminished

// Generate fingerings for the chords:
findShape testGuitar 0 3 CMaj |> printShape "C Major" |> ignore
findShape testGuitar 0 3 EMaj |> printShape "E Major" |> ignore
findShape testGuitar 0 3 EMin |> printShape "E Minor" |> ignore
findShape testGuitar 0 3 AMaj |> printShape "A Major" |> ignore
findShape testGuitar 0 3 AMin |> printShape "A Minor" |> ignore
findShape testGuitar 0 3 ADim |> printShape "A Dim" |> ignore
findShape testGuitar 0 3 DMaj |> printShape "D Major" |> ignore
findShape testGuitar 0 3 DMin |> printShape "D Minor" |> ignore
findShape testGuitar 0 3 D7 |> printShape "D seventh" |> ignore
findShape testGuitar 0 3 DMaj7 |> printShape "D major seventh" |> ignore

// Output:
//  C Major
//  String 1 fret 0
//  String 2 fret 1
//  String 3 fret 0
//  String 4 fret 2
//  String 5 fret 3

// E Major
//  String 1 fret 0
//  String 2 fret 0
//  String 3 fret 1
//  String 4 fret 2
//  String 5 fret 2
//  String 6 fret 0

// E Minor
//  String 1 fret 0
//  String 2 fret 0
//  String 3 fret 0
//  String 4 fret 2
//  String 5 fret 2
//  String 6 fret 0

// D Major
//  String 1 fret 2
//  String 2 fret 3
//  String 3 fret 2
//  String 4 fret 0
