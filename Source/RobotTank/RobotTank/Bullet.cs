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
    class Bullet
    {
        Vector2 pos;
        Vector2 velocity;
        const int MaxSpeed = 14;
        float speed;
        const float speedChange = 0.04f;
        float angle;
        bool alive;
        const int BulletWidth = 60;  //sprite sheet dimensions
        const int BulletHeight = 16;
        const int TimePerFrame = 50;
        Texture2D bulletTex;

        public Bullet(Vector2 tankPos, Vector2 firePos, float _angle, Texture2D bulletTex)
        {
            angle = _angle; //angle to fire at
            alive = true;
            speed = 7f;
            this.bulletTex = bulletTex;
            pos = tankPos + Vector2.Transform(firePos, Matrix.CreateRotationZ(angle));  //add the start position to this to get the inital positon of the bullet at the end of the turret
            velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed); //get velocity using angle and speed
        }

        public void Update()
        {
            if (speed + speedChange < MaxSpeed)
                speed += speedChange;
            pos += velocity;
            velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, Camera.Transform);
            spriteBatch.Draw(bulletTex, pos, null, Color.White, angle, new Vector2(bulletTex.Width / 2, bulletTex.Height / 2), 1f, SpriteEffects.None, 1f);
            spriteBatch.End();
        }

        public void DrawLight(SpriteBatch spriteBatch, Texture2D lightMask)
        {
            float scale = (float)bulletTex.Width / lightMask.Width;
            scale *= 1.3f; //make it 30% bigger than the width of the texture
            spriteBatch.Draw(lightMask, pos, null, Color.White, 0f, new Vector2(lightMask.Width / 2, lightMask.Height / 2), scale, SpriteEffects.None, 1f);
        }

        public BoundingSphere BS { get { return new BoundingSphere(new Vector3(pos.X, pos.Y, 0), BulletWidth / 2); } }
        public Vector2 Position { get { return pos; } }
        public bool Alive 
        {
            get { return alive; }
            set { alive = value; }
        }
    }
}
