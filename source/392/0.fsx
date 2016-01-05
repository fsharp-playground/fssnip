  let solve next_f done_f initial =
      let rec search state =
          seq {
              if done_f state then
                 yield state
              else
                 for state' in next_f state do
                     yield! search state'
              }
      search initial