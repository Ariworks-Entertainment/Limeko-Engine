using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ImGuiTest;

/// <summary>
/// Manages the Dear ImGui OpenGL3 backend for OpenTK 4.
/// Call NewFrame() before building UI, and Render() after.
/// </summary>
public sealed class ImGuiController : IDisposable
{
    private readonly GameWindow _window;

    // OpenGL resources
    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _shader;
    private int _fontTexture;

    // Shader uniform locations
    private int _uProjection;
    private int _uTexture;

    // Attribute locations
    private int _aPosition;
    private int _aUV;
    private int _aColor;

    private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;
    private bool _disposed;

    public ImGuiController(GameWindow window)
    {
        _window = window;

        var ctx = ImGui.CreateContext();
        ImGui.SetCurrentContext(ctx);

        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        SetPerFrameImGuiData(1f / 60f);
        CreateDeviceResources();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void NewFrame(double deltaTime)
    {
        SetPerFrameImGuiData((float)deltaTime);
        UpdateInput();
        ImGui.NewFrame();
    }

    public void Render()
    {
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    public void WindowResized(int width, int height)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(width, height);
    }

    // Called by the window's TextInput event
    public void PressChar(char c) => ImGui.GetIO().AddInputCharacter(c);

    // -------------------------------------------------------------------------
    // Per-frame setup
    // -------------------------------------------------------------------------

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _window.ClientSize.X / _scaleFactor.X,
            _window.ClientSize.Y / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds;
    }

    // -------------------------------------------------------------------------
    // Input — ImGui.NET 1.90+ uses AddKeyEvent / AddMouseButtonEvent, not arrays
    // -------------------------------------------------------------------------

    private void UpdateInput()
    {
        var io = ImGui.GetIO();
        var mouse = _window.MouseState;
        var kb = _window.KeyboardState;

        // Mouse position
        io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

        // Mouse buttons
        io.AddMouseButtonEvent(0, mouse[MouseButton.Left]);
        io.AddMouseButtonEvent(1, mouse[MouseButton.Right]);
        io.AddMouseButtonEvent(2, mouse[MouseButton.Middle]);

        // Scroll wheel
        if (mouse.ScrollDelta.Y != 0f)
            io.AddMouseWheelEvent(0f, mouse.ScrollDelta.Y);

        // Modifier keys (still set directly on io in 1.90)
        io.KeyCtrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
        io.KeyAlt = kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt);
        io.KeyShift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
        io.KeySuper = kb.IsKeyDown(Keys.LeftSuper) || kb.IsKeyDown(Keys.RightSuper);

        // Key events via AddKeyEvent
        foreach ((Keys glfw, ImGuiKey imgui) in s_keyMap)
        {
            if (kb.IsKeyPressed(glfw))
                io.AddKeyEvent(imgui, true);
            else if (kb.IsKeyReleased(glfw))
                io.AddKeyEvent(imgui, false);
        }
    }

    // OpenTK Keys -> ImGuiKey (replaces the removed io.KeyMap[] array)
    private static readonly (Keys Glfw, ImGuiKey Imgui)[] s_keyMap =
    [
        (Keys.Tab,          ImGuiKey.Tab),
        (Keys.Left,         ImGuiKey.LeftArrow),
        (Keys.Right,        ImGuiKey.RightArrow),
        (Keys.Up,           ImGuiKey.UpArrow),
        (Keys.Down,         ImGuiKey.DownArrow),
        (Keys.PageUp,       ImGuiKey.PageUp),
        (Keys.PageDown,     ImGuiKey.PageDown),
        (Keys.Home,         ImGuiKey.Home),
        (Keys.End,          ImGuiKey.End),
        (Keys.Insert,       ImGuiKey.Insert),
        (Keys.Delete,       ImGuiKey.Delete),
        (Keys.Backspace,    ImGuiKey.Backspace),
        (Keys.Space,        ImGuiKey.Space),
        (Keys.Enter,        ImGuiKey.Enter),
        (Keys.KeyPadEnter,  ImGuiKey.KeypadEnter),
        (Keys.Escape,       ImGuiKey.Escape),
        (Keys.A,            ImGuiKey.A),
        (Keys.C,            ImGuiKey.C),
        (Keys.V,            ImGuiKey.V),
        (Keys.X,            ImGuiKey.X),
        (Keys.Y,            ImGuiKey.Y),
        (Keys.Z,            ImGuiKey.Z),
        (Keys.LeftControl,  ImGuiKey.LeftCtrl),
        (Keys.RightControl, ImGuiKey.RightCtrl),
        (Keys.LeftShift,    ImGuiKey.LeftShift),
        (Keys.RightShift,   ImGuiKey.RightShift),
        (Keys.LeftAlt,      ImGuiKey.LeftAlt),
        (Keys.RightAlt,     ImGuiKey.RightAlt),
        (Keys.LeftSuper,    ImGuiKey.LeftSuper),
        (Keys.RightSuper,   ImGuiKey.RightSuper),
    ];

    // -------------------------------------------------------------------------
    // OpenGL device resource creation
    // -------------------------------------------------------------------------

    private void CreateDeviceResources()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        const string vertSrc = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec4 aColor;
uniform mat4 uProjection;
out vec2 fUV;
out vec4 fColor;
void main()
{
    fUV         = aUV;
    fColor      = aColor;
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}";

        const string fragSrc = @"
#version 330 core
in vec2 fUV;
in vec4 fColor;
uniform sampler2D uTexture;
out vec4 outColor;
void main()
{
    outColor = fColor * texture(uTexture, fUV);
}";

        int vert = CompileShader(ShaderType.VertexShader, vertSrc);
        int frag = CompileShader(ShaderType.FragmentShader, fragSrc);

        _shader = GL.CreateProgram();
        GL.AttachShader(_shader, vert);
        GL.AttachShader(_shader, frag);
        GL.LinkProgram(_shader);
        GL.GetProgram(_shader, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
            throw new Exception($"ImGui shader link error: {GL.GetProgramInfoLog(_shader)}");

        GL.DeleteShader(vert);
        GL.DeleteShader(frag);

        _uProjection = GL.GetUniformLocation(_shader, "uProjection");
        _uTexture = GL.GetUniformLocation(_shader, "uTexture");
        _aPosition = GL.GetAttribLocation(_shader, "aPosition");
        _aUV = GL.GetAttribLocation(_shader, "aUV");
        _aColor = GL.GetAttribLocation(_shader, "aColor");

        RecreateFontDeviceTexture();
    }

    public void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private static int CompileShader(ShaderType type, string src)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, src);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
            throw new Exception($"ImGui {type} compile error: {GL.GetShaderInfoLog(shader)}");
        return shader;
    }

    // -------------------------------------------------------------------------
    // Render
    // -------------------------------------------------------------------------

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0) return;

        // Save GL state
        int[] lastViewport = new int[4];
        int[] lastScissor = new int[4];
        GL.GetInteger(GetPName.Viewport, lastViewport);
        GL.GetInteger(GetPName.ScissorBox, lastScissor);
        bool lastBlend = GL.IsEnabled(EnableCap.Blend);
        bool lastCullFace = GL.IsEnabled(EnableCap.CullFace);
        bool lastDepthTest = GL.IsEnabled(EnableCap.DepthTest);
        bool lastScissorTest = GL.IsEnabled(EnableCap.ScissorTest);

        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);

        var io = ImGui.GetIO();
        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;

        float[] projection =
        [
             2f/(R-L),    0,           0, 0,
             0,           2f/(T-B),    0, 0,
             0,           0,          -1, 0,
            (R+L)/(L-R), (T+B)/(B-T), 0, 1,
        ];

        GL.UseProgram(_shader);
        GL.Uniform1(_uTexture, 0);
        GL.UniformMatrix4(_uProjection, 1, false, projection);

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.EnableVertexAttribArray(_aPosition);
        GL.EnableVertexAttribArray(_aUV);
        GL.EnableVertexAttribArray(_aColor);
        GL.VertexAttribPointer(_aPosition, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(_aUV, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(_aColor, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        System.Numerics.Vector2 clipOff = drawData.DisplayPos;
        System.Numerics.Vector2 clipScale = drawData.FramebufferScale;

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];

            GL.BufferData(BufferTarget.ArrayBuffer,
                cmdList.VtxBuffer.Size * stride,
                cmdList.VtxBuffer.Data, BufferUsageHint.StreamDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                cmdList.IdxBuffer.Size * sizeof(ushort),
                cmdList.IdxBuffer.Data, BufferUsageHint.StreamDraw);

            for (int cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                var cmd = cmdList.CmdBuffer[cmdIdx];
                if (cmd.UserCallback != IntPtr.Zero) continue;

                var clipMin = new System.Numerics.Vector2(
                    (cmd.ClipRect.X - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                var clipMax = new System.Numerics.Vector2(
                    (cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.W - clipOff.Y) * clipScale.Y);

                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y) continue;

                GL.Scissor(
                    (int)clipMin.X,
                    (int)(io.DisplaySize.Y - clipMax.Y),
                    (int)(clipMax.X - clipMin.X),
                    (int)(clipMax.Y - clipMin.Y));

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, (int)cmd.TextureId);

                GL.DrawElementsBaseVertex(
                    PrimitiveType.Triangles,
                    (int)cmd.ElemCount,
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(cmd.IdxOffset * sizeof(ushort)),
                    (int)cmd.VtxOffset);
            }
        }

        // Restore GL state
        GL.DisableVertexAttribArray(_aPosition);
        GL.DisableVertexAttribArray(_aUV);
        GL.DisableVertexAttribArray(_aColor);
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.UseProgram(0);

        if (!lastBlend) GL.Disable(EnableCap.Blend);
        if (lastCullFace) GL.Enable(EnableCap.CullFace);
        if (lastDepthTest) GL.Enable(EnableCap.DepthTest);
        if (!lastScissorTest) GL.Disable(EnableCap.ScissorTest);

        GL.Viewport(lastViewport[0], lastViewport[1], lastViewport[2], lastViewport[3]);
        GL.Scissor(lastScissor[0], lastScissor[1], lastScissor[2], lastScissor[3]);
    }

    // -------------------------------------------------------------------------
    // Disposal
    // -------------------------------------------------------------------------

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteProgram(_shader);
        GL.DeleteTexture(_fontTexture);

        ImGui.DestroyContext();
    }
}