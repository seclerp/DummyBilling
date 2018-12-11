module LiqPay

open System.Security.Cryptography
open Newtonsoft.Json
open System.Text
open System

open Newtonsoft.Json.Converters
open System.Runtime.Serialization
open Result

let private getHash (value : string) =
    use sha1 = new SHA1CryptoServiceProvider();
    value 
    |> Encoding.UTF8.GetBytes
    |> sha1.ComputeHash

let makeTransationId () = Guid.NewGuid().ToString("N")

let makeRequestSignature privateKey data = 
    privateKey +
    data +
    privateKey
    |> getHash
    |> Convert.ToBase64String 

module Request = 
    [<CLIMutable>]
    type LiqPayClientModel = {
        [<JsonProperty("version")>]        Version     : int
        [<JsonProperty("public_key")>]     PublicKey   : string
        [<JsonProperty("private_key")>]    PrivateKey  : string
        [<JsonProperty("amount")>]         Amount      : uint32
        [<JsonProperty("action")>]         Action      : string
        [<JsonProperty("currency")>]       Currency    : string
        [<JsonProperty("description")>]    Description : string
        [<JsonProperty("order_id")>]       OrderId     : string
        [<JsonProperty("sandbox")>]        Sandbox     : string
    }

module Callback = 
    [<CLIMutable>]
    type LiqPayCallbackData = {
        [<JsonProperty("public_key")>]     PublicKey     : string
        [<JsonProperty("amount")>]         Amount        : uint32
        [<JsonProperty("currency")>]       Currency      : string
        [<JsonProperty("description")>]    Description   : string
        [<JsonProperty("type")>]           Type          : string
        [<JsonProperty("status")>]         Status        : string
        [<JsonProperty("signature")>]      Signature     : string
        [<JsonProperty("order_id")>]       OrderId       : string
        [<JsonProperty("transaction_id")>] TransactionId : string
        [<JsonProperty("sender_phone")>]   SenderPhone   : string
    }

    [<CLIMutable>]
    type LiqPayCallbackModel = {
        [<JsonProperty("data")>]      Data      : string
        [<JsonProperty("signature")>] Signature : string
    }

    let toCallbackData data =
        data
        |> Convert.FromBase64String
        |> Encoding.UTF8.GetString
        |> JsonConvert.DeserializeObject<LiqPayCallbackData>

    let checkCallbackFields  =
        function
        | { PublicKey = null } 
        | { Description = null } 
        | { Signature = null }
        | { OrderId = null }
        | { TransactionId = null }
        | { SenderPhone = null } -> Result.Failure "One of the required field is empty"
        | validModel             -> Result.Success validModel

    let validateSignature privateKey signature data =
        makeRequestSignature privateKey data
        |> function
        | s when s = signature -> Result.Success data
        | _                   -> Result.Failure "Invalid signature"