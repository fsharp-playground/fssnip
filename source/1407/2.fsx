 row.EncryptedValue |> DecryptStringWithOldPassword 
                |> function Some x -> (EncryptStringWithNewPassword x |> 
                                        function Some x -> row.EncryptedValue <- x 
                                                    | _ -> printfn "Failed to Encrypt for CCID: %d" row.ccid;) 
                            | _ -> printfn "Failed to Decrypt for CCID: %d" row.ccid;