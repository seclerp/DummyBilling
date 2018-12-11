namespace DummyBilling.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration

open System.Text
open LiqPay
open LiqPay.Request
open Newtonsoft.Json

type HomeController (configuration:IConfiguration) =
    inherit Controller()

    let publicKey = configuration.GetValue<string>("LiqPay:PublicKey");
    let privateKey = configuration.GetValue<string>("LiqPay:PrivateKey");

    member this.Index () =
        let requestModel = {
            Version = 3
            PublicKey = publicKey
            PrivateKey = privateKey
            Amount = 123u
            Action = "pay"
            Currency = "UAH"
            Description = "Test payment"
            OrderId = makeTransationId()
            Sandbox = "1"
        } 
    
        let data = 
            requestModel
            |> JsonConvert.SerializeObject
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            
        this.ViewData.["PaymentData"] <- data
        this.ViewData.["Signature"] <- makeRequestSignature privateKey data
            
        this.View()