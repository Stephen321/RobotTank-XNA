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
    class Stain
    {
        const float OpacityChange = 0.00001f; //very slowly fades out
        float opacity;
        Vector2 pos;
        bool dead;

        public Stain(Vector2 pos)
        {
            dead = false;
            opacity = 1f;
            this.pos = pos;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, GameTime gametime)
        {
            if (opacity - OpacityChange > 0)
                opacity -= (float)(OpacityChange * gametime.ElapsedGameTime.TotalMilliseconds);
            else
                dead = true;

            spriteBatch.Draw(texture, pos, null, Color.White * opacity, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 1f);
        }

        public bool Dead { get { return dead; } }
    }
}
