using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Graphics1
{
    public class Mesh
    {
        public int Vao { get; private set; }
        public int Vbo { get; private set; }
        public int Ebo { get; private set; }
        public int IndexCount { get; private set; }

        public Mesh(float[] vertices, uint[] indices)
        {
            IndexCount = indices.Length;
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            // Attribute 0: vec3 aPosition (3 floats)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Attribute 1: vec4 aColor (4 floats)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Ebo);
            GL.DeleteBuffer(Vbo);
            GL.DeleteVertexArray(Vao);
        }
    }
}
