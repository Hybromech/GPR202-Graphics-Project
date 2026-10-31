using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Graphics1
{
    public class Grid : IDisposable
    {
        private int vao;
        private int vbo;
        private int vertexCount;
        private int shaderProgram;

        public Grid(int slices = 100, float spacing = 1.0f, float yLevel = -0.01f)
        {
            List<float> vertices = new List<float>();
            float extent = (slices / 2.0f) * spacing;

            for (int i = -slices / 2; i <= slices / 2; i++)
            {
                float pos = i * spacing;

                // Major axes (center) colored differently from minor grid lines
                float r = (i == 0) ? 0.6f : 0.25f;
                float g = (i == 0) ? 0.6f : 0.25f;
                float b = (i == 0) ? 0.6f : 0.25f;

                // Line parallel to Z axis (along X)
                vertices.AddRange(new[] { pos, yLevel, -extent, r, g, b, 1.0f });
                vertices.AddRange(new[] { pos, yLevel, extent, r, g, b, 1.0f });

                // Line parallel to X axis (along Z)
                vertices.AddRange(new[] { -extent, yLevel, pos, r, g, b, 1.0f });
                vertices.AddRange(new[] { extent, yLevel, pos, r, g, b, 1.0f });
            }

            vertexCount = vertices.Count / 7;

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

            // Attribute 0: Position (3 floats)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Attribute 1: Color (4 floats)
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
                out vec3 worldPos;

                void main()
                {
                    worldPos = aPosition;
                    color = aColor;
                    gl_Position = vec4(aPosition, 1.0) * view * projection;
                }
            ";

            string fs = @"
                #version 330 core
                in vec4 color;
                in vec3 worldPos;
                out vec4 pixelColor;

                uniform vec3 cameraPos;

                void main()
                {
                    // Smoothly fade out the grid toward the horizon
                    float dist = length(worldPos.xz - cameraPos.xz);
                    float fade = clamp(1.0 - (dist / 80.0), 0.0, 1.0);
                    pixelColor = vec4(color.rgb, color.a * fade);
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

        public void Draw(Matrix4 view, Matrix4 proj, Vector3 cameraPos)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), true, ref proj);
            GL.Uniform3(GL.GetUniformLocation(shaderProgram, "cameraPos"), cameraPos);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, vertexCount);
            GL.BindVertexArray(0);

            GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }
    }
}