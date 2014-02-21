﻿using System;
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
        static String shaderDir = @"..\..\..\..\Shaders\";
        static String settingsDir= @"..\..\..\..\Settings\";

        //*****************************************************************************
        //	Global variables
        //*****************************************************************************
        static int basicShaderProgram;
        //static Vector3 lightPosition;

        //*****************************************************************************
        //	Camera state variables
        //*****************************************************************************
        static float camera_theta = pi / 6.0f;
        static float camera_phi = pi / 4.0f;
        static float camera_r = 30.0f;
        static float camera_target_altitude = 5.2f;
        static float camera_horizontal_delta = 0.1f;
        static float camera_vertical_delta = 1.0f;
        //static Vector4 camera_lookAt = new Vector4(0.0f, camera_target_altitude, 0.0f, 1.0f);
        static Matrix4 camera_rotation_matrix = Matrix4.Identity;

        static float light_theta = pi / 6.0f;
        static float light_phi = pi / 4.0f;
        static float light_r = 30.0f;

        static Entity myCar;



        static ArrayList keyList = new ArrayList();

        static int source = 0;
        static bool musicPaused;
        static bool keyHandled = false;

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
            using (StreamReader vertReader = new StreamReader(vShaderPath),
                                fragReader = new StreamReader(fShaderPath))
            {
                GL.ShaderSource(vShader, vertReader.ReadToEnd());
                GL.ShaderSource(fShader, fragReader.ReadToEnd());
            }
            GL.CompileShader(vShader);
            Console.WriteLine("Vertex Shader: " + GL.GetShaderInfoLog(vShader));
            GL.CompileShader(fShader);
            Console.WriteLine("Fragment Shader: " + GL.GetShaderInfoLog(fShader));

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vShader);
            GL.AttachShader(shaderProgram, fShader);
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
            keyHandled = false;
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
                    Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                    myCar = new Entity("TeapotCar\\Teapot car\\Teapot-no-materials-tri");//"Cube\\koobe");//"TeapotCar\\Teapot car\\Teapot-no-materials-tri");//"map\\uggly_test_track_Triangulate");//

                    //Set up shaders
                    basicShaderProgram = loadShaderProgram(shaderDir + "Simple_VS.glsl", shaderDir + "Simple_FS.glsl");
                    GL.BindAttribLocation(basicShaderProgram, 0, "position");
                    GL.BindAttribLocation(basicShaderProgram, 1, "normalIn");
                    GL.BindAttribLocation(basicShaderProgram, 2, "textCoordIn");
                    GL.BindFragDataLocation(basicShaderProgram, 0, "fragmentColor");
                    GL.LinkProgram(basicShaderProgram);

                    Console.WriteLine(GL.GetProgramInfoLog(basicShaderProgram));

                    //Load uniforms and texture
                    GL.UseProgram(basicShaderProgram);
                    myCar.setUpMtl();
                    //myCar.loadTexture();
                    GL.Enable(EnableCap.Texture2D);
                    GL.UseProgram(0);

                    //Set up Uniforms

                    //lightPosition = new Vector3(up);

                    game.KeyDown += handleKeyDown;
                    game.KeyUp += handleKeyUp;


                    //Music
                    source = Audio.initSound();
                    SettingsParser.Init(settingsDir + "default.ini");
                    

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
                        Audio.deleteBS(source);
                        game.Exit();
                    }
                    else if (game.Keyboard[Key.Number9])
                    {
                        if (!keyHandled)
                        {
                            Audio.increaseGain(source);
                            keyHandled = !keyHandled;
                        }
                    }
                    else if (game.Keyboard[Key.Number0])
                    {
                        if (!keyHandled)
                        {
                            Audio.decreaseGain(source);
                            keyHandled = !keyHandled;
                        }
                    }
                    else if (game.Keyboard[Key.Space])
                    {
                        if (!keyHandled)
                        {
                            if (musicPaused)
                            {
                                Audio.playSound(source);
                                musicPaused = !musicPaused;
                                keyHandled = !keyHandled;
                            }
                            else
                            {
                                Audio.pauseSound(source);
                                musicPaused = !musicPaused;
                                keyHandled = !keyHandled;
                            }
                        }
                    }
                    else if (game.Keyboard[Key.O])
                    {
                        if (!keyHandled)
                        {
                            source = Audio.nextTrack(source);
                            keyHandled = !keyHandled;
                        }
                    }


                    updateCamera();

                    //Audio management
                    if (Audio.audioStatus(source) == 0)
                        Audio.playSound(source);
                    else if (Audio.audioStatus(source) == 3)
                        source = Audio.nextTrack(source);

                };

                game.RenderFrame += (sender, e) =>
                {
                    //GL.ClearColor(0.2f, 0.2f, 0.8f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    int w = game.Width;
                    int h = game.Height;

                    GL.UseProgram(basicShaderProgram);

                    Vector3 camera_position = sphericalToCartesian(camera_theta, camera_phi, camera_r);
                    //camera_lookAt = new Vector3(0.0f, camera_target_altitude, 0.0f);
                    Vector3 camera_lookAt = new Vector3(0.0f, 0.0f, 0.0f);//Vector4.Transform(camera_lookAt, camera_rotation_matrix);
                    Matrix4 viewMatrix = Matrix4.LookAt(camera_position, camera_lookAt, up);
                    Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(pi / 4, (float)w / (float)h, 0.1f, 1000f);
                    Matrix4 modelMatrix = Matrix4.Identity;
                    // Here we start getting into the lighting model
                    Matrix4 modelViewMatrix = modelMatrix * viewMatrix; //I know this is opposite see down why
                    GL.UniformMatrix4(GL.GetUniformLocation(basicShaderProgram, "modelViewMatrix"), false, ref modelViewMatrix);
                    Matrix4 modelViewProjectionMatrix = modelViewMatrix * projectionMatrix; ///VAFAN??!??!? varför blir det fel annars?
                    GL.UniformMatrix4(GL.GetUniformLocation(basicShaderProgram, "modelViewProjectionMatrix"), false, ref modelViewProjectionMatrix);




                    Matrix4 normalMatrix = Matrix4.Invert(Matrix4.Transpose(viewMatrix * modelMatrix));
                    GL.UniformMatrix4(GL.GetUniformLocation(basicShaderProgram, "normalMatrix"), false, ref normalMatrix);

                    Vector4 lightPosition = new Vector4(sphericalToCartesian(light_theta, light_phi, light_r), 1.0f);
                    Vector4 viewSpaceLightPosition = Vector4.Transform(lightPosition, modelViewMatrix);
                    GL.Uniform3(GL.GetUniformLocation(basicShaderProgram, "viewSpaceLightPosition"), viewSpaceLightPosition.Xyz);

                    /*
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref viewMatrix);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref projectionMatrix);
                    */

                    GL.Enable(EnableCap.DepthTest);
                    GL.Enable(EnableCap.CullFace);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, myCar.getTextureId());
                    myCar.render(basicShaderProgram);
                    GL.End();

                    game.SwapBuffers();
                    GL.UseProgram(0);

                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }
    }
}
