            let mutable encryptedValue = String.Empty;
            encryptedValue <- row.EncryptedValue |> DecryptStringWithOldPassword 
                |> function Some x -> (EncryptStringWithNewPassword x |> 
                                        function Some x -> x 
                                                    | _ -> printfn "Failed to Encrypt for CCID: %d" row.CreditCardId;row.EncryptedValue) 
                            | _ -> printfn "Failed to Decrypt for CCID: %d" row.CreditCardId ;row.EncryptedValue //restore same value if encryption/decryption failed
            if(not (String.IsNullOrEmpty encryptedValue)) then row.EncryptedValue <- encryptedValue