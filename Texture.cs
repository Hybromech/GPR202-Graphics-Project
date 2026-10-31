using System;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Graphics1
{
    public class Texture
    {
        public static int LoadTexture(string filename)
        {
            string fullpath = "Textures/" + filename;
            if (File.Exists(fullpath) == false)
            {
                throw new FileNotFoundException($"Texture not found at:{fullpath}");
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            int handle = GL.GenTexture();
            UseTexture(handle);

            using (var stream = File.OpenRead(fullpath))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return handle;
        }

        public static void UseTexture(int handle)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}
