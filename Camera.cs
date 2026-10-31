using System.Collections.Generic;
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
        public Vector3 Direction => (Position + Front);

        private float pitch;
        public float Pitch 
        {
            get => pitch;
            set
            {
                if (value > 89.0f)
                {
                    pitch = 89.0f; // Clamp the Pitch
                }
                else if (value <= -89.0f)
                {
                    pitch = -89.0f;
                }
                else
                {
                    pitch = value;
                }
                ApplyChanges();
            }
        }
        private float yaw;
        public float Yaw
        {
            get => yaw;
            set
            {
                yaw = value;
                ApplyChanges();
            }
        }
        // read in from configuration file
        public float Speed = 60; //45
        public float Sensitivity = 0.05f;
        public float FOV = 65f;

        public float AspectRatio { get; set; }

        public Camera(Vector3 pos, int width, int height)
        {
            Position = pos;
            AspectRatio = (float)width / height;
        }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Direction, Up);
        public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView((float)MathHelper.DegreesToRadians(FOV), AspectRatio, 0.1f, 300); // setup view frustum

        private void ApplyChanges()
        {
            float radians_pitch = MathHelper.DegreesToRadians(Pitch);
            float radians_yaw = MathHelper.DegreesToRadians(Yaw);
            front.X = (float)Math.Cos(radians_pitch) * (float)Math.Cos(radians_yaw);
            front.Y = (float)Math.Sin(radians_pitch);
            front.Z = (float)Math.Cos(radians_pitch) * (float)Math.Sin(radians_yaw);
            front = Vector3.Normalize(front);
        }
        
    }
}
