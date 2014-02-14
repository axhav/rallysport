using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;



namespace RallysportGame
{


    class Program
    {
        //*****************************************************************************
        //	Useful constants
        //*****************************************************************************
        const float pi = MathHelper.Pi;
        static Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        static String modelsDir = @"..\..\..\..\Models\";
        static String shaderDir = @"..\..\..\..\Shaders\";

        //*****************************************************************************
        //	Global variables
        //*****************************************************************************
        static int basicShaderProgram;
        static Vector3 lightPosition;

        //*****************************************************************************
        //	Camera state variables
        //*****************************************************************************
        static float camera_theta = pi / 6.0f;
        static float camera_phi = pi / 4.0f;
        static float camera_r = 300.0f;
        static float camera_target_altitude = 5.2f;
        static float camera_horizontal_delta = 0.1f;
        static float camera_vertical_delta = 1.0f;
        static Vector4 camera_lookAt = new Vector4(0.0f, camera_target_altitude, 0.0f, 1.0f);
        static Matrix4 camera_rotation_matrix = Matrix4.Identity;

        static Entity myCar;
        static ArrayList keyList = new ArrayList();

        // Helper function to turn spherical coordinates into cartesian (x,y,z)
        static Vector3 sphericalToCartesian(float theta, float phi, float r)
        {
            return new Vector3((float)(r * Math.Sin(theta) * Math.Sin(phi)),
                                (float)(r * Math.Cos(phi)),
                                (float)(r * Math.Cos(theta) * Math.Sin(phi)));
        }

        static int loadShaderProgram(String vShaderPath, String fShaderPath)
        {
            int shaderProgram;
            int vShader = GL.CreateShader(ShaderType.VertexShader);
            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            using (TextReader vertReader = File.OpenText(vShaderPath),
                              fragReader = File.OpenText(fShaderPath))
            {
                GL.ShaderSource(vShader, vertReader.ReadToEnd());
                GL.ShaderSource(fShader, fragReader.ReadToEnd());
                Console.WriteLine(vertReader.ReadToEnd().Length);
            }
            
            GL.CompileShader(vShader);
            GL.CompileShader(fShader);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vShader);
            GL.AttachShader(shaderProgram, fShader);
            GL.BindAttribLocation(shaderProgram, 0, "position");
            GL.BindFragDataLocation(shaderProgram, 0, "fragmentColor");
            GL.LinkProgram(shaderProgram);
            GL.DetachShader(shaderProgram, vShader);
            GL.DetachShader(shaderProgram, fShader);
            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);

            ErrorCode error = GL.GetError();
            if (error != 0)
                Console.WriteLine(error);

            return shaderProgram;
        }


        /// <summary>
        /// Will handle key events so multiple keys can be triggered at once
        /// 
        /// alla loopar kan säkert optimeras och borde kanske ses över detta e mest som ett snabb test 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        static void handleKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!keyList.Contains(e.Key)) /// FULHACK tydligen så kan den annars generera 30+ keydown events om man håller inne
                keyList.Add(e.Key);
        }
        static void handleKeyUp(object sender, KeyboardKeyEventArgs e)
        {

            for (int i = 0; i < keyList.Count; i++)
            {
                if (keyList[i].Equals(e.Key))
                {
                    keyList.RemoveAt(i);
                }
            }
        }

        static void updateCamera()
        {
            foreach (Key key in keyList)
            {
                switch (key)
                {
                    case Key.A:
                        camera_theta -= camera_horizontal_delta;
                        break;
                    case Key.D:
                        camera_theta += camera_horizontal_delta;
                        break;
                    case Key.W:
                        camera_r -= camera_vertical_delta;
                        break;
                    case Key.S:
                        camera_r += camera_vertical_delta;
                        break;
                    default:
                        break;
                }
            }
        }

        static void Main(string[] args)
        {

            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                    myCar = new Entity(new Meshomatic.ObjLoader().LoadFile(modelsDir + "TeapotCar\\Teapot car\\Teapot-no-materials.obj"));

                    //Set up shaders
                    basicShaderProgram = loadShaderProgram(shaderDir + "shader.vert", shaderDir + "shader.frag");
                    
                    int linkStatus = 0;
                    unsafe { GL.GetProgram(basicShaderProgram, GetProgramParameterName.LinkStatus, &linkStatus); }
                    if (linkStatus == 0)
                        Console.WriteLine("Linking unsuccessful!");

                    GL.ValidateProgram(basicShaderProgram);
                    int status = 0;
                    unsafe { GL.GetProgram(basicShaderProgram, GetProgramParameterName.ValidateStatus, &status); }
                    if (status == 0)
                        Console.WriteLine("Shaderprogram invalid!");


                    lightPosition = new Vector3(up);

                    game.KeyDown += handleKeyDown;
                    game.KeyUp += handleKeyUp;

                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    camera_rotation_matrix = Matrix4.Identity;
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                    updateCamera();
                };

                game.RenderFrame += (sender, e) =>
                {
                    GL.ClearColor(0.2f, 0.2f, 0.8f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    int w = game.Width;
                    int h = game.Height;
                    //GL.UseProgram(basicShaderProgram);
                    
                    Vector3 camera_position = sphericalToCartesian(camera_theta, camera_phi, camera_r);
                    //camera_lookAt = new Vector3(0.0f, camera_target_altitude, 0.0f);
                    camera_lookAt = Vector4.Transform(camera_lookAt, camera_rotation_matrix);
                    Matrix4 viewMatrix = Matrix4.LookAt(camera_position, new Vector3(camera_lookAt), up);
                    Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(pi / 4, w / h, 0.1f, 1000f);

                    Matrix4 modelViewProjectionMatrix = projectionMatrix*viewMatrix;
                    // Here we start getting into the lighting model
                    /*
                    unsafe {
                        float* mvpp = (float*) &modelViewProjectionMatrix;
                        GL.UniformMatrix4(GL.GetUniformLocation(basicShaderProgram, "modelViewProjectionMatrix"), 1, false, mvpp); 
                    }
                    */
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref viewMatrix);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref projectionMatrix);
                    
                    myCar.render();
                    game.SwapBuffers();
                    GL.UseProgram(0);

                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }
    }
}
