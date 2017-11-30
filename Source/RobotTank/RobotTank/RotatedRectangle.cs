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
    class RotatedRectangle
    {
        public Vector2 LowerLeft { get; set; }
        public Vector2 LowerRight { get; set; }
        public Vector2 UpperLeft { get; set; }
        public Vector2 UpperRight { get; set; }

        public RotatedRectangle(Vector2[] corners)
        {
            UpperLeft = corners[0];
            UpperRight = corners[1];
            LowerLeft = corners[2];
            LowerRight = corners[3];
        }
    }
}
