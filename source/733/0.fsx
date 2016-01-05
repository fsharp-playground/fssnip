module AripekanKuutioJuttu

// Tästä lähtien toimii interactivessa, voi testailla:


open System

type Entiteetti(nimi:string) = 
   member x.Nimi = nimi

type Ominaisuus = string

type StorageMethods =
| LisääEntiteetti of Ominaisuus * Entiteetti
| HaeEntiteetit of Ominaisuus * AsyncReplyChannel<Entiteetti list>

type OminaisuusStorage() =
    let ominaisuusstorage = MailboxProcessor.Start(fun komento ->
        let rec msgPassing kaikki =
            async { let! k = komento.Receive()
                    match k with
                    | LisääEntiteetti(ominaisuus, entiteetti) ->

                        return! msgPassing((ominaisuus, entiteetti) :: kaikki)
                    | HaeEntiteetit(haettuOminaisuus, reply) ->

                        let entiteetit = 
                            kaikki 
                            |> List.filter(fun i -> fst i = haettuOminaisuus)
                            |> List.map(fun i -> snd i)
                        reply.Reply(entiteetit)
                        return! msgPassing(kaikki)
            }
        msgPassing [])

    member x.Save (entiteetti) =         
        ominaisuusstorage.Post(LisääEntiteetti(entiteetti))
        "saved"

    member x.Get id = 
        ominaisuusstorage.PostAndReply(fun rep -> HaeEntiteetit(id,rep))
        |> List.rev


// Tests for Interactive:         
let storage = new OminaisuusStorage()
let ent1 = new Entiteetti("A")
let ent2 = new Entiteetti("B")

storage.Save("Lukutaito", ent1) |> ignore
storage.Save("Lukutaito", ent2) |> ignore
storage.Save("Kirjoitustaito", ent1) |> ignore

storage.Get("Lukutaito")

