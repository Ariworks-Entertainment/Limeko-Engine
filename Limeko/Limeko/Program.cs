using Avalonia;
using BepuPhysics.Collidables;
using ImGuiNET;
using ImGuiTest;
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
        public static Window? WindowInstance { get; private set; }

        public static ImGuiController? ImGuiController = null;


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

                // create ImGUI Controller
                ImGuiController = new ImGuiController(this);

                // general configuration
                Rendering.ConfigureOpenGL();
                GL.ClearColor(0.56f, 1f, 0.56f, 1f);

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
                this.IsVisible = true;

                // Console.WriteLine("Use '-c (Project-Name)' to Create a new Project.");
                // Console.Write("Load Project: ");
                // string projectToLoad = Console.ReadLine(); // <-- TEMPORARY
                // eventually replace this code with logic for Dear ImGUI stuff.

                /*
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
                */
            }

            protected override void OnUnload()
            {
                base.OnUnload();
                // Dispose of all Shaders, free any assets, etc.
                // _shader.Dispose();
                ImGuiController.Dispose();
            }

            protected override void OnTextInput(TextInputEventArgs e)
            {
                base.OnTextInput(e);
                ImGuiController.PressChar((char)e.Unicode);
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base .OnUpdateFrame(e);
                // Runs every frame.

                ImGuiController.NewFrame(e.Time);

                Editor.Update();

                // Should only execute during runtime.
                /*
                Input.Update(); // Update Input before components!!
                EntitySystem.Update();
                */
            }

            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, e.Width, e.Height);
                WindowSize = new Vector2(e.Width, e.Height);
                ImGuiController.WindowResized(e.Width, e.Height);

                Console.WriteLine($"Resized: {e.Width}x {e.Height}y");
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

                // Rendering.Update();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                ImGuiController.Render();

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


        private static string newProjectName = "";
        private static string newProjectDeveloper = "";

        private static bool working = false;
        private static bool projectMenu = true;
        private static bool createProjectMenu = false;

        /// <summary>
        /// Runs every frame while the Editor is open.
        /// Handles Editor UI, Project management, etc.
        /// </summary>
        public static void Update()
        {
            var viewport = ImGui.GetMainViewport();
            var center = viewport.GetCenter();

            BeginTheme();

            if(projectMenu && !isProjectOpen)
            {
                ImGui.SetNextWindowPos(center, ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
                ImGui.Begin("Projects", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.SetWindowSize(new System.Numerics.Vector2(682, 400));

                ImGui.Text("Project Browser");
                ImGui.Separator();
                ImGui.Dummy(new System.Numerics.Vector2(0, 6));

                if (ImGui.Button("Create New Project"))
                {
                    createProjectMenu = true;
                    projectMenu = false;
                }

                ImGui.Dummy(new System.Numerics.Vector2(0, 6));
                ImGui.SeparatorText("OR");
                ImGui.Dummy(new System.Numerics.Vector2(0, 6));

                ImGui.Text("Select a project to load:");
                ImGui.BeginListBox(" ");
                foreach (string project in projects)
                {
                    if(ImGui.Selectable(project.Split("\\").Last()))
                    {
                        working = true;
                        LoadProject(project).Wait();
                        projectMenu = false;
                    }
                }
                ImGui.EndListBox();

                ImGui.End();
            }
            else if(createProjectMenu && !isProjectOpen)
            {
                ImGui.SetNextWindowPos(center, ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
                ImGui.Begin("Create new Project", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.SetWindowSize(new System.Numerics.Vector2(500, 460));

                ImGui.Text("Create new Project");
                ImGui.Separator();
                ImGui.Dummy(new System.Numerics.Vector2(0, 6));

                ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 1f, 0.6f), "# Project Info:");
                ImGui.Dummy(new System.Numerics.Vector2(0, 6));

                ImGui.Text("Project Title");
                ImGui.InputTextWithHint(" ", "New Project", ref newProjectName, (uint)64, ImGuiInputTextFlags.AutoSelectAll);
                ImGui.SetItemTooltip("This will be the name of your Project, and will appear on built versions of it. You can always change this later.");

                ImGui.Dummy(new System.Numerics.Vector2(0, 4));

                ImGui.Text("Project Developer");
                ImGui.InputTextWithHint("## ", "ProDev0303", ref newProjectDeveloper, (uint)32, ImGuiInputTextFlags.AutoSelectAll);
                ImGui.SetItemTooltip("This won't have any big effect aside from labelling. You can always change this later.");


                string typed = newProjectName.Trim();

                string fullPath = Path.Combine(defaultProjectPath, typed);
                bool pathExists = Directory.Exists(fullPath);

                if(pathExists) ImGui.BeginDisabled();
                if(ImGui.Button("Create"))
                {
                    if(!string.IsNullOrEmpty(typed))
                    {
                        working = true;
                        projectMenu = false;

                        Console.WriteLine($"Creating project with name '{typed}'");
                        CreateProject(typed).Wait(); // create project...
                        LoadProject(Path.Combine(defaultProjectPath, typed)).Wait(); // then, load it

                        working = false;
                        Console.WriteLine($"Created and loaded project {typed}!");
                    }
                }
                if(pathExists) ImGui.EndDisabled();
                if(ImGui.Button("Back")) { createProjectMenu = false; projectMenu = true; }


                System.Numerics.Vector4 pathTextColor = new System.Numerics.Vector4(1f, 1f, 1f, 1f); // white
                if(!pathExists) pathTextColor = new System.Numerics.Vector4(0.2f, 1f, 0.2f, 1f); // green
                else pathTextColor = new System.Numerics.Vector4(1f, 0.2f, 0.2f, 1f); // red

                ImGui.TextColored(pathTextColor, fullPath);
                if(pathExists) ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 1f, 0.4f), "A project with that name already exists.");

                ImGui.End();
            }
            else if(isProjectOpen)
            {
                // Editor UI
            }
            ImGui.PopStyleColor(41);
        }


        /// <summary>
        /// Initializes core User and Engine data.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static void InitializeCore()
        {
            // Configure and Assign the Default Project Path.
            // Eventually support settings like a custom path.

            ImGui.CreateContext();


            string programData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dP = Path.Combine(programData, "Limeko\\Projects");
            if (!Directory.Exists(dP)) Directory.CreateDirectory(dP);
            defaultProjectPath = dP;

            RefreshProjectList();
        }

        private static void RefreshProjectList()
        {
            projects = Directory.GetDirectories(defaultProjectPath).ToList();
        }

        private static void BeginTheme()
        {
            // -- Backgrounds --
            ImGui.PushStyleColor(ImGuiCol.WindowBg, (System.Numerics.Vector4)new Vector4(0.047f, 0.063f, 0.047f, 0.97f)); // dark lime tint
            ImGui.PushStyleColor(ImGuiCol.ChildBg, (System.Numerics.Vector4)new Vector4(0.055f, 0.075f, 0.055f, 0.90f));
            ImGui.PushStyleColor(ImGuiCol.PopupBg, (System.Numerics.Vector4)new Vector4(0.059f, 0.082f, 0.059f, 0.98f));

            // -- Borders --
            ImGui.PushStyleColor(ImGuiCol.Border, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.25f)); // visible lime border
            ImGui.PushStyleColor(ImGuiCol.BorderShadow, (System.Numerics.Vector4)new Vector4(0.000f, 0.000f, 0.000f, 0.00f));

            // -- Frames --
            ImGui.PushStyleColor(ImGuiCol.FrameBg, (System.Numerics.Vector4)new Vector4(0.071f, 0.110f, 0.071f, 0.85f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, (System.Numerics.Vector4)new Vector4(0.100f, 0.160f, 0.100f, 0.85f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, (System.Numerics.Vector4)new Vector4(0.130f, 0.210f, 0.130f, 0.90f));

            // -- Title bars --
            ImGui.PushStyleColor(ImGuiCol.TitleBg, (System.Numerics.Vector4)new Vector4(0.039f, 0.055f, 0.039f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, (System.Numerics.Vector4)new Vector4(0.118f, 0.380f, 0.200f, 1.00f)); // punchy lime
            ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, (System.Numerics.Vector4)new Vector4(0.039f, 0.055f, 0.039f, 0.70f));

            // -- Scrollbar --
            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, (System.Numerics.Vector4)new Vector4(0.039f, 0.055f, 0.039f, 0.00f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.55f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.80f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 1.00f));

            // -- Checkmark & Slider --
            ImGui.PushStyleColor(ImGuiCol.CheckMark, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.SliderGrab, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 1.00f));

            // -- Buttons --
            ImGui.PushStyleColor(ImGuiCol.Button, (System.Numerics.Vector4)new Vector4(0.118f, 0.320f, 0.180f, 1.00f)); // proper lime green
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.75f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 0.90f));

            // -- Headers --
            ImGui.PushStyleColor(ImGuiCol.Header, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.25f));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.45f));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 0.55f));

            // -- Separator --
            ImGui.PushStyleColor(ImGuiCol.Separator, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.30f));
            ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.70f));
            ImGui.PushStyleColor(ImGuiCol.SeparatorActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 1.00f));

            // -- Resize Grip --
            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.25f));
            ImGui.PushStyleColor(ImGuiCol.ResizeGripHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.60f));
            ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 0.90f));

            // -- Tabs --
            ImGui.PushStyleColor(ImGuiCol.Tab, (System.Numerics.Vector4)new Vector4(0.059f, 0.110f, 0.071f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.TabHovered, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.50f));
            ImGui.PushStyleColor(ImGuiCol.TabSelected, (System.Numerics.Vector4)new Vector4(0.118f, 0.380f, 0.200f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.TabDimmed, (System.Numerics.Vector4)new Vector4(0.039f, 0.075f, 0.047f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.TabDimmedSelected, (System.Numerics.Vector4)new Vector4(0.071f, 0.200f, 0.110f, 1.00f));

            // -- Text --
            ImGui.PushStyleColor(ImGuiCol.Text, (System.Numerics.Vector4)new Vector4(0.878f, 1.000f, 0.925f, 1.00f)); // soft mint white
            ImGui.PushStyleColor(ImGuiCol.TextDisabled, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 0.40f)); // faded mint
            ImGui.PushStyleColor(ImGuiCol.TextSelectedBg, (System.Numerics.Vector4)new Vector4(0.204f, 0.757f, 0.451f, 0.35f));

            // -- Misc --
            ImGui.PushStyleColor(ImGuiCol.DragDropTarget, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 0.90f));
            ImGui.PushStyleColor(ImGuiCol.NavWindowingHighlight, (System.Numerics.Vector4)new Vector4(0.486f, 1.000f, 0.776f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.ModalWindowDimBg, (System.Numerics.Vector4)new Vector4(0.000f, 0.000f, 0.000f, 0.50f));
        }


        /// <summary>
        /// Initializes the core Editor logic.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static async Task InitializeEditor()
        {
            // Layouts aren't yet implemented.
            Console.WriteLine("Initializing Editor...");
            Console.WriteLine($"Layout: None");
            Console.WriteLine($"Level: None");
            Console.WriteLine($"Previous Session: Unknown");

            Console.WriteLine("Editor Unfinished. Consider it loaded!");
        }

        /// <summary>
        /// Loads an existing Project, given one is not open.
        /// Internal Method--Don't call directly!
        /// </summary>
        /// <param name="path"></param>
        public static async Task LoadProject(string path)
        {
            RefreshProjectList();
            if(!projects.Contains(path)) return;
            Console.WriteLine($"Loading {path.Split("\\").Last()}...");
            Stopwatch loadTime = new Stopwatch();
            loadTime.Start();

            // load
            Console.WriteLine("Parsing Assets...");
            int assetCount = 0;
            List<string> toLoad = GetAllDirectories(path).Result;
            toLoad.Add(path);
            foreach(var dir in toLoad)
            {
                foreach(var asset in Directory.GetFiles(dir))
                {
                    Console.WriteLine($"> {asset.Split("\\").Last()}");
                    assetCount++;
                }
            }
            Console.WriteLine("Completed Parse.");
            // eventually actually load assets and whatnot

            loadTime.Stop();
            activeProjectPath = path;
            isProjectOpen = true;

            Console.WriteLine("");
            InitializeEditor().Wait();

            Console.WriteLine($"\nCompleted!");
            Console.WriteLine($"Loaded {assetCount} assets from {toLoad.Count} directories.");
            Console.WriteLine($"Took {loadTime.Elapsed.Minutes} minutes and {(loadTime.Elapsed.Seconds)}.{loadTime.Elapsed.Milliseconds} seconds");
        }

        private static async Task<List<string>> GetAllDirectories(string path)
        {
            List<string> returnDirectories = new();
            foreach (var d in Directory.GetDirectories(path).ToList())
            {
                returnDirectories.Add(d);
                var a = GetAllDirectories(d).Result;
                if(a.Count > 0)
                {
                    foreach(var c in a)
                    {
                        returnDirectories.Add(c);
                    }
                }
            }
            return returnDirectories;
        }

        public static async Task CreateProject(string name)
        {
            string trimmed = name.Trim();
            string newProjectPath = Path.Combine(defaultProjectPath, trimmed);
            if(Directory.Exists(newProjectPath))
            {
                // has yet to be updated to use ImGUI.
                Console.WriteLine("A project with that name already exists. Load it?");
                Console.Write("[y/n]: ");
                if(Console.ReadLine().Trim().ToLower() == "y")
                {
                    await LoadProject(newProjectPath);
                    return;
                }
                return;
            }
            Console.WriteLine($"Creating parent directory for {trimmed}...");
            Directory.CreateDirectory(newProjectPath);


            Console.WriteLine("Creating sub-directories...");

            string gitIgnore = $".editor/\r\nbin/\r\nobj/\r\n.vs/\r\n.idea/\r\n*.user\r\n.DS_Store\r\nThumbs.db";
            File.WriteAllText(Path.Combine(newProjectPath, ".gitignore"), gitIgnore);

            DirectoryInfo eInf = Directory.CreateDirectory(Path.Combine(newProjectPath, ".editor"));
            eInf.Attributes = FileAttributes.Hidden;

            DirectoryInfo assetsInf = Directory.CreateDirectory(Path.Combine(newProjectPath, "Assets"));

            DirectoryInfo assLev = Directory.CreateDirectory(Path.Combine(assetsInf.FullName, "Levels"));
            Directory.CreateDirectory(Path.Combine(assetsInf.FullName, "Materials"));
            Directory.CreateDirectory(Path.Combine(assetsInf.FullName, "Models"));
            Directory.CreateDirectory(Path.Combine(assetsInf.FullName, "Scripts"));
            Directory.CreateDirectory(Path.Combine(assetsInf.FullName, "Settings"));


            Console.WriteLine("Creating default Assets...");

            File.Create(Path.Combine(assLev.FullName, "level0.level"));


            // [create subdirectories, default assets, etc.]
            // TODO: finish this or whatever

            Console.WriteLine($"Created project '{trimmed}' at {newProjectPath}");
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