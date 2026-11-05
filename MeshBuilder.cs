using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics1
{
    internal class MeshBuilder
    {
        public static Mesh CreateCubeMesh()
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

        public static Mesh CreatePyramidMesh()
        {
            float[] vertices = new float[]
            {
                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                -0.5f, -0.4f,  0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f,  0.5f,   0.0f, 1.0f,

                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                 0.5f, -0.4f,  0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f, -0.5f,   0.0f, 1.0f,

                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                 0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                -0.5f, -0.4f, -0.5f,   0.0f, 1.0f,

                 0.0f,  0.6f,  0.0f,   1.0f, 1.0f,
                -0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                -0.5f, -0.4f,  0.5f,   0.0f, 1.0f,

                -0.5f, -0.4f, -0.5f,   1.0f, 1.0f,
                 0.5f, -0.4f, -0.5f,   1.0f, 0.0f,
                 0.5f, -0.4f,  0.5f,   0.0f, 1.0f,
                -0.5f, -0.4f,  0.5f,   0.0f, -1.0f,
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
                9, 10, 11,
                12, 13, 14, 14, 15, 12
            };

            return new Mesh(vertices, indices);
        }

        public static Mesh CreateCylinderMesh(int segments = 28, float radius = 0.18f, float length = 1.6f)
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

        public static Mesh CreateSphereMesh(int latitudeBands = 20, int longitudeBands = 20, float radius = 0.5f)
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

        public static Mesh CreateTorusMesh(int radialSegments = 32, int tubularSegments = 20, float mainRadius = 0.65f, float tubeRadius = 0.10f)
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
    }
}
