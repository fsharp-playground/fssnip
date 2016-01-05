
let batchSize = 10L

let byteSeq (reader: BinaryReader) = 	
	let bs = reader.BaseStream
	seq {						
 		while not (bs.Position = bs.Length) do // bs.Position is always 0 in this outer loop
		//printf "-> %i" (bs.Position |> int)
		yield seq {	
			let sPos = bs.Position
			//printf "-> %i" (bs.Position |> int)
			while not (bs.Position = (min (sPos + batchSize) bs.Length)) do // bs.Position is increasing as it reads the bytes
				yield reader.ReadByte() 
		}
	}

let fileName = @"";
use reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))

byteSeq reader

