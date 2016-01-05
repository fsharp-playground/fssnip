open SlimDX
open SlimDX.D3DCompiler
open SlimDX.Direct3D11
open SlimDX.Windows
open System.Runtime.InteropServices

#nowarn "9" 
[<StructLayout(LayoutKind.Sequential)>]
type Constants = {frame : int32}

// Helper class for handling constants
type ShaderConstants<'a> (device : Device) =
    // NOTE - the buffer size must be a multiple of 16
    let vsConstBuffer = 
        new Buffer(device, 
            new BufferDescription(
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = ((sizeof<'a> + 15) / 16) * 16,
                CpuAccessFlags = CpuAccessFlags.Write,
                Usage = ResourceUsage.Dynamic))

    let context = device.ImmediateContext

    let updateShaderConstants constBuffer sizeInBytes data  = 
        let constData = context.MapSubresource(constBuffer, MapMode.WriteDiscard, MapFlags.None)
        Marshal.StructureToPtr(data, constData.Data.DataPointer, false)
        context.UnmapSubresource(constBuffer, 0)
        
    member this.Update(constants : 'a) =
        updateShaderConstants vsConstBuffer sizeof<'a> constants
        context.ComputeShader.SetConstantBuffers([|vsConstBuffer|], 0, 1)

// Sadly there are some naming conflicts between SlimDX.DXGI and SlimDX.Direct3D11.
open SlimDX.DXGI

[<EntryPoint>]
let main args =
    use form = new RenderForm("Test Window")
    let width, height = 1000, 1000
    do  form.SetBounds(0, 0, width, height)

    let swapChainDescription = 
        SwapChainDescription(
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

    use renderTarget = 
        use renderResource = Resource.FromSwapChain<Texture2D>(swapChain, 0)
        new RenderTargetView(device, renderResource)
    
    use computeShader =
        let code = @"
            RWTexture2D<float4> Output;

            int frame;

            uint wang_hash (uint seed) {
                seed = (seed ^ 61) ^ (seed >> 16);
                seed *= 9;
                seed = seed ^ (seed >> 4);
                seed *= 0x27d4eb2d;
                seed = seed ^ (seed >> 15);
                return seed;
            }

            [numthreads(32, 32, 1)]
            void main (uint3 threadID : SV_DispatchThreadID) {
                float noise = wang_hash(threadID.x + threadID.y * 640 + frame) / 4294967296.0;
                Output[threadID.xy] = float4(noise, noise, noise, 1);
            }
        "
        use bytecode = ShaderBytecode.Compile(code, "main", "cs_5_0", ShaderFlags.None, EffectFlags.None) 
        new ComputeShader(device, bytecode)

    use computeResult = new UnorderedAccessView(device, renderTarget.Resource)
    let context = device.ImmediateContext

    context.ComputeShader.Set(computeShader);
    context.ComputeShader.SetUnorderedAccessView(computeResult, 0);

    // set viewport
    context.Rasterizer.SetViewports(Viewport(0.0f, 0.0f, float32 width, float32 height, 0.0f, 1.0f))

    let constants = ShaderConstants<Constants>(device)
    let frame = ref 0

    // runs per frame
    MessagePump.Run(form, MainLoop(fun () ->
        // clear render buffer to black
        context.ClearRenderTargetView(renderTarget, Color4())
        // update shader variables
        constants.Update({frame = !frame})
        // run compute shader
        context.Dispatch(32, 32, 1);
        // copy output buffer to screen
        swapChain.Present(0, PresentFlags.None) |> ignore

        frame := !frame + width * height
    ))

    0 // return an integer exit code
