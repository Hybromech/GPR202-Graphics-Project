using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using System.IO.Enumeration;
using System.IO;

namespace Graphics1
{
    // Helper class to encapsulate OpenGL vertex buffers for each 3D shape


    public class Game : GameWindow
    {
        private int shaderProgramObject;
        protected int textureMetal;
        protected int textureAlien;
        private Stopwatch stopwatch;
        private int[] viewport = new int[4];
        protected bool swapTex = false;

        // 3D Meshes
        private Mesh cubeMesh;
        private Mesh pyramidMesh;
        private Mesh cylinderMesh;
        private Mesh sphereMesh;
        private Mesh torusMesh;
        public Camera camera;
        public SkyBox skybox;
        private bool firstMove = false;
        public Vector2 lastMouse; // put in input manager

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
        }

        #region Procedural Geometry Generators

        private void HandleTextures()
        {
            // Texture Coordinates
            float[] texCoords = {
            0.0f, 0.0f, // lower-left corner
            1.0f, 0.0f, // lower-right corner
            0.5f, 1.0f  // top-center corner
        };
            // Set wrap mode
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // Set border color
            float[] borderColor = { 1.0f, 1.0f, 0.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            // Set filter modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            // Set Mip Maps
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private Mesh CreateCubeMesh()
        {
            float[] vertices = new float[]
            {
                // Front (Red)
                -0.5f, -0.5f,  0.5f,   1.0f, 1.0f,
                 0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,   0.0f, 1.0f,
                // Back (Green)
                -0.5f, -0.5f, -0.5f,   1.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,   0.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
                // Top (Blue)
                -0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,   1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,   0.0f, 1.0f,
                // Bottom (Yellow)
                -0.5f, -0.5f, -0.5f,   1.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,   0.0f, 1.0f,
                // Right (Cyan)
                 0.5f, -0.5f, -0.5f,   1.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,   1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,   0.0f, 1.0f,
                // Left (Magenta)
                -0.5f, -0.5f, -0.5f,   1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,   1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,   0.0f, 1.0f,
            };

            uint[] indices = new uint[]
            {
                 0,  1,  2,   2,  3,  0,
                 4,  5,  6,   6,  7,  4,
                 8,  9, 10,  10, 11,  8,
                12, 13, 14,  14, 15, 12,
                16, 17, 18,  18, 19, 16,
                20, 21, 22,  22, 23, 20
            };

            return new Mesh(vertices, indices);
        }

        private Mesh CreatePyramidMesh()
        {
            float[] vertices = new float[]
            {
                // Front face (Orange)
                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                -0.5f, -0.4f,  0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f,  0.5f,   0.0f, 1.0f,

                // Right face (Pink)
                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                 0.5f, -0.4f,  0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f, -0.5f,   0.0f, 1.0f,

                // Back face (Teal)
                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                 0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                -0.5f, -0.4f, -0.5f,   0.0f, 1.0f,

                // Left face (Lime)
                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                -0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                -0.5f, -0.4f,  0.5f,   0.0f, 1.0f,

                // Base (Dark Violet)
                -0.5f, -0.4f, -0.5f,   1.0f, 1.0f,
                 0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f,  0.5f,   0.0f, 1.0f,
                -0.5f, -0.4f,  0.5f,   0.0f, -1.0f,
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,        // Front
                3, 4, 5,        // Right
                6, 7, 8,        // Back
                9, 10, 11,      // Left
                12, 13, 14, 14, 15, 12 // Base
            };

            return new Mesh(vertices, indices);
        }

        private Mesh CreateCylinderMesh(int segments = 28, float radius = 0.18f, float length = 1.6f)
        {
            List<float> verts = new List<float>();
            List<uint> inds = new List<uint>();
            float halfLen = length / 2.0f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / segments);
                float x = MathF.Cos(angle) * radius;
                float y = MathF.Sin(angle) * radius;

                float r = 1.0f;
                float g = 0.6f + 0.3f * MathF.Cos(angle);
                float b = 0.1f + 0.1f * MathF.Sin(angle);

                verts.AddRange(new[] { x, y, halfLen, r, g, b, 1.0f });
                verts.AddRange(new[] { x, y, -halfLen, r * 0.7f, g * 0.7f, b * 0.7f, 1.0f });
            }

            for (int i = 0; i < segments; i++)
            {
                uint front1 = (uint)(i * 2);
                uint back1 = front1 + 1;
                uint front2 = (uint)((i + 1) * 2);
                uint back2 = front2 + 1;

                inds.AddRange(new[] { front1, back1, front2 });
                inds.AddRange(new[] { front2, back1, back2 });
            }

            uint frontCenter = (uint)(verts.Count / 7);
            verts.AddRange(new[] { 0.0f, 0.0f, halfLen, 1.0f, 0.85f, 0.3f, 1.0f });

            uint backCenter = frontCenter + 1;
            verts.AddRange(new[] { 0.0f, 0.0f, -halfLen, 0.6f, 0.4f, 0.1f, 1.0f });

            for (int i = 0; i < segments; i++)
            {
                uint front1 = (uint)(i * 2);
                uint front2 = (uint)((i + 1) * 2);
                inds.AddRange(new[] { frontCenter, front1, front2 });

                uint back1 = front1 + 1;
                uint back2 = front2 + 1;
                inds.AddRange(new[] { backCenter, back2, back1 });
            }

            return new Mesh(verts.ToArray(), inds.ToArray());
        }

        private Mesh CreateSphereMesh(int latitudeBands = 20, int longitudeBands = 20, float radius = 0.5f)
        {
            List<float> verts = new List<float>();
            List<uint> inds = new List<uint>();

            for (int lat = 0; lat <= latitudeBands; lat++)
            {
                float theta = (float)(lat * Math.PI / latitudeBands);
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int lon = 0; lon <= longitudeBands; lon++)
                {
                    float phi = (float)(lon * 2.0 * Math.PI / longitudeBands);
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    float r = 0.3f + 0.7f * (0.5f + 0.5f * x);
                    float g = 0.9f;
                    float b = 1.0f;

                    verts.AddRange(new[] { x * radius, y * radius, z * radius, r, g, b, 1.0f });
                }
            }

            for (int lat = 0; lat < latitudeBands; lat++)
            {
                for (int lon = 0; lon < longitudeBands; lon++)
                {
                    uint first = (uint)(lat * (longitudeBands + 1) + lon);
                    uint second = (uint)(first + longitudeBands + 1);

                    inds.AddRange(new[] { first, second, first + 1 });
                    inds.AddRange(new[] { second, second + 1, first + 1 });
                }
            }

            return new Mesh(verts.ToArray(), inds.ToArray());
        }

        private Mesh CreateTorusMesh(int radialSegments = 32, int tubularSegments = 20, float mainRadius = 0.65f, float tubeRadius = 0.10f)
        {
            List<float> verts = new List<float>();
            List<uint> inds = new List<uint>();

            for (int i = 0; i <= radialSegments; i++)
            {
                float u = (float)(i * 2.0 * Math.PI / radialSegments);

                for (int j = 0; j <= tubularSegments; j++)
                {
                    float v = (float)(j * 2.0 * Math.PI / tubularSegments);

                    float x = (mainRadius + tubeRadius * MathF.Cos(v)) * MathF.Cos(u);
                    float y = (mainRadius + tubeRadius * MathF.Cos(v)) * MathF.Sin(u);
                    float z = tubeRadius * MathF.Sin(v);

                    float r = 0.2f + 0.4f * MathF.Cos(u * 2.0f);
                    float g = 0.6f + 0.4f * MathF.Sin(v * 2.0f);
                    float b = 0.95f;

                    verts.AddRange(new[] { x, y, z, r, g, b, 1.0f });
                }
            }

            for (int i = 0; i < radialSegments; i++)
            {
                for (int j = 0; j < tubularSegments; j++)
                {
                    uint a = (uint)(i * (tubularSegments + 1) + j);
                    uint b = (uint)((i + 1) * (tubularSegments + 1) + j);

                    inds.AddRange(new[] { a, b, a + 1 });
                    inds.AddRange(new[] { b, b + 1, a + 1 });
                }
            }

            return new Mesh(verts.ToArray(), inds.ToArray());
        }

        #endregion
        protected override void OnLoad()
        {
            base.OnLoad();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.08f, 0.08f, 0.12f, 1.0f);

            // Shaders
            string vertexShaderCode = @"
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

            string fragmentShaderCode = @"
                #version 330 core
                
                out vec4 pixelColor;

                in vec2 texCoord;

                uniform sampler2D texture0;

                void main()
                {
                    pixelColor = texture(texture0, texCoord);
                }
            ";

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShaderCode);
            GL.CompileShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShaderCode);
            GL.CompileShader(fs);

            shaderProgramObject = GL.CreateProgram();
            GL.AttachShader(shaderProgramObject, vs);
            GL.AttachShader(shaderProgramObject, fs);
            GL.LinkProgram(shaderProgramObject);

            GL.DetachShader(shaderProgramObject, vs);
            GL.DetachShader(shaderProgramObject, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            // Populate initial viewport dimensions from window size
            GL.GetInteger(GetPName.Viewport, viewport);
            if (viewport[2] == 0 || viewport[3] == 0)
            {
                viewport[2] = ClientSize.X;
                viewport[3] = ClientSize.Y;
            }

            // Initialize all meshes
            cubeMesh = CreateCubeMesh();
            pyramidMesh = CreatePyramidMesh();
            cylinderMesh = CreateCylinderMesh();
            sphereMesh = CreateSphereMesh();
            torusMesh = CreateTorusMesh();

            // Setup Textures
            StbImage.stbi_set_flip_vertically_on_load(1);
            textureMetal = Texture.LoadTexture("metal1.jpg");
            textureAlien = Texture.LoadTexture("alien1.jpg");
            swapTex = false;
            Texture.UseTexture(textureMetal);

            // Setup camera
            camera = new Camera(Vector3.UnitZ * 3, viewport[2], viewport[3]);

            // Setup skybox

            skybox = new SkyBox();
        }
        protected override void OnUnload()
        {
            stopwatch.Stop();

            cubeMesh?.Dispose();
            pyramidMesh?.Dispose();
            cylinderMesh?.Dispose();
            sphereMesh?.Dispose();
            torusMesh?.Dispose();

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramObject);

            base.OnUnload();
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgramObject);

            double t = stopwatch.Elapsed.TotalSeconds;

            // ---$# Main Camera Code #$---
            // -------------------------------------------------------------------------
            // RESPONSIVE VIEWPORT & CAMERA ZOOM (PREVENTS OBJECTS FROM BEING CLIPPED)
            // -------------------------------------------------------------------------
            float width = Math.Max(1.0f, viewport[2]);
            float height = Math.Max(1.0f, viewport[3]);
            float currentAspect = width / height;

            // Target aspect ratio (4:3) where all objects fit comfortably
            const float referenceAspect = 1024.0f / 768.0f;
            const float baseDistance = 7.0f;

            // If the window gets narrower than the reference aspect ratio, 
            // dynamically pull the camera back so the scene scales down to fit horizontally.
            float cameraDistance = baseDistance;
            if (currentAspect < referenceAspect)
            {
                cameraDistance = baseDistance * (referenceAspect / currentAspect);
            }

            Matrix4 view_asp = Matrix4.CreateTranslation(0.0f, 0.0f, -cameraDistance);
            Matrix4 proj_asp = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), currentAspect, 0.1f, 100.0f);

            // Main Camera config
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int locModel = GL.GetUniformLocation(shaderProgramObject, "model");
            int locView = GL.GetUniformLocation(shaderProgramObject, "view");
            int locProj = GL.GetUniformLocation(shaderProgramObject, "projection");

            GL.UniformMatrix4(locView, true, ref view);
            GL.UniformMatrix4(locProj, true, ref projection);

            void DrawModel(Mesh mesh, Matrix4 modelMatrix)
            {
                GL.UniformMatrix4(locModel, true, ref modelMatrix);
                mesh.Draw();
            }

            // -------------------------------------------------------------------------
            // 1. CUBE: Gentle breath + Bobbing
            // -------------------------------------------------------------------------
            float cubeScaleVal = 0.9f + 0.18f * MathF.Sin((float)t * 0.8f);
            Matrix4 cubeModel = Matrix4.CreateScale(cubeScaleVal) *
                                Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)t * 25f)) *
                                Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)t * 35f)) *
                                Matrix4.CreateTranslation(new Vector3(-2.6f, 1.2f + 0.25f * MathF.Sin((float)t * 0.7f), 0.0f));
            DrawModel(cubeMesh, cubeModel);

            // -------------------------------------------------------------------------
            // 2. PYRAMID: Accordion stretch + Bobbing
            // -------------------------------------------------------------------------
            float stretchY = 1.0f + 0.22f * MathF.Sin((float)t * 0.9f);
            float squishXZ = 1.0f - 0.10f * MathF.Sin((float)t * 0.9f);
            Matrix4 pyramidModel = Matrix4.CreateScale(squishXZ, stretchY, squishXZ) *
                                  Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)t * 15f)) *
                                  Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)t * 45f)) *
                                  Matrix4.CreateTranslation(new Vector3(0.0f, 1.2f + 0.25f * MathF.Cos((float)t * 0.65f), 0.0f));
            DrawModel(pyramidMesh, pyramidModel);

            // =========================================================================
            // 3, 4 & 5. NUCLEUS SYSTEM: DOUGHNUT, CYLINDER & ORBITING SPHERE (ELECTRON)
            // =========================================================================
            Vector3 centerAnchor = new Vector3(1.4f, -1.1f, 0.0f);
            float sharedBobbingY = 0.25f * MathF.Sin((float)t * 0.7f);
            Vector3 interactionPosition = centerAnchor + new Vector3(0.0f, sharedBobbingY, 0.0f);

            // Base tilt for the nucleus so its opening is clearly visible
            Matrix4 baseTilt = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(30.0f)) *
                               Matrix4.CreateRotationY(MathHelper.DegreesToRadians(15.0f));

            // Cycle: 6.0 seconds total
            // - Phase 1 (0.0s to 3.0s): Torus does full 360-degree Y revolution
            // - Phase 2 (3.0s to 6.0s): Torus faces forward while cylinder threads through
            float cycleDuration = 6.0f;
            float cyclePhase = (float)(t % cycleDuration);

            float torusRotationDeg;
            float cylinderZ;

            if (cyclePhase < 3.0f)
            {
                float p = cyclePhase / 3.0f;
                float smoothP = 0.5f * (1.0f - MathF.Cos(p * MathF.PI)); // 0.0 -> 1.0
                torusRotationDeg = smoothP * 360.0f;
                cylinderZ = -2.3f;
            }
            else
            {
                torusRotationDeg = 0.0f;
                float p = (cyclePhase - 3.0f) / 3.0f;
                float threadMotion = MathF.Sin(p * MathF.PI);
                cylinderZ = -2.3f + (4.6f * threadMotion);
            }

            // Torus Model (Nucleus)
            Matrix4 torusModel = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(torusRotationDeg)) *
                                 baseTilt *
                                 Matrix4.CreateTranslation(interactionPosition);
            //DrawModel(torusMesh, torusModel);

            // Cylinder Model (Threads down Z)
            float cylinderSelfSpin = (float)(t * 90.0f);
            Matrix4 cylinderModel = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(cylinderSelfSpin)) *
                                    Matrix4.CreateTranslation(0.0f, 0.0f, cylinderZ) *
                                    baseTilt *
                                    Matrix4.CreateTranslation(interactionPosition);
            //DrawModel(cylinderMesh, cylinderModel);

            // Sphere Model (Electron Orbit)
            float electronScale = 0.22f;
            float orbitSpeed = 160.0f;
            float orbitAngleDeg = (float)(t * orbitSpeed);
            float orbitRadius = 1.35f;

            Matrix4 sphereModel = Matrix4.CreateScale(electronScale) *
                                  Matrix4.CreateTranslation(orbitRadius, 0.0f, 0.0f) *
                                  Matrix4.CreateRotationY(MathHelper.DegreesToRadians(orbitAngleDeg)) *
                                  Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(45.0f)) *
                                  Matrix4.CreateRotationX(MathHelper.DegreesToRadians(25.0f)) *
                                  Matrix4.CreateTranslation(interactionPosition);
            //DrawModel(sphereMesh, sphereModel);

            // Render our Skybox
            skybox.Draw(view, projection);

            // Send Buffer to display
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (IsFocused == false)
                return;

            KeyboardState input = KeyboardState;
            if (input.IsKeyReleased(Keys.Space)) // Swap Textures on keypress
            {
                swapTex = !swapTex;
                Texture.UseTexture((!swapTex == false) ? textureAlien : textureMetal);
            }
            // Toggle camera looking
            if (input.IsKeyPressed(Keys.LeftControl))
            {
                camera.CursorControl = !camera.CursorControl;

                if (camera.CursorControl)
                {
                    // Hide and lock cursor for infinite mouse-look
                    CursorState = CursorState.Grabbed;
                    firstMove = true;
                }
                else
                {
                    // Release cursor back to normal desktop use
                    CursorState = CursorState.Normal;
                }
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

                    camera.Yaw += deltaX * camera.Sensitivity;
                    camera.Pitch -= deltaY * camera.Sensitivity;
                }
            }

            // Close program when escape is pressed
            {
                if (input.IsKeyPressed(Keys.Escape))
                    Close();
            }
        }
    }
}
