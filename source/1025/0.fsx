open SlimDX
open SlimDX.D3DCompiler
open SlimDX.Direct3D11
open SlimDX.Windows
open SlimDX.DXGI

[<EntryPoint>]
let main args =
    let form = new RenderForm("Test Window")
    let width, height = 640, 640
    do  form.SetBounds(0, 0, width, height)

    let swapChainDescription = 
        new SwapChainDescription(
            BufferCount = 1,
            Usage = (Usage.RenderTargetOutput ||| Usage.UnorderedAccess),
            OutputHandle = form.Handle,
            IsWindowed = true,
            ModeDescription = ModeDescription(0, 0, Rational(60, 1), Format.R8G8B8A8_UNorm),
            SampleDescription = SampleDescription(1, 0))

    let result, device, swapChain = 
        Device.CreateWithSwapChain(
            DriverType.Hardware, 
            DeviceCreationFlags.Debug, 
            [|FeatureLevel.Level_11_0|], 
            swapChainDescription)

    let renderTarget = 
        use renderResource = Resource.FromSwapChain<Texture2D>(swapChain, 0)
        new RenderTargetView(device, renderResource)
    
    let computeShader =
        let code = @"
            RWTexture2D<float4> Output;

            [numthreads(32, 32, 1)]
            void main( uint3 threadID : SV_DispatchThreadID )
            {
                Output[threadID.xy] = float4(threadID.xy / 640.0f, 0, 1);
            }
        "
        use bytecode = ShaderBytecode.Compile(code, "main", "cs_5_0", ShaderFlags.None, EffectFlags.None) 
        new ComputeShader(device, bytecode)

    let computeResult = new UnorderedAccessView(device, renderTarget.Resource)      
    let context = device.ImmediateContext

    context.ComputeShader.Set(computeShader);
    context.ComputeShader.SetUnorderedAccessView(computeResult, 0);

    // set viewport
    context.Rasterizer.SetViewports(Viewport(0.0f, 0.0f, float32 width, float32 height, 0.0f, 1.0f))

    // runs per frame
    MessagePump.Run(form, MainLoop(fun () ->
        // clear render buffer to black
        context.ClearRenderTargetView(renderTarget, Color4())
        // run compute shader
        context.Dispatch(32, 32, 1);
        // copy output buffer to screen
        swapChain.Present(0, PresentFlags.None) |> ignore
    ))

    0 // return an integer exit code