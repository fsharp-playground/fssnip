module Varak.UI.GLTools

// Tools for rendering via OpenGL.
// Disposable types in this module must be disposed manually.
// Functions: checkGLError
// Types: GLHandle, Shader, Texture, TextureID, TexArray, IndexArray, RectRenderer, RenderWindow

open Varak
open System
open System.Drawing
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4



/// Fails and logs if there is an OpenGL error, including the error itself and the string passed.
let checkGLError where =
    let error = GL.GetError()
    if error <> ErrorCode.NoError then
        let msg = "OGL Error at " + where + ": " + error.ToString()
        IO.File.WriteAllText("zz_OGLERROR.txt", msg)
        failwith msg



/// Manages an OpenGL handle. The !! operator and the GLID member return the raw ID.
type GLHandle (ownerType, glid : int, deleter) =
    let mutable glid = Some(glid)

    member this.GLID =
        match glid with
        | Some(id) -> id
        | None -> failwith ("Tried to access disposed GLHandle created by " + ownerType.ToString())

    interface IDisposable with
        member this.Dispose () =
            glid |> Option.iter (fun i ->
                deleter i
                GC.SuppressFinalize(this)
                glid <- None)
    override this.Finalize () =
        failwith ("GLHandle created by " + ownerType.ToString() + " was not disposed properly.")

    static member (!!) (handle : GLHandle) = handle.GLID



/// GLSL shader routine as a disposable object.
type Shader (shaderType : ShaderType, source : string) as shader =
    inherit GLHandle(typeof<Shader>, GL.CreateShader(shaderType), GL.DeleteShader)
    do
        GL.ShaderSource(!!shader, source)
        GL.CompileShader(!!shader)
        if GL.GetShader(!!shader, ShaderParameter.CompileStatus) = 0 then
            failwith (shaderType.ToString() + " compilation error. Output: " + GL.GetShaderInfoLog(!!shader))
        checkGLError "MShader"



/// A single OpenGL texture
type Texture private (glid_in) =
    inherit GLHandle(typeof<Texture>, glid_in, GL.DeleteTexture)

    /// Factory that takes a Drawing.Bitmap, assuming premultiplied alpha for mipmap generation via OpenGL.
    static member FromBitmapPA (bmp : Bitmap) =
        let id = GL.GenTexture()
        GL.BindTexture(TextureTarget.Texture2D, id)
        // The following Imaging.PixelFormat is probably a lie.
        let bmpData = bmp.LockBits(Rectangle(0, 0, bmp.Width, bmp.Height),
                                   Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height,
                      0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0)
        bmp.UnlockBits(bmpData)

        // Enable and generate mipmaps
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.LinearMipmapLinear)
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)

        GL.BindTexture(TextureTarget.Texture2D, 0)
        checkGLError "Texture.FromBitmapPA"
        new Texture(id)

    /// Shorthand to load file and call FromBitmapPA
    static member FromFilePA (s : string) = new Bitmap(s) |> Texture.FromBitmapPA



/// A texture in a texture array, identified by its index.
type [<Struct>] TextureID (index : uint8) =
    member t.Index = index



/// An OpenGL array of identically sized textures.
type TexArray private (glid_in : int) =
    inherit GLHandle(typeof<TexArray>, glid_in, GL.DeleteTexture)

    /// Factory that takes a Bitmap array, assuming premultiplied alpha for mipmap generation via OpenGL.
    static member FromBitmapsPA (bmps : Bitmap list) =
        if bmps.IsEmpty then failwith "Empty texture arrays are currently not supported." else

        let w, h = bmps.Head.Width, bmps.Head.Height
        let id = GL.GenTexture()
        GL.BindTexture(TextureTarget.Texture2DArray, id)
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, Config.mipmapLevels, SizedInternalFormat.Rgba8, w, h, bmps.Length)

        bmps |> List.iteri (fun i bmp ->
            if bmp.Width <> w || bmp.Height <> h then
                failwith "Bitmap size for texture array generation must not vary."
            // The following Imaging.PixelFormat is probably a lie.
            let bmpData = bmp.LockBits(Rectangle(0, 0, bmp.Width, bmp.Height),
                                       Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, w, h, 1,
                             PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0)
            bmp.UnlockBits(bmpData)
            )

        // Enable and generate mipmaps
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, int TextureMinFilter.LinearMipmapLinear)
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray)

        GL.BindTexture(TextureTarget.Texture2DArray, 0)
        checkGLError "TexArray.FromBitmapPA"
        new TexArray(id)



/// An immutable buffer to hold vertex indices for rendering, also known as Index Buffer Object (IBO).
type IndexArray (data : int []) =
    let handle = new GLHandle(typeof<IndexArray>, GL.GenBuffer(), GL.DeleteBuffer)
    do
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, !!handle)
        GL.BufferData(BufferTarget.ElementArrayBuffer, nativeint (data.Length * sizeof<int>), data, BufferUsageHint.StaticDraw)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0)
        checkGLError "IndexBuffer init"

    member ia.GLID = !!handle

    interface IDisposable with
        member ia.Dispose () = dispose handle



/// Offers a quick way to render a single rectangle.
type RectRenderer () =
    let vertexShaderSrc =
        """ #version 400
            layout(location = 0) in vec2 pos;
            out vec2 texcoord;
            uniform vec4 texRect;
            uniform vec4 transform;

            void main() {
                vec2 tfPos = (pos * 2. - vec2(1., 1.)) * mat2(transform.xy, transform.zw);
                gl_Position = vec4(tfPos, 0, 1);
                texcoord = texRect.xy * (vec2(1.,1.) - pos) + texRect.zw * pos;
            }
        """

    let fragmentShaderSrc =
        """ #version 400
            in vec2 texcoord;
            out vec4 outColor;
            uniform sampler2D tex;

            void main() {
                outColor = texture(tex, texcoord);
            }
        """

    let programHandle =
        use vs = new Shader(ShaderType.VertexShader,     vertexShaderSrc)
        use fs = new Shader(ShaderType.FragmentShader, fragmentShaderSrc)
        let h = new GLHandle(typeof<RectRenderer>, GL.CreateProgram(), GL.DeleteProgram)
        GL.AttachShader(!!h, vs.GLID)
        GL.AttachShader(!!h, fs.GLID)
        GL.LinkProgram(!!h); checkGLError "SimpleRenderer program creation"
        if (GL.GetProgram(!!h, GetProgramParameterName.LinkStatus) <> 1) then
            failwith "SimpleRenderer shader program failed to link."
        h // MShaders get disposed here, but the shader program retains linked shaders until its deletion.
        
    let unLoc_texRect   = GL.GetUniformLocation(!!programHandle, "texRect")
    let unLoc_transform = GL.GetUniformLocation(!!programHandle, "transform")
        
    let vaoHandle = new GLHandle(typeof<RectRenderer>, GL.GenVertexArray(), GL.DeleteVertexArray)
    let vboHandle = new GLHandle(typeof<RectRenderer>, GL.GenBuffer(), GL.DeleteBuffer)
    let vertices = [| 0; 0; 1; 0; 0; 1; 1; 1 |] |> Array.map float32

    do
        GL.BindVertexArray(!!vaoHandle)
        GL.BindBuffer(BufferTarget.ArrayBuffer, !!vboHandle)
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32> |> nativeint, vertices, BufferUsageHint.StaticDraw)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(0)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)
        GL.BindVertexArray(0)

    /// Renders one rectangle. posTransform is applied to the centered maximum-size rectangle and does not affect texture coords.
    member this.Render (texture : Texture, texArea : Rectangle<1>, posTransform : mat2d<1>) =
        GL.BindVertexArray(!!vaoHandle);    checkGLError "Before RectRenderer.Render"
        GL.UseProgram(!!programHandle)
        let lc, hc = texArea.LowCorner, texArea.HighCorner
        GL.Uniform4(unLoc_texRect, lc.x, lc.y, hc.x, hc.y)
        GL.Uniform4(unLoc_transform, posTransform.c11, posTransform.c21, posTransform.c12, posTransform.c22)
        GL.BindTexture(TextureTarget.Texture2D, !!texture)
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4)
        GL.BindTexture(TextureTarget.Texture2D, 0)
        GL.UseProgram(0)
        GL.BindVertexArray(0);              checkGLError "After RectRenderer.Render"
    
    interface IDisposable with
        member this.Dispose () =
            dispose programHandle
            dispose vaoHandle
            dispose vboHandle



/// Manages the OpenGL context and executes the main rendering call. The initializer function is called after
/// OpenGL is set up, but before rendering begins and sets the initial ViewPartition. Derived from the OpenTK GameWindow.
type RenderWindow (initializer, width, height) =
    inherit GameWindow(width, height, GraphicsMode(ColorFormat(32), 0, 0, 4), "ProtoClient running OTK 1.1",
                       GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)

    let emptyView = {new IView with member v.Render _ = ()}
    member val Partition = ViewArea emptyView with get, set

    new (init) = new RenderWindow(init, 1280, 720)

    override this.OnLoad e =
        checkGLError "Before OnLoad"
        this.VSync <- VSyncMode.Adaptive
        GL.DepthFunc(DepthFunction.Lequal)
        GL.Enable(EnableCap.Blend)
        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha) // Premultiplied
        GL.ClearColor(Color.Black)
        checkGLError "RenderWindow.OnLoad"
        this.Partition <- initializer this
        checkGLError "RenderWindow.OnLoad initializer call"

    /// <summary>
    /// Called when it is time to render the next frame.
    /// </summary>
    /// <param name="e">Contains timing information.</param>
    override this.OnRenderFrame e =
        checkGLError "Before OnRenderFrame"
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        checkGLError "RenderWindow.OnRenderFrame GL.Clear"
        
        let screen = Rect.centered(2., 2.)
        let screenPx = vec2(this.ClientRectangle.Width, this.ClientRectangle.Height)
        let sglPerPx = Config.minScreenEdge / float (min screenPx.x screenPx.y)
        let screenSgl = Vec.d1 screenPx * sglPerPx
        let toPxCoords (v : vec2d<1>) =
            vec2((v.x / 2. + 0.5) * float screenPx.x, (v.y / 2. + 0.5) * float screenPx.y) |> Vec.ir

        for rect, view in this.Partition.Visibles screen do
            match Rect.intersection rect screen with
            | None -> ()
            | Some drawArea ->
                let corner = toPxCoords drawArea.LowCorner
                let sizePx = toPxCoords drawArea.HighCorner - corner
                GL.Viewport(corner.x, corner.y, sizePx.x, sizePx.y); checkGLError "OnRenderFrame GL.Viewport"
                view.Render(drawArea.Diagonal /> 2. .* screenSgl |> Vec.f)

        this.SwapBuffers()
        checkGLError "OnRenderFrame end"