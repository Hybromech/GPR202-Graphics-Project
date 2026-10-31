using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;

namespace Graphics1
{
    public class SkyBox
    {
        private float[] skyboxVertices = {
	// positions          
	-1.0f,  1.0f, -1.0f,
    -1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

    -1.0f,  1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f,  1.0f
};

        private int vbo, vao;
        private int cubemapID;
        private string[] cubemapFaces =
        {
            "skybox/right.jpg",
            "skybox/left.jpg",
            "skybox/top.jpg",
            "skybox/bottom.jpg",
            "skybox/front.jpg",
            "skybox/back.jpg",
        };

        string skyboxVertexShaderCode =
            @"
                #version 330 core
                layout (location=0) in vec3 aPosition;
                uniform mat4 view;
                uniform mat4 projection;
                
                out vec3 texCoords;

                void main()
                {
                    texCoords = aPosition;
                    vec4 pos = vec4(aPosition,1.0) * view * projection;
                    gl_Position = pos.xyww; //skybox has no depth
                }
            ";

        string skyboxFragmentShaderCode =
            @"
                #version 330 core
                out vec4 pixelColor;
                in vec3 texCoords;
                uniform samplerCube skybox;

                void main()
                {
                    // Direction vector points below horizon when texCoords.y < 0
                    if (texCoords.y < 0.0)
                    {
                        // Option A: Smooth fade into a dark floor color
                        vec4 floorColor = vec4(0.04, 0.04, 0.06, 1.0);
                        float factor = clamp(-texCoords.y * 10.0, 0.0, 1.0);
                        pixelColor = mix(texture(skybox, texCoords), floorColor, factor);
                
                        // Option B: Hard cut to solid color/black
                        // pixelColor = vec4(0.05, 0.05, 0.05, 1.0);
                
                        // Option C: Completely discard fragments (shows GL.ClearColor)
                        // discard;
                    }
                    else
                    {
                        pixelColor = texture(skybox, texCoords);
                    }
                }
            ";

        private int shaderProgramObject;

        public SkyBox()
        {
            LoadCubemap();
            Setup();  
        }

        public void Draw(Matrix4 view, Matrix4 proj)
        {
            GL.DepthFunc(DepthFunction.Lequal);

            // Disable face culling so internal walls are visible
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(shaderProgramObject);

            // Strip translation — keeps only rotation
            Matrix4 skyboxView = new Matrix4(new Matrix3(view));

            int loc1 = GL.GetUniformLocation(shaderProgramObject, "view");
            GL.UniformMatrix4(loc1, true, ref skyboxView);

            int loc2 = GL.GetUniformLocation(shaderProgramObject, "projection");
            GL.UniformMatrix4(loc2, true, ref proj);

            GL.BindVertexArray(vao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemapID);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            // Restore default depth test
            GL.DepthFunc(DepthFunction.Less);
        }

        private int LoadCubemap()
        {
            cubemapID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemapID);

            StbImage.stbi_set_flip_vertically_on_load(0);
            for (int i = 0; i < cubemapFaces.Length; i++)
            {
                string fullpath = "Textures/" + cubemapFaces[i];
                if (File.Exists(fullpath) == false)
                {
                    throw new FileNotFoundException("Skybox Texture not found at '" + fullpath + "'");
                }
                using (var stream = File.OpenRead(fullpath))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                }
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            return cubemapID;
        }
        private void Setup()
        {
            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * skyboxVertices.Length, skyboxVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,3 * sizeof(float), 0);

            // Setup our Skybox Shaders
            // allocate vertex shader
            int vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderObject, skyboxVertexShaderCode);
            GL.CompileShader(vertexShaderObject);

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderObject);
            if (vertexShaderInfo != String.Empty)
            {
                Console.WriteLine("SKYBOX VERT: " + vertexShaderInfo);
            }

            // allocate fragment shader
            int fragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderObject, skyboxFragmentShaderCode);
            GL.CompileShader(fragmentShaderObject);

            string fragmentShaderInfo = GL.GetShaderInfoLog(fragmentShaderObject);
            if (fragmentShaderInfo != String.Empty)
            {
                Console.WriteLine("SKYBOX FRAG: " + fragmentShaderInfo);
            }

            // create shader program
            this.shaderProgramObject = GL.CreateProgram();
            GL.AttachShader(this.shaderProgramObject, vertexShaderObject);
            GL.AttachShader(this.shaderProgramObject, fragmentShaderObject);
            GL.LinkProgram(this.shaderProgramObject);

            GL.DetachShader(this.shaderProgramObject, vertexShaderObject);
            GL.DetachShader(this.shaderProgramObject, fragmentShaderObject);
            GL.DeleteShader(vertexShaderObject);
            GL.DeleteShader(fragmentShaderObject);

            GL.UseProgram(shaderProgramObject);
            GL.Uniform1(GL.GetUniformLocation(shaderProgramObject, "skybox"), 0);
        }
    }
}