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
    class Drop
    {

        Vector2 pos;
        Texture2D texture;
        bool alive;
        float timer; //timer until drop is no longer alive
        const int MaxTime = 5000;
        float opacity; //fades out
        
        public Drop()
        {
            alive = true;
            pos = new Vector2(100, 100);
            timer = MaxTime;
            opacity = 1;
        }

        public void LoadContent(ContentManager theContentManager, string assetName)
        {
            texture = theContentManager.Load<Texture2D>(assetName); //load own content
        }

        public void Draw(SpriteBatch theSpriteBatch)
        {
            if (alive)
            {
                theSpriteBatch.Draw(texture, pos, null, Color.White * opacity, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 1f);
            }
        }
        public void DrawLight(SpriteBatch theSpriteBatch, Texture2D lightMask)
        {
            if (alive)
            {
                float scale = (float)texture.Width / lightMask.Width;
                scale *= 1.8f; //make it 80% bigger than the width of the texture
                theSpriteBatch.Draw(lightMask, pos, null, Color.White, 0f, new Vector2(lightMask.Width / 2, lightMask.Height / 2), scale, SpriteEffects.None, 1f);
            }
        }
        public void Update(GameTime gameTime)
        {
            if (alive)
            {
                timer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                opacity = timer / MaxTime;
                if (timer < 0)
                    Reset(); //drop moves off the screen
            }
        }

        public void Reset()
        {
            alive = false;
            pos = new Vector2(-100, -100); //move drop off the screen
        }
        public void Activate(Vector2 pos)
        {
            timer = MaxTime;
            this.pos = pos;
            alive = true;
            opacity = 1f;
        }

        public bool Alive { get { return alive; } }

        public BoundingSphere BS
        {
            get { return new BoundingSphere(new Vector3(pos.X, pos.Y,0), texture.Width / 2); }
        }

        public int halfWidth
        {
            get { return texture.Width / 2; }
        }

        public int halfHeight
        {
            get { return texture.Height / 2; }
        }
    }
}
