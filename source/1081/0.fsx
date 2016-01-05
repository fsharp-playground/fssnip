let rec retry work resultOk retries = async {
  let! res = work
  if (resultOk res) || (retries = 0) then return res
  else return! retry work resultOk (retries - 1) }