using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;

namespace Graphics1
{
    public partial class Game : GameWindow
    {
        private ShaderManager shaderProgram;
        protected int textureMetal;
        protected int textureAlien;
        protected int textureDroneMetal;
        protected int textureGrass;
        protected int textureWindow;

        private Stopwatch stopwatch;
        private int[] viewport = new int[4];
        private List<int> textures = new List<int>();
        protected bool swapTex = false;

        // 3D Meshes & Models
        private Mesh cubeMesh;
        private Mesh pyramidMesh;
        private Mesh cylinderMesh;
        private Mesh sphereMesh;
        private Mesh torusMesh;

        private float armRot_offset = 0;
        private float sawRot_offset = 0;
        private Model_Loader DroneModel;
        private Model_Loader ArmModel;
        private Model_Loader SawModel;

        public Camera camera;
        public SkyBox skybox;
        private bool firstMove = false;
        public Vector2 lastMouse;
        public Grid worldGrid;
        public Gizmo orientationGizmo;

        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Vector2i windowSize = new Vector2i(1024, 768);
            this.CenterWindow(windowSize);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            this.Title = $"3D Primitives: Auto-Fitting Viewport ({e.Width}, {e.Height})";
            GL.Viewport(0, 0, e.Width, e.Height);
            viewport[2] = e.Width;
            viewport[3] = e.Height;

            camera?.UpdateViewport(e.Width, e.Height);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            worldGrid = new Grid(slices: 80, spacing: 1.0f, yLevel: -1.5f);
            orientationGizmo = new Gizmo();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.08f, 0.08f, 0.12f, 1.0f);

            // Shaders using ShaderManager
            string vsCode = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                out vec2 texCoord;

                void main()
                {
                    gl_Position = vec4(aPosition, 1.0f) * model * view * projection;
                    texCoord = aTexCoord;
                }
            ";

            string fsCode = @"
                #version 330 core
                out vec4 pixelColor;
                in vec2 texCoord;
                uniform sampler2D texture0;

                void main()
                {
                    pixelColor = texture(texture0, texCoord);
                }
            ";

            shaderProgram = new ShaderManager(vsCode, fsCode);

            GL.GetInteger(GetPName.Viewport, viewport);
            if (viewport[2] == 0 || viewport[3] == 0)
            {
                viewport[2] = ClientSize.X;
                viewport[3] = ClientSize.Y;
            }

            // Initialize all meshes
            cubeMesh = MeshBuilder.CreateCubeMesh();
            pyramidMesh = MeshBuilder.CreatePyramidMesh();
            cylinderMesh = MeshBuilder.CreateCylinderMesh();
            sphereMesh = MeshBuilder.CreateSphereMesh();
            torusMesh = MeshBuilder.CreateTorusMesh();

            // Load Custom Models
            DroneModel = Model_Loader.Load("Models/Drone1.glb");
            ArmModel = Model_Loader.Load("Models/Arm.glb");
            SawModel = Model_Loader.Load("Models/Saw.glb");

            // Setup Textures via TextureLoader
            textureMetal = TextureLoader.LoadTexture("metal1.jpg");
            textureAlien = TextureLoader.LoadTexture("alien1.jpg");
            textureDroneMetal = TextureLoader.LoadTexture("droneMetal.png");
            textureGrass = TextureLoader.LoadTexture("grass.png");
            textureWindow = TextureLoader.LoadTexture("window.png");
            swapTex = false;

            textures.Add(textureMetal);
            textures.Add(textureAlien);
            textures.Add(textureDroneMetal);
            textures.Add(textureGrass);
            textures.Add(textureWindow);

            obj_drone = new Object { texture = textureDroneMetal };
            obj_arm = new Object { texture = textureMetal };
            obj_saw = new Object { texture = textureMetal };
            obj_cube = new Object { texture = textureMetal };
            obj_pyramid = new Object { texture = textureMetal };

            objects.Add(obj_drone);
            objects.Add(obj_arm);
            objects.Add(obj_saw);
            objects.Add(obj_cube);
            objects.Add(obj_pyramid);

            camera = new Camera(Vector3.UnitZ * 3, viewport[2], viewport[3]);
            skybox = new SkyBox();
        }

        protected override void OnUnload()
        {
            stopwatch.Stop();

            cubeMesh?.Dispose();
            pyramidMesh?.Dispose();
            //cylinderMesh?.Dispose();
            //sphereMesh?.Dispose();
            //torusMesh?.Dispose();

            worldGrid?.Dispose();
            orientationGizmo?.Dispose();
            shaderProgram?.Dispose();

            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderProgram.Use();

            double t = stopwatch.Elapsed.TotalSeconds;

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int locModel = shaderProgram.GetUniformLocation("model");
            shaderProgram.SetMatrix4("view", ref view);
            shaderProgram.SetMatrix4("projection", ref projection);

            // 1. CUBE
            float cubeScaleVal = 0.9f + 0.18f * MathF.Sin((float)t * 0.8f);
            Matrix4 cubeModel = Matrix4.CreateScale(cubeScaleVal) *
                                Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)t * 25f)) *
                                Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)t * 35f)) *
                                Matrix4.CreateTranslation(new Vector3(-2.6f, 1.2f + 0.25f * MathF.Sin((float)t * 0.7f), 0.0f));
            obj_cube.DrawModel(cubeMesh, cubeModel, locModel);

            // 2. PYRAMID
            float stretchY = 1.0f + 0.22f * MathF.Sin((float)t * 0.9f);
            float squishXZ = 1.0f - 0.10f * MathF.Sin((float)t * 0.9f);
            Matrix4 pyramidModel = Matrix4.CreateScale(squishXZ, stretchY, squishXZ) *
                                  Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)t * 15f)) *
                                  Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)t * 45f)) *
                                  Matrix4.CreateTranslation(new Vector3(10.0f, 1.2f + 0.25f * MathF.Cos((float)t * 0.65f), 0.0f));
            obj_pyramid.DrawModel(pyramidMesh, pyramidModel, locModel);

            // DRONE HOVER
            Vector3 DronePosition = new Vector3(0.0f, 0.3f * MathF.Cos((float)t * 0.80f), -5.0f);
            Matrix4 droneModel = Matrix4.CreateScale(1, 1, 1) *
                                 Matrix4.CreateRotationX(MathHelper.DegreesToRadians(120)) *
                                 Matrix4.CreateRotationY(MathHelper.DegreesToRadians(120 * MathF.Cos((float)t * 0.20f))) *
                                 Matrix4.CreateTranslation(DronePosition);

            obj_drone.DrawModel_Custom(DroneModel, droneModel, locModel);

            // DRONE Arm
            Matrix4 armModel = Matrix4.CreateScale(0.5f, 0.5f, 0.5f) *
                               Matrix4.CreateRotationX(MathHelper.DegreesToRadians(armRot_offset)) *
                               Matrix4.CreateRotationY(MathHelper.DegreesToRadians(1)) *
                               Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(1)) *
                               Matrix4.CreateTranslation(new Vector3(0.07f, 2.0f, 0.0f)) *
                               droneModel;

            obj_arm.DrawModel_Custom(ArmModel, armModel, locModel);

            // Saw Blade
            Matrix4 sawModel = Matrix4.CreateScale(1.7f, 1.7f, 1.7f) *
                               Matrix4.CreateRotationX(MathHelper.DegreesToRadians(1)) *
                               Matrix4.CreateRotationY(MathHelper.DegreesToRadians(1)) *
                               Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(sawRot_offset)) *
                               Matrix4.CreateTranslation(new Vector3(0.07f, 2.0f, 0.0f)) *
                               armModel;

            obj_saw.DrawModel_Custom(SawModel, sawModel, locModel);

            worldGrid.Draw(view, projection, camera.Position);
            skybox.Draw(view, projection);
            orientationGizmo.Draw(view, viewport[2], viewport[3], gizmoSize: 240);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (IsFocused == false)
                return;

            KeyboardState input = KeyboardState;
            if (input.IsKeyReleased(Keys.Space))
            {
                swapTex = !swapTex;
            }

            //-----Camera-----

            float speed = 5;//0.001f;
            if (input.IsKeyDown(Keys.W)) camera.Position += camera.Front * speed * (float)args.Time;
            if (input.IsKeyDown(Keys.S)) camera.Position -= camera.Front * speed * (float)args.Time;
            if (input.IsKeyDown(Keys.A)) camera.Position -= camera.Right * speed * (float)args.Time;
            if (input.IsKeyDown(Keys.D)) camera.Position += camera.Right * speed * (float)args.Time;
            if (input.IsKeyDown(Keys.E)) camera.Position += camera.Up * speed * (float)args.Time;
            if (input.IsKeyDown(Keys.Q)) camera.Position -= camera.Up * speed * (float)args.Time;
            
            if (input.IsKeyPressed(Keys.LeftControl))
            {
                camera.CursorControl = !camera.CursorControl;
                CursorState = camera.CursorControl ? CursorState.Grabbed : CursorState.Normal;
                if (camera.CursorControl) firstMove = true;
            }

            if (camera.CursorControl)
            {
                MouseState mouse = this.MouseState;
                if (firstMove)
                {
                    lastMouse = new Vector2(mouse.X, mouse.Y);
                    firstMove = false;
                }
                else
                {
                    float deltaX = mouse.X - lastMouse.X;
                    float deltaY = mouse.Y - lastMouse.Y;
                    lastMouse = new Vector2(mouse.X, mouse.Y);

                    camera.Yaw += deltaX * camera.Sensitivity ;
                    camera.Pitch -= deltaY * camera.Sensitivity;
                }
            }

            if (input.IsKeyDown(Keys.Left)) armRot_offset -= 0.5f;
            if (input.IsKeyDown(Keys.Right)) armRot_offset += 0.5f;
            if (input.IsKeyDown(Keys.C)) sawRot_offset += 1.5f;

            if (input.IsKeyPressed(Keys.T))
            {
                if (textureIndex < textures.Count - 1) textureIndex++;
                else textureIndex = 0;
            }
            if (input.IsKeyPressed(Keys.KeyPad0)) objects[0].texture = textures[textureIndex];
            if (input.IsKeyPressed(Keys.KeyPad1)) objects[1].texture = textures[textureIndex];
            if (input.IsKeyPressed(Keys.KeyPad2)) objects[2].texture = textures[textureIndex];
            if (input.IsKeyPressed(Keys.KeyPad3)) objects[3].texture = textures[textureIndex];
            if (input.IsKeyPressed(Keys.KeyPad4)) objects[4].texture = textures[textureIndex];

            if (input.IsKeyPressed(Keys.Escape)) Close();
        }
    }
}