open SlimDX
open SlimDX.D3DCompiler
open SlimDX.Direct3D11
open SlimDX.Windows
open System.Runtime.InteropServices

type TriangleShader(device, elements, size) =
    let vertexShader, inputSignature = 
        let code = @"
            struct VShaderOutput {
	            float4 pos : SV_POSITION;
	            float4 color : COLOR;
            };

            VShaderOutput VShader(float2 pos : POSITION, float4 color : COLOR) {
                VShaderOutput output;
                output.pos = float4(pos, 1, 1);
                output.color = color;
                return output;
            }
        "
        use bytecode = ShaderBytecode.Compile(code, "VShader", "vs_5_0", ShaderFlags.Debug, EffectFlags.None)
        new VertexShader(device, bytecode), ShaderSignature.GetInputSignature(bytecode)

    let pixelShader = 
        let code = @"
            float4 PShader(float4 pos : SV_POSITION, float4 color : COLOR) : SV_Target {
	            return color;
            }
        "
        use bytecode = ShaderBytecode.Compile(code, "PShader", "ps_5_0", ShaderFlags.Debug, EffectFlags.None)
        new PixelShader(device, bytecode)

    let context = device.ImmediateContext

    member this.SetContext(device, vertexBuffer) =
        use layout  = new InputLayout(device, inputSignature, elements)

        context.InputAssembler.InputLayout <- layout
        context.InputAssembler.PrimitiveTopology <- PrimitiveTopology.TriangleList
        context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, size, 0))
        
        context.VertexShader.Set(vertexShader)
        context.PixelShader.Set(pixelShader)

// create random triangle positions and return them in a vertex buffer
let createTriangleVertexBuffer(device, count, size) =
    let bufferSizeInBytes = count * size * 3
    let stream = new DataStream(int64 bufferSizeInBytes, true, true)
    let rnd = System.Random()

    for i in 1 .. size * 3 do
        stream.Write(float32(rnd.NextDouble()) * 2.0f - 1.0f) // x
        stream.Write(float32(rnd.NextDouble()) * 2.0f - 1.0f) // y
        stream.Write(rnd.Next()) // color

    stream.Position <- 0L
    new Buffer(device, stream, bufferSizeInBytes, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)

open SlimDX.DXGI

[<EntryPoint>]
let main args =
    use form = new RenderForm("Test Window")
    let width, height = 400, 400
    let triangleCount = 20

    do  form.SetBounds(0, 0, width, height)

    let swapChainDescription = 
        SwapChainDescription(
            BufferCount       = 1,
            Usage             = (Usage.RenderTargetOutput),
            OutputHandle      = form.Handle,
            IsWindowed        = true,
            ModeDescription   = ModeDescription(0, 0, Rational(60, 1), Format.R8G8B8A8_UNorm),
            SampleDescription = SampleDescription(1, 0))
    
    let result, device, swapChain = 
        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, [|FeatureLevel.Level_11_0|], swapChainDescription)

    use renderTarget = 
        use renderResource = Resource.FromSwapChain<Texture2D>(swapChain, 0)
        new RenderTargetView(device, renderResource)
    
    let vertexLayout = [| new InputElement("POSITION", 0, DXGI.Format.R32G32_Float, 0, 0);
                          new InputElement("COLOR", 0, DXGI.Format.R8G8B8A8_UNorm, 8, 0) |]
    let vertexSizeInBytes = 12 
    let shader = new TriangleShader(device, vertexLayout, vertexSizeInBytes)
    let vertexBuffer = createTriangleVertexBuffer(device, triangleCount, vertexSizeInBytes);

    let context = device.ImmediateContext

    // set viewport
    context.Rasterizer.SetViewports(Viewport(0.0f, 0.0f, float32 width, float32 height, 0.0f, 1.0f))
    // disable triangle culling and set fill mode to solid
    context.Rasterizer.State <- RasterizerState.FromDescription(device, RasterizerStateDescription(CullMode = CullMode.None, FillMode = FillMode.Solid))
    // set render target
    context.OutputMerger.SetTargets(renderTarget)

    // runs per frame
    MessagePump.Run(form, MainLoop(fun () ->
        // clear render buffer to black
        context.ClearRenderTargetView(renderTarget, Color4())
        // bind vertex data i.e. where the triangles should be drawn
        shader.SetContext(device, vertexBuffer)
        // draw triangles
        context.Draw(triangleCount, 0)
        // copy output buffer to screen
        swapChain.Present(0, PresentFlags.None) |> ignore
    ))

    0 // return an integer exit code