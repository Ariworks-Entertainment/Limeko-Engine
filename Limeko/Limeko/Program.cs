using Avalonia;
using BepuPhysics.Collidables;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Limeko
{
    public class Core
    {
        public static string Version = "dev-0.0.0-alpha";

        /// <summary>
        /// The static instance of this program's active window.
        /// </summary>
        public static Window WindowInstance { get; private set; }

        public static void Main()
        {
            // Editor.SplashScreen.Show();

            Console.Title = "Limeko Console";
            WindowInstance = new Window();
            WindowInstance.Run();
        }

        public class Window : GameWindow
        {
            float _deltaTime;
            float _fixedDeltaTime;

            public static Vector2 WindowSize;

            public static int targetFrameRate = 90;


            static GameWindowSettings gameSettings = new GameWindowSettings()
            {
                UpdateFrequency = targetFrameRate
            };

            static NativeWindowSettings windowSettings = new NativeWindowSettings()
            {
                MinimumClientSize = new Vector2i(920, 550),
                ClientSize = new Vector2i(1560, 960),
                WindowState = WindowState.Normal,
                Vsync = VSyncMode.On,
                Title = "Limeko",
                StartVisible = false
            };


            public Window() : base(gameSettings, windowSettings)
            { }

            protected override async void OnLoad()
            {
                base.OnLoad();

                // Initialize the Editor window.
                // UI, Editor subsystems, etc.
                // Do *not* Initialize Physics--that's for runtime.

                // Slowly learning from my mistakes.

                // start internal stuff.
                Editor.InitializeCore();


                Console.WriteLine("<--> Starting Editor (Internal) <-->");

                // sizing
                WindowSize = new Vector2(Size.X, Size.Y);

                // general configuration
                Rendering.ConfigureOpenGL();

                Console.WriteLine("> Setting background");
                // slightly above black to avoid confusion
                GL.ClearColor(0.04f, 0.04f, 0.04f, 1f);

                Console.WriteLine("OpenGL Core Running.");




                Console.Clear();

                Editor.Utils.Misc.PrintLimeko(true);
                Editor.Utils.Misc.PrintLicenseDisclaimer();
                Console.WriteLine("");
                Editor.Utils.Misc.PrintVersionInfo();

                Console.WriteLine("\n\n#= Dev-Stats =#\n");

                Console.WriteLine($"> Project Path: {Editor.Utils.GetActiveProjectPath()}");
                Console.WriteLine($"> Default Project Path: {Editor.Utils.GetDefaultProjectPath()}\n");
                Console.WriteLine($"| Found {Editor.projects.Count} Projects:");
                foreach(string project in Editor.projects) Console.WriteLine($"| > {project.Split("\\").Last()}");
                Console.WriteLine("");

                Console.WriteLine("Use '-c (Project-Name)' to Create a new Project.");
                Console.Write("Load Project: ");
                string projectToLoad = Console.ReadLine(); // <-- TEMPORARY
                // eventually replace this code with logic for Dear ImGUI stuff.

                if(projectToLoad.Contains("-c"))
                {
                    string newProjectName = projectToLoad.Replace("-c", "").Trim();
                    await Editor.CreateProject(newProjectName);
                    this.IsVisible = true;
                    return;
                }

                foreach(string project in Editor.projects)
                {
                    if(project.Contains(projectToLoad, StringComparison.OrdinalIgnoreCase))
                    {
                        await Editor.LoadProject(project);
                        this.IsVisible = true;
                        return;
                    }
                }
                Console.WriteLine("Fail");
                this.Close();
                this.Dispose();
            }

            protected override void OnUnload()
            {
                base.OnUnload();
                // Dispose of all Shaders, free any assets, etc.
                // _shader.Dispose();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base .OnUpdateFrame(e);
                // Runs every frame.

                // Should only execute during runtime.
                /*
                Input.Update(); // Update Input before components!!
                EntitySystem.Update();
                */
            }

            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, Size.X, Size.Y);
                WindowSize = new Vector2(Size.X, Size.Y);
                Console.WriteLine($"Resized: {Size}");
            }

            protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs e)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Matrix4 model =
                    Matrix4.CreateRotationY(_deltaTime) *
                    Matrix4.CreateRotationX(_deltaTime * 0.5f);


                // EDITOR VIEW CAMERA
                /*
                Vector3 editorCameraForward = new Vector3(
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians()) *
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch)),
                    MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch)),
                    MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_yaw)) *
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch))
                );

                Matrix4 view = Matrix4.LookAt(_cameraPosition, _cameraPosition + Vector3.Normalize(editorCameraForward), Vector3.UnitY);

                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(OpenTK.Mathematics.MathHelper.DegreesToRadians(70f), Size.X / (float)Size.Y, 0.01f, 100f);


                Rendering.Update(view, projection);
                */

                SwapBuffers();
            }

            public void OnSettingsUpdated()
            {
                this.UpdateFrequency = (float)targetFrameRate;
            }

            public void ReloadAssets()
            {
                Console.WriteLine("Not Implemented.");
            }
        }
    }

    public class Input
    {
        // not implemented

        // supports multiple keyboards, although the current input method-
        // -does not support multiple keyboards.
        // Maybe switch to an input library?


        // mouse control
        Vector2 _lastMouse;
        bool _firstMove = true;
        float _sensitivity = 0.15f;

        public static Dictionary<Keyboard, KeyboardState> keyboards = new();

        /// <summary>
        /// An internal method for updating inputs. Do not call this directly!
        /// </summary>
        public static void Update()
        {
            
        }

        // Inefficient but functional
        public enum Key
        {
            Q,W,E,R,T,Y,U,I,O,P,A,S,D,F,G,H,J,K,L,Z,X,C,V,B,N,M,ZERO,ONE,TWO,THREE,FOUR,FIVE,SIX,SEVEN,EIGHT,NINE,ESCAPE,COMMA,PERIOD,COLON,QUOTE
        }

        public class Keyboard
        {
            public virtual void OnKeyDown(Key key)
            {

            }
        }
    }

    public class Rendering
    {
        // Logic

        private static List<Renderer> registeredRenderers = new();

        /// <summary>
        /// Configures OpenGL to not render backfaces, allow transparency, etc.
        /// </summary>
        public static void ConfigureOpenGL()
        {
            GL.Enable(EnableCap.CullFace); // don't cull faces we can't see
            GL.Enable(EnableCap.Blend); // allow transparency
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // some sort of blending for transparency(?)
            GL.FrontFace(FrontFaceDirection.Ccw); // keep counter-clockwise faces
        }

        public static void Register(Renderer renderer)
        {
            registeredRenderers.Add(renderer);
        }
        public static void Register(Renderer renderer, out int index)
        {
            registeredRenderers.Add(renderer);
            index = registeredRenderers.IndexOf(renderer);
        }

        public static void Unregister(Renderer renderer)
        {
            registeredRenderers.Remove(renderer);
        }
        public static void Unregister(int index)
        {
            registeredRenderers.RemoveAt(index);
        }

        /// <summary>
        /// Renders any Registered objects.
        /// </summary>
        public static void Update(Matrix4 view, Matrix4 projection)
        {
            // PER-OBJECT RENDERING
            foreach (var obj in registeredRenderers)
            {
                if(obj.Mesh == null)
                {
                    Unregister(obj); continue;
                }

                // Still need to complete & add the Shader class.
                // obj.Material.Bind();

                var shader = obj.Material.Shader;

                shader.SetInt("uLightCount", 1);

                shader.SetVector3("uLightDirs[0]",
                    Vector3.Normalize(new Vector3(-0.3f, -1f, -0.2f)));

                shader.SetVector3("uLightColors[0]", Vector3.One);
                shader.SetFloat("uLightIntensity[0]", 1.0f);

                // optional but important
                shader.SetFloat("uAmbient", 0.2f);

                obj.Material.Shader.SetMatrix4("uModel", obj.GetMatrix());
                obj.Material.Shader.SetMatrix4("uView", view);
                obj.Material.Shader.SetMatrix4("uProjection", projection);

                obj.Mesh.Draw(PrimitiveType.Triangles);
            }
        }


        // Components

        public class Renderer
        {
            public required EntitySystem.Entity Entity;
            public Material Material = new();
            public Mesh? Mesh;

            public Vector3 PositionOffset;
            public Quaternion RotationOffset;
            public Vector3 ScaleOffset = Vector3.One;

            public Matrix4 GetMatrix()
            {
                Vector3 rotatedOffset =
                    Vector3.Transform(PositionOffset,
                    Entity.Transform.Rotation);

                return
                    Matrix4.CreateScale(Entity.Transform.Scale * ScaleOffset) *
                    Matrix4.CreateFromQuaternion(Entity.Transform.Rotation * RotationOffset) *
                    Matrix4.CreateTranslation(Entity.Transform.Position + rotatedOffset);
            }
        }

        /// <summary>
        /// Holds a Shader, and displays instanced variables for it.
        /// (Per-Material Shader Instance Control)
        /// </summary>
        public class Material
        {
            // creating a new material defaults to Lit.
            public Material()
            {
                // Shader = Renderer.DefaultLit();
            }

            public Shader Shader;
        }

        /// <summary>
        /// Holds data about how things should be rendered, shaded, textured, and colored.
        /// </summary>
        public class Shader
        {
            public int Handle { get; private set; }

            public Shader(string vertPath, string fragPath)
            {
                string vertSource = File.ReadAllText(vertPath);
                string fragSource = File.ReadAllText(fragPath);

                // --- Vertex shader ---
                int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, vertSource);
                GL.CompileShader(vertexShader);
                CheckShader(vertexShader);

                // --- Fragment shader ---
                int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, fragSource);
                GL.CompileShader(fragmentShader);
                CheckShader(fragmentShader);

                // --- Program ---
                Handle = GL.CreateProgram();
                GL.AttachShader(Handle, vertexShader);
                GL.AttachShader(Handle, fragmentShader);
                GL.LinkProgram(Handle);

                GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
                if (success == 0)
                    throw new Exception(GL.GetProgramInfoLog(Handle));

                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
            }

            public void Use()
            {
                GL.UseProgram(Handle);
            }

            public void Dispose()
            {
                GL.DeleteProgram(Handle);
            }

            public void SetFloat(string name, float value)
            {
                int location = Uniforms.GetUniformLocation(name, Handle);
                if (location != -1) GL.Uniform1(location, value);
            }

            public void SetColor(string name, Vector3 rgb)
            {
                int location = Uniforms.GetUniformLocation(name, Handle);
                if (location != -1) GL.Uniform3(location, rgb);
            }

            public void SetMatrix4(string name, Matrix4 value)
            {
                int location = Uniforms.GetUniformLocation(name, Handle);
                if (location != -1)
                    GL.UniformMatrix4(location, false, ref value);
            }

            public void SetVector3(string name, Vector3 value)
            {
                int location = Uniforms.GetUniformLocation(name, Handle);
                if (location != -1) GL.Uniform3(location, value);
            }

            public void SetInt(string name, int value)
            {
                GL.Uniform1(Uniforms.GetUniformLocation(name, Handle), value);
            }

            private void CheckShader(int shader)
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
                if (success == 0) throw new Exception(GL.GetShaderInfoLog(shader));
            }
        }

        public class Mesh
        {
            int _vao;
            int _vbo;
            int _vertexCount;

            public Mesh(float[] vertices)
            {
                _vertexCount = vertices.Length / 8;

                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();

                GL.BindVertexArray(_vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    vertices.Length * sizeof(float),
                    vertices,
                    BufferUsageHint.StaticDraw);

                int stride = 8 * sizeof(float);

                // position
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                    false, stride, 0);
                GL.EnableVertexAttribArray(0);

                // normal
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
                    false, stride, 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                // uv
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float,
                    false, stride, 6 * sizeof(float));
                GL.EnableVertexAttribArray(2);
            }

            public void Draw(PrimitiveType type = PrimitiveType.Triangles)
            {
                GL.BindVertexArray(_vao);
                GL.DrawArrays(type, 0, _vertexCount);
            }
        }

        public static class Uniforms
        {
            static Dictionary<(int, string), int> _uniformCache = new();

            public static int GetUniformLocation(string name, int program)
            {
                var key = (program, name);
                if (_uniformCache.TryGetValue(key, out int loc)) return loc;

                loc = GL.GetUniformLocation(program, name);
                _uniformCache[key] = loc;
                return loc;
            }

            public static void ClearUniformCache()
            {
                _uniformCache.Clear();
            }
        }
    }

    public class EntitySystem
    {
        public static event EventHandler OnUpdate;

        public static void Awake()
        {
            OnUpdate = new EventHandler(OnUpdate);
        }

        public static void Update()
        {
            OnUpdate.Invoke(null, EventArgs.Empty);
        }


        /// <summary>
        /// The base class for every object.
        /// Serves as a 'GameObject' component.
        /// </summary>
        public class Entity
        {
            public required string Name;
            public required int Identifier;
            public Transform Transform = new();
        }

        /// <summary>
        /// Controls the position, rotation, and scale of an entity, and additionally all of it's children.
        /// </summary>
        public class Transform
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }
    }

    public class Physics
    {
        /// <summary>
        /// The amount of gravity objects will experience in M/s.
        /// </summary>
        public static Vector3 gravity = new Vector3(0f, 9.80665f, 0f);
        // not fully implemented

        public void StartSimulation()
        {
            throw new Exception("Fuck naw");
        }
    }

    public class Levels
    {
        public static void CreateNew()
        {

        }
    }

    public class Audio
    {
        /// <summary>
        /// Plays Audio in both 2D stereo space and 3D world space.
        /// </summary>
        public class Speaker
        {
            public AudioTrack? track;
            public float volume;
            public float pitch;

            public float spatialMix = 0f;
            // not implemented
        }

        /// <summary>
        /// A generalized class for all supported audio types. (.mp3, .wav, etc.)
        /// </summary>
        public class AudioTrack
        {
            // not implemented
            /*
            public AudioCodec codec { get; private set; }
            public byte[] audioData;
            */
        }
    }

    public class Editor
    {
        public static bool isProjectOpen;
        // the currently open project.
        public static string activeProjectPath = "";

        // the default location new projects are created at.
        public static string defaultProjectPath = "";

        public static List<string> projects = new();

        /// <summary>
        /// Initializes core User and Engine data.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static void InitializeCore()
        {
            // Configure and Assign the Default Project Path.
            // Eventually support settings like a custom path.
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dP = Path.Combine(programData, "Limeko/Projects");
            if (!Directory.Exists(dP)) Directory.CreateDirectory(dP);
            defaultProjectPath = dP;

            projects = Directory.GetDirectories(defaultProjectPath).ToList();
        }


        /// <summary>
        /// Initializes the core Editor logic.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static void InitializeEditor()
        {

        }

        /// <summary>
        /// Loads an existing Project, given one is not open.
        /// Internal Method--Don't call directly!
        /// </summary>
        /// <param name="path"></param>
        public static async Task LoadProject(string path)
        {
            if(!projects.Contains(path)) return;
            Console.WriteLine($"Loading {path.Split("\\").Last()}...");
            Stopwatch loadTime = new Stopwatch();
            loadTime.Start();

            // load
            int assetCount = 0;
            await Task.Delay(2000); // temporary

            loadTime.Stop();
            activeProjectPath = path;
            Console.WriteLine($"\n\nLoaded!");
            Console.WriteLine($"Loaded {assetCount} assets");
            Console.WriteLine($"Took {loadTime.Elapsed.Minutes} minutes and {(loadTime.Elapsed.Seconds)} seconds");
        }

        public static async Task CreateProject(string name)
        {
            string newProjectPath = Path.Combine(defaultProjectPath, name);
            if (Directory.Exists(newProjectPath))
            {
                Console.WriteLine("A project with that name already exists. Load it?");
                Console.Write("[y/n]: "); if(Console.ReadLine().Trim().ToLower() == "y")
                {
                    await LoadProject(newProjectPath);
                    return;
                }
                return;
            }
            Directory.CreateDirectory(newProjectPath);
            // create subdirectories, default assets, etc.
            await Task.Delay(1000); // temporary
            Console.WriteLine($"Created project '{name}' at {newProjectPath}");
        }

        /// <summary>
        /// Unloads the currently open Project, given one is open.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static void UnloadProject()
        {
            if(Editor.Utils.GetActiveProjectPath() != null)
            {
                // Dispose & Unload.
            }
        }

        public static class SplashScreen
        {
            public static void Show()
            {
                // No Logic
            }
        }

        public static class Utils
        {
            public static string GetActiveProjectPath()
            {
                string? path = Editor.isProjectOpen && !string.IsNullOrEmpty(Editor.activeProjectPath) ? Editor.activeProjectPath : null;
                return path;
            }

            public static string GetDefaultProjectPath()
            {
                string? path = !string.IsNullOrEmpty(Editor.defaultProjectPath) ? Editor.defaultProjectPath : null;
                return path;
            }

            public static class Misc
            {
                public static void PrintLimeko(bool spacer)
                {
                    if (spacer) Console.WriteLine("");
                    Console.WriteLine("                                                           .-'''-.     ");
                    Console.WriteLine(".---.                                                     '   _    \\   ");
                    Console.WriteLine("|   |.--. __  __   ___         __.....__          .     /   /` '.   \\  ");
                    Console.WriteLine("|   ||__||  |/  `.'   `.   .-''         '.      .'|    .   |     \\  '  ");
                    Console.WriteLine("|   |.--.|   .-.  .-.   ' /     .-''\"'-.  `.  .'  |    |   '      |  ' ");
                    Console.WriteLine("|   ||  ||  |  |  |  |  |/     /________\\   \\<    |    \\    \\     / /  ");
                    Console.WriteLine("|   ||  ||  |  |  |  |  ||                  | |   | ____`.   ` ..' /   ");
                    Console.WriteLine("|   ||  ||  |  |  |  |  |\\    .-------------' |   | \\ .'   '-...-'`    ");
                    Console.WriteLine("|   ||  ||  |  |  |  |  | \\    '-.____...---. |   |/  .                ");
                    Console.WriteLine("|   ||__||__|  |__|  |__|  `.             .'  |    /\\  \\               ");
                    Console.WriteLine("'---'                        `''-...... -'    |   |  \\  \\              ");
                    Console.WriteLine("                                              '    \\  \\  \\             ");
                    Console.WriteLine("                                             '------'  '---'           ");
                    if (spacer) Console.WriteLine("");
                }

                public static void PrintLicenseDisclaimer()
                {
                    Console.WriteLine("Limeko-Engine  Copyright (C) 2026  lunark");
                    Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
                    Console.WriteLine("This is free software, and you are welcome to redistribute it.");
                    Console.WriteLine("under certain conditions. Press F9 to learn more.");
                }

                public static void PrintVersionInfo()
                {
                    Console.WriteLine($"Version {Core.Version}");
                    switch(Core.Version.Split('-').Last())
                    {
                        case "alpha":
                            Console.WriteLine("You are running an ALPHA version of Limeko. Don't expect a flawless experience.");
                            break;
                        case "beta":
                            Console.WriteLine("You are running a BETA version of Limeko. Issues are to be expected.");
                            break;
                        case "stable":
                            Console.WriteLine("You are running a STABLE version of Limeko.");
                            break;
                        default:
                            Console.WriteLine("Unknown version type.");
                            break;
                    }
                }

                public static void OpenWebpage(string url)
                {
                    ProcessStartInfo info = new ProcessStartInfo
                    { FileName = url, UseShellExecute = true };
                    Process.Start(info);
                }
            }
        }
    }
}