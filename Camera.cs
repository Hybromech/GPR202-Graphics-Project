using System;
using OpenTK.Mathematics;

namespace Graphics1
{
    public class Camera
    {
        public bool CursorControl = false;
        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;

        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;

        public Vector3 Position;

        // Base distance used for responsive framing
        public float BaseDistance = 7.0f;
        public const float ReferenceAspect = 1024.0f / 768.0f;

        private float pitch;
        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = MathHelper.Clamp(value, -89.0f, 89.0f);
                ApplyChanges();
            }
        }

        private float yaw = -90.0f;
        public float Yaw
        {
            get => yaw;
            set
            {
                yaw = value;
                ApplyChanges();
            }
        }

        public float Speed = 45f;
        public float Sensitivity = 0.12f;
        public float FOV = 45.0f; // Updated to match your 45-degree reference FOV
        public float AspectRatio { get; set; }

        public Camera(Vector3 pos, int width, int height)
        {
            Position = pos;
            UpdateViewport(width, height);
            ApplyChanges();
        }

        public void UpdateViewport(int width, int height)
        {
            float w = Math.Max(1.0f, width);
            float h = Math.Max(1.0f, height);
            AspectRatio = w / h;
        }

        public Matrix4 GetViewMatrix()
        {
            // Calculate dynamic pullback distance when aspect ratio drops below 4:3
            float distance = BaseDistance;
            if (AspectRatio < ReferenceAspect)
            {
                distance = BaseDistance * (ReferenceAspect / AspectRatio);
            }

            // Offset position backwards along the viewing direction so objects fit horizontally
            Vector3 responsivePosition = Position - (Front * (distance - BaseDistance));

            return Matrix4.LookAt(responsivePosition, responsivePosition + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(FOV),
                AspectRatio,
                0.1f,
                300.0f
            );
        }

        private void ApplyChanges()
        {
            float radians_pitch = MathHelper.DegreesToRadians(Pitch);
            float radians_yaw = MathHelper.DegreesToRadians(Yaw);

            front.X = MathF.Cos(radians_pitch) * MathF.Cos(radians_yaw);
            front.Y = MathF.Sin(radians_pitch);
            front.Z = MathF.Cos(radians_pitch) * MathF.Sin(radians_yaw);
            front = Vector3.Normalize(front);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
    }
}