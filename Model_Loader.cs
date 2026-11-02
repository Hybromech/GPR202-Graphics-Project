using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Schema2;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

public class Model_Loader : IDisposable
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public const int SizeInBytes = (3 + 3 + 2) * sizeof(float);
    }

    public class MeshPart : IDisposable
    {
        public int Vao { get; private set; }
        public int Vbo { get; private set; }
        public int Ebo { get; private set; }
        public int IndexCount { get; private set; }
        public Matrix4x4 LocalTransform { get; set; }

        public MeshPart(Vertex[] vertices, uint[] indices, Matrix4x4 transform)
        {
            IndexCount = indices.Length;
            LocalTransform = transform;

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Attribute 0: Position (vec3)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            // Attribute 1: TexCoords (vec2) - Mapped to location 1 to match Game.cs shader
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 6 * sizeof(float));

            // Attribute 2: Normal (vec3) - Mapped to location 2
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(Vao);
            GL.DeleteBuffer(Vbo);
            GL.DeleteBuffer(Ebo);
        }
    }

    public List<MeshPart> Parts { get; } = new List<MeshPart>();

    public static Model_Loader Load(string filePath)
    {
        var loader = new Model_Loader();
        var model = ModelRoot.Load(filePath);

        var scene = model.DefaultScene ?? model.LogicalScenes[0];

        foreach (var node in scene.VisualChildren)
        {
            loader.ProcessNode(node, Matrix4x4.Identity);
        }

        return loader;
    }

    private void ProcessNode(Node node, Matrix4x4 parentTransform)
    {
        Matrix4x4 localTransform = node.LocalMatrix * parentTransform;

        if (node.Mesh != null)
        {
            foreach (var primitive in node.Mesh.Primitives)
            {
                var vertices = new List<Vertex>();
                var indices = new List<uint>();

                var posAccessor = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                var normAccessor = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
                var uvAccessor = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();

                if (posAccessor == null)
                    continue;

                for (int i = 0; i < posAccessor.Count; i++)
                {
                    Vector3 pos = posAccessor[i];
                    Vector3 norm = normAccessor != null && i < normAccessor.Count ? normAccessor[i] : Vector3.UnitY;

                    // Invert V coordinate for OpenGL convention (glTF origin is top-left)
                    Vector2 uv = Vector2.Zero;
                    if (uvAccessor != null && i < uvAccessor.Count)
                    {
                        var rawUv = uvAccessor[i];
                        uv = new Vector2(rawUv.X, 1.0f - rawUv.Y);
                    }

                    vertices.Add(new Vertex(pos, norm, uv));
                }

                var indexAccessor = primitive.GetIndices();
                if (indexAccessor != null)
                {
                    foreach (var idx in indexAccessor)
                    {
                        indices.Add(idx);
                    }
                }
                else
                {
                    for (uint i = 0; i < vertices.Count; i++)
                    {
                        indices.Add(i);
                    }
                }

                if (vertices.Count > 0 && indices.Count > 0)
                {
                    Parts.Add(new MeshPart(vertices.ToArray(), indices.ToArray(), localTransform));
                }
            }
        }

        foreach (var child in node.VisualChildren)
        {
            ProcessNode(child, localTransform);
        }
    }

    public void Draw()
    {
        foreach (var part in Parts)
        {
            part.Render();
        }
    }

    public void Dispose()
    {
        foreach (var part in Parts)
        {
            part.Dispose();
        }
        Parts.Clear();
    }
}