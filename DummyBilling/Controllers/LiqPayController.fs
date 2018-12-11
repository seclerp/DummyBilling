namespace DummyBilling.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration

open Result
open LiqPay
open LiqPay.Callback
open Microsoft.Extensions.Logging

[<Route("api/[controller]")>]
[<ApiController>]
type LiqPayController (configuration:IConfiguration) =
    inherit ControllerBase()

    let privateKey = configuration.GetValue<string>("LiqPay:PrivateKey")

    [<HttpPost("callback")>]
    member this.Callback([<FromForm>] callbackModel:LiqPayCallbackModel) = 
        let context = this.HttpContext
        let parsedData = toCallbackData callbackModel.Data
        let validationResult = validateSignature privateKey callbackModel.Signature callbackModel.Data
        match validationResult with
        | Success _ ->    parsedData |> printfn "Callback data: %A"
        | Failure message -> message |> printfn "Error: %s"
        