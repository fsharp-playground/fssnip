 row.EncryptedValue |> DecryptStringWithOldPassword 
                |> function Some x -> (EncryptStringWithNewPassword x |> 
                                        function Some x -> row.EncryptedValue <- x 
                                                    | _ -> printfn "Failed to Encrypt for CCID: %d" row.CreditCardId;) 
                            | _ -> printfn "Failed to Decrypt for CCID: %d" row.CreditCardId ; //restore same value if encryption/decryption failed