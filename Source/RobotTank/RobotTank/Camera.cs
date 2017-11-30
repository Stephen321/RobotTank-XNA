using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RobotTank
{
    public class Camera
    {
        static GraphicsDevice graphicDevice;
        static Vector2 position;
        static float zoom; 

        public static GraphicsDevice GraphicDevice
        {
            set { graphicDevice = value; }
        }

        public static Matrix Transform
        {
            get { return Matrix.CreateTranslation(-Pos.X, -Pos.Y, 0) * Matrix.CreateScale(zoom, zoom, 0) * Matrix.CreateTranslation(Origin); }
        }

        public static Vector3 Origin
        {
            get { return new Vector3(graphicDevice.Viewport.Width * 0.5f, graphicDevice.Viewport.Height * 0.5f, 0); }
        }
        public static Vector2 Pos
        {
            set { position = value; }
            get { return position; }
        }
        public static float Zoom
        {
            set { zoom = value; }
            get { return zoom; }
        }
    }

}

