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
    class Drone
    {

        Vector2 pos;
        static Texture2D texture;
        bool alive;
        float orbitAngle;
        const float OrbitAngleChange = 0.01f;
        int timer; //timer until drone is no longer alive
        const int MaxTime = 10000; //last this many milliseconds
        Vector2 startPos;
        float rotation;
        ParticleEngine particleEngine;
        public Drone()
        {
            alive = false; 
            pos = new Vector2(-100, -100);
            timer = MaxTime;
            startPos = new Vector2(90, 0); 
            particleEngine = new ParticleEngine(pos, Color.Green, Color.DarkGreen);
        }

        public void LoadContent(ContentManager theContentManager, string assetName, Texture2D particleTex)
        {
            texture = theContentManager.Load<Texture2D>(assetName); //load own content
            particleEngine.LoadContent(particleTex);
        }
        public void Draw(SpriteBatch theSpriteBatch)
        {
            if (alive)
            {
                theSpriteBatch.Draw(texture, pos, null, Color.White, rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 1f);
                particleEngine.Draw(theSpriteBatch);
            }
        }
        public void Update(Vector2 playerPos, GameTime gameTime)
        {
            if (alive)
            {
                //Matrix playerTransform =  Matrix.CreateTranslation(new Vector3(playerPos.X, playerPos.Y, 0));
                orbitAngle += OrbitAngleChange;
                pos =   Vector2.Transform(startPos, Matrix.CreateRotationZ(orbitAngle) );//* playerTransform);  
                rotation = (float)(Math.Atan2(-pos.Y, -pos.X));
                timer -= gameTime.ElapsedGameTime.Milliseconds;
                particleEngine.EmitterLocation = pos;
                Vector2 particleVelocity = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                particleVelocity.Normalize();
                particleVelocity *= 5;
                particleEngine.Update(1, 10, particleVelocity);
                if (timer < 0)
                    Reset(); 
            }
        }

        public void Reset()
        {
            alive = false;
            pos = new Vector2(-100, -100); //move drop off the screen
        }
        public void Activate()
        {
            timer = MaxTime;
            alive = true;
            pos = startPos;
        }

        public bool Alive { get { return alive; } }

        public BoundingSphere BS
        {
            get { return new BoundingSphere(new Vector3(pos.X, pos.Y, 0), texture.Width / 2); }
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
