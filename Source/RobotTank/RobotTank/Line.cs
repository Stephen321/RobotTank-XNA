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
    class Line
    {
        Vector2 p1, p2; //this will be the position in the center of the line
        int length, thickness; //length and thickness of the line, or width and height of rectangle
        Rectangle rect; //where the line will be drawn
        float rotation; // rotation of the line, with axis at the center of the line
        Color color;


        //p1 and p2 are the two end points of the line
        public Line(int thickness, Color color)
        {
            this.thickness = thickness;
            this.color = color;
        }

        public void Update(GameTime gameTime)
        {
            length = (int)Vector2.Distance(p1, p2); //gets distance between the points
            rotation = getRotation(p1.X, p1.Y, p2.X, p2.Y); //gets angle between points(method on bottom)
            rect = new Rectangle((int)p1.X, (int)p1.Y, length, thickness);

            //To change the line just change the positions of p1 and p2
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Texture2D pixel)
        {
            spriteBatch.Draw(pixel, rect, null, color, rotation, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        //this returns the angle between two points in radians 
        private float getRotation(float x, float y, float x2, float y2)
        {
            float adj = x - x2;
            float opp = y - y2;
            float tan = opp / adj;
            float res = MathHelper.ToDegrees((float)Math.Atan2(opp, adj));
            res = (res - 180) % 360;
            if (res < 0) { res += 360; }
            res = MathHelper.ToRadians(res);
            return res;
        }

        public Vector2 P1
        {
            get { return p1; }
            set { p1 = value; }
        }
        public Vector2 P2
        {
            get { return p2; }
            set { p2 = value; }
        }
    }
}