type DoB = | DoB of DateTime

let DoB (dt:DateTime) = // shadow constructor
  if(dt.Year>1900) //what does the business say a min valid dob should be? 
  then Some(DoB dt) 
  else None

let dob= DoB(DateTime(2014,1,1))