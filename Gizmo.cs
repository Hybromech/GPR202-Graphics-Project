using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Graphics1
{
    public class Gizmo : IDisposable
    {
        private int vao;
        private int vbo;
        private int shaderProgram;

        public Gizmo()
        {
            // X (Red), Y (Green), Z (Blue) line segments
            float[] axes = new float[]
            {
                // Position (X,Y,Z)   // Color (R,G,B,A)
                0.0f, 0.0f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f,     0.0f, 0.4f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f,     0.0f, 0.4f, 1.0f, 1.0f,
            };

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, axes.Length * sizeof(float), axes, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            string vs = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec4 aColor;

                uniform mat4 view;
                uniform mat4 projection;

                out vec4 color;

                void main()
                {
                    color = aColor;
                    gl_Position = vec4(aPosition, 1.0) * view * projection;
                }
            ";

            string fs = @"
                #version 330 core
                in vec4 color;
                out vec4 pixelColor;

                void main()
                {
                    pixelColor = color;
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vs);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fs);
            GL.CompileShader(fragmentShader);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Draw(Matrix4 cameraView, int screenWidth, int screenHeight, int gizmoSize = 120)
        {
            // Position the mini-viewport in the top-right corner with 15px margin
            int margin = 15;
            int x = screenWidth - gizmoSize - margin;
            int y = screenHeight - gizmoSize - margin;

            GL.Viewport(x, y, gizmoSize, gizmoSize);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Strip translation so axes rotate in place
            Matrix4 rotView = new Matrix4(new Matrix3(cameraView));

            // Pull back slightly to view the origin comfortably
            Matrix4 view = rotView * Matrix4.CreateTranslation(0.0f, 0.0f, -3.2f);
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50.0f), 1.0f, 0.1f, 10.0f);

            GL.LineWidth(3.0f);

            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), true, ref proj);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 6);
            GL.BindVertexArray(0);

            GL.LineWidth(1.0f);

            // Restore the main scene viewport
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }
    }
}