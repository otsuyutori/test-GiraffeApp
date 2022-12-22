module GiraffeApp.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open GiraffeApp.HttpHandlers
open GiraffeApp.SentencePieceProcessor
open System.Runtime.InteropServices
open System.Text
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms.Onnx
open Microsoft.ML.Transforms

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        subRoute "/api"
            (choose [
                GET >=> choose [
                    route "/hello" >=> handleGetHello
                ]
            ])
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

type EncoderInput = {
    [<ColumnName "input_ids">]
    InputIds : int64[]
    [<ColumnName "attention_mask">]
    AttentionMask : int64[]
}

[<CLIMutable>]
type OnnxOutput = {
    [<ColumnName "hidden_states">]
    HiddenStates : Single[]
}

type DecoderInput = {
    [<ColumnName "input_ids">]
    InputIds : int64[]
    [<ColumnName "encoder_attention_mask">]
    AttentionMask : int64[]
    [<ColumnName "encoder_hidden_states">]
    EncoderHiddenStates : float32[]
}

[<CLIMutable>]
type DecoderOutput = {
    [<ColumnName "logits">]
    Logits : Single[]
}

[<EntryPoint>]
let main str =
    let onnx_path = "/home/otsuyutori/GiraffeApp/bin/Debug/net6.0/t5-base-japanese-encoder.onnx"
    let mlContext = MLContext()
    let estimator = mlContext.Transforms.ApplyOnnxModel(
        modelFile = onnx_path,
        outputColumnNames = [| "hidden_states" |],
        inputColumnNames = [| "input_ids"; "attention_mask" |]
    )
    let s0 = Seq.empty<EncoderInput>
    let emptyDv = mlContext.Data.LoadFromEnumerable(s0)
    let pipeline = estimator.Fit(emptyDv)
    let engine = mlContext.Model.CreatePredictionEngine<EncoderInput, OnnxOutput>(pipeline);
    let input = { InputIds = [|76;1|]; AttentionMask = [|1;0|] }
    let output = engine.Predict(input)

    let dec_onnx_path = "/home/otsuyutori/GiraffeApp/bin/Debug/net6.0/t5-base-japanese-init-decoder.onnx"
    let decMlContext = MLContext()
    let decEstimator = decMlContext.Transforms.ApplyOnnxModel(
        modelFile = dec_onnx_path,
        outputColumnNames = [| "logits" |],
        inputColumnNames = [| "input_ids"; "encoder_attention_mask"; "encoder_hidden_states" |]
    )
    let d0 = Seq.empty<DecoderInput>
    let decEmptyDv = decMlContext.Data.LoadFromEnumerable(d0)
    let decPipeline = decEstimator.Fit(decEmptyDv)
    let decEngine = decMlContext.Model.CreatePredictionEngine<DecoderInput, DecoderOutput>(decPipeline);
    let decoderInput = { InputIds = [|0|]; AttentionMask = [|0|]; EncoderHiddenStates = output.HiddenStates }
    let decOutput = decEngine.Predict(decoderInput)
    printfn "%A" decOutput.Logits
    printfn "%d" decOutput.Logits.Length
    0

//  args =
//     Host.CreateDefaultBuilder(args)
//         .ConfigureWebHostDefaults(
//             fun webHostBuilder ->
//                 webHostBuilder
//                     .Configure(Action<IApplicationBuilder> configureApp)
//                     .ConfigureServices(configureServices)
//                     .ConfigureLogging(configureLogging)
//                     |> ignore)
//         .Build()
//         .Run()
//     0