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

type private FrankChannel(innerChannel) =
  inherit DelegatingChannel(innerChannel)

let private webApi app = fun inner ->
  { new FrankChannel(inner) with
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
// [snippet: Self-host sample]
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

namespace HelloAspNet.App
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
open System.Web.Routing
open Microsoft.ApplicationServer.Http
open Microsoft.ApplicationServer.Http.Activation
open Frank
open Frank.Hosting.AspNet
open Frank.Hosting.Wcf
// [snippet: ASP.NET Routing sample]
type Global() =
  inherit System.Web.HttpApplication() 

  static member RegisterRoutes(routes:RouteCollection) =
    // Echo the request body contents back to the sender. 
    // Use Fiddler to post a message and see it return.
    let app request = async {
      return new HttpResponseMessage(HttpStatusCode.OK, "OK", Content = new ObjectContent<string>("Hello, world!\n")) }

    // Uses the head middleware.
    // Try using Fiddler and perform a HEAD request.
    routes.MapServiceRoute<EmptyService>("hello", frank app)

  member x.Start() =
    Global.RegisterRoutes(RouteTable.Routes)
// [/snippet]