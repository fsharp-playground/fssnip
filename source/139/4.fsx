module Frank.Hosting.Wcf
(* License
 *
 * Author: Ryan Riley <ryan.riley@panesofglass.org>
 * Copyright (c) 2011, Ryan Riley.
 *
 * Licensed under the Apache License, Version 2.0.
 * See LICENSE.txt for details.
 *)

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.ServiceModel
open Microsoft.ApplicationServer.Http
open Microsoft.ApplicationServer.Http.Activation
open Microsoft.ApplicationServer.Http.Description
// [snippet: The wrapper type declarations]
[<ServiceContract>]
type EmptyService() =
  [<OperationContract>]
  member x.Invoke() = ()

type private LiteChannel(innerChannel) =
  inherit DelegatingChannel(innerChannel)

let private webApi app = fun inner ->
  { new LiteChannel(inner) with
      override this.SendAsync(request, cancellationToken) =
        Async.StartAsTask(app request, cancellationToken = cancellationToken) } :> HttpMessageChannel

let frank app =
  HttpHostConfiguration.Create()
    .AddMessageHandlers(typeof<LiteChannel>)
    .SetMessageHandlerFactory(webApi app)
// [/snippet]

module Program
(* License
 *
 * Author: Ryan Riley <ryan.riley@panesofglass.org>
 * Copyright (c) 2010-2011, Ryan Riley.
 *
 * Licensed under the Apache License, Version 2.0.
 * See LICENSE.txt for details.
 *)

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.ServiceModel
open Microsoft.ApplicationServer.Http
open Microsoft.ApplicationServer.Http.Activation
open Microsoft.ApplicationServer.Http.Description
open Frank.Hosting.Wcf
// [snippet: Sample application]
[<EntryPoint>]
let main args =
  let app request = async {
    return new HttpResponseMessage(HttpStatusCode.OK, "OK", Content = new ObjectContent<string>("Hello, world!\n")) }

  let baseUri = Uri "http://localhost:1000/"
  let host = new HttpConfigurableServiceHost<EmptyService>(frank app, [| baseUri |])
  host.Open()

  printfn "Host open.  Hit enter to exit..."
  printfn "Use a web browser and go to %Aroot or do it right and get fiddler!" baseUri
  System.Console.Read() |> ignore

  host.Close()
  0
// [/snippet]