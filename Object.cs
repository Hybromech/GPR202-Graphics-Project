using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Graphics1
{
    public class Object
    {
        public int texture;

        public void DrawModel(Mesh mesh, Matrix4 modelMatrix, int locModel)
        {
            TextureLoader.Bind(texture);
            GL.UniformMatrix4(locModel, true, ref modelMatrix);
            mesh.Draw();
        }

        public void DrawModel_Custom(Model_Loader ml, Matrix4 modelMatrix, int locModel)
        {
            TextureLoader.Bind(texture);
            GL.UniformMatrix4(locModel, true, ref modelMatrix);
            ml.Draw();
        }
    }
}