using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Graphics1
{
    public class ShaderManager : IDisposable
    {
        public int Handle { get; private set; }

        public ShaderManager(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            // Clean up shaders after linking
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            string infoLog = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(infoLog))
            {
                Console.WriteLine($"Error compiling shader ({type}): {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }

        public void SetMatrix4(string name, ref Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(GetUniformLocation(name), true, ref data);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}