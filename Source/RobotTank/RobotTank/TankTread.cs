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
    class TankTread
    {
        const float OpacityChange = 0.005f;
        float opacity;
        float rotation;
        Vector2 pos;
        bool dead;

        public TankTread(Vector2 tankCentrePos, float tankRotation, float speed)
        {
            dead = false;
            rotation = tankRotation;
            opacity = 1f;
            sbyte frontOrBack = -1;
            if (speed < 0)
                frontOrBack = 1;
            Vector2 treadPosOnTank = new Vector2(frontOrBack * 35, -35); //35 to front/ back track, centred y 
            pos = tankCentrePos + Vector2.Transform(treadPosOnTank, Matrix.CreateRotationZ(rotation));  //add the start position to this to get the inital positon of the bullet at the end of the turret
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D treads)
        {
            if (opacity - OpacityChange > 0)
                opacity -= OpacityChange;
            else
                dead = true;

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, Camera.Transform);
            spriteBatch.Draw(treads, pos, null, Color.White * opacity, rotation, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.End();
        }

        public bool Dead { get { return dead; } }
    }
}
