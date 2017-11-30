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
    class PlayerTank
    {
        Matrix bodyTransform;
        Matrix turretTransform;
        Vector3 bodyPos;
        Vector3 velocity;
        float bodyAngle;
        float turretAngle;
        const float BodyTurnSpeed = 0.03f; //.02
        const float TurretTurnSpeed = 0.05f;
        const float BodyMoveSpeed = 0.015f; //.015
        const float MaxSpeed = 5f;  //5
        float speed;
        const int BulletBorder = 100; //distance outside of the screen the bullets will be removed at

        List<Bullet> bullets = new List<Bullet>();
        List<TankTread> tankTreads = new List<TankTread>();
        int timeToPlaceTreads;
        const int MaxTimeToPlaceTreads = 500;
        KeyboardState previousKeyboardState = Keyboard.GetState();
        int WorldWidth, WorldHeight;
        int xBorder, yBorder; //the border around the world where the camera will stop following the player so it doesnt show outside the map

        float reloadtimer;
        const int FireRate = 400;
        Vector2 topLeft; 
        Vector2 topRight;
        Vector2 bottomLeft;
        Vector2 bottomRight;
        Vector2[] corners;
        const int MaxCorners = 4;
        Texture2D bodyTex;
        bool alive;
        int bulletDamage;
        Texture2D rocketTex;
        bool destroyed;
        SoundEffect fireSound;

        public PlayerTank(int _WorldWidth, int _WorldHeight, Texture2D _bodyTex, Texture2D rocketTex, SoundEffect fireSound)
        {
            this.fireSound = fireSound;
            this.rocketTex = rocketTex;
            WorldWidth = _WorldWidth;
            WorldHeight = _WorldHeight;
            xBorder = WorldWidth / 8;  //world width is 4 times bigger than screen so /8 is half the screen width
            yBorder = WorldHeight / 8;

            timeToPlaceTreads = 0;
            bodyAngle = 0f;
            turretAngle = 0f;
            speed = 0f;
            velocity = Vector3.Zero;
            bodyPos = new Vector3(WorldWidth / 2, WorldHeight / 2, 0);
            bodyTransform =  Matrix.CreateTranslation(bodyPos) * Matrix.CreateRotationZ(bodyAngle);
            bodyTex = _bodyTex;
            bulletDamage = 20;
            alive = true;
            destroyed = false;
        }

        public void Update(GameTime gameTime, Vector2 bodyOrigin)
        {
            PlaceTreads(gameTime);
            GetRotatedCorners();

            CheckMovement();
            CheckFire(gameTime);

            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Update();
                if (bullets[i].Position.X < -BulletBorder ||
                    bullets[i].Position.X > WorldWidth + BulletBorder ||
                    bullets[i].Position.Y < -BulletBorder ||
                    bullets[i].Position.Y > WorldHeight + BulletBorder ||
                    !bullets[i].Alive) //not alive
                {
                    bullets.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < tankTreads.Count; i++)
            {
                if (tankTreads[i].Dead)
                {
                    tankTreads.RemoveAt(i);
                    i--;
                }
            }
            if (!destroyed)
            {
                Move();
                CheckBounds(bodyOrigin);
                bodyTransform = Matrix.CreateRotationZ(bodyAngle) * Matrix.CreateTranslation(bodyPos);
                turretTransform = Matrix.CreateRotationZ(turretAngle) * bodyTransform;
                CentreCamera();
            }

            previousKeyboardState = Keyboard.GetState();
        }

        private void Stop()
        {
            if (speed > 0) //was moving forward
            {
                if (speed - BodyMoveSpeed * 4 > 0)
                    speed -= BodyMoveSpeed * 4; //slow down 4 times faster than speeding up
                else
                    speed = 0;
            }
            if (speed < 0)  //was moving backwards
            {
                if (speed + BodyMoveSpeed * 4 < 0)
                    speed += BodyMoveSpeed * 4; 
                else
                    speed = 0;
            }
        }
        private void CheckMovement()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                bodyAngle -= BodyTurnSpeed;
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                bodyAngle += BodyTurnSpeed;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (speed + BodyMoveSpeed < MaxSpeed) //move forwards
                    speed += BodyMoveSpeed;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                if (speed - BodyMoveSpeed > -MaxSpeed)
                    speed -= BodyMoveSpeed;    //move backwards
            }
            else
            {
                Stop();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                turretAngle -= TurretTurnSpeed;
            else if (Keyboard.GetState().IsKeyDown(Keys.X))
                turretAngle += TurretTurnSpeed;
        }

        private void CheckFire(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && previousKeyboardState.IsKeyDown(Keys.LeftControl))
            {//held down
                if (reloadtimer < FireRate)
                {
                    reloadtimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                else
                {
                    fireSound.Play();
                    bullets.Add(new Bullet(new Vector2(bodyPos.X, bodyPos.Y), new Vector2(102, 0), turretAngle + bodyAngle, rocketTex));
                    reloadtimer = 0;
                }
            }
        }

        private void Move()
        {
            velocity = new Vector3((float)Math.Cos(bodyAngle) * speed, (float)Math.Sin(bodyAngle) * speed, 0);
            bodyPos += velocity;
        }

        private void CheckBounds(Vector2 bodyOrigin)
        {
            if (bodyPos.X < bodyOrigin.X || bodyPos.X > WorldWidth - bodyOrigin.X ||
               bodyPos.Y < bodyOrigin.Y || bodyPos.Y > WorldHeight - bodyOrigin.Y)
                speed = 0; //change , doesnt work cause rect is rotated
            //use ifs to change speed
            bodyPos.X = MathHelper.Clamp(bodyPos.X, bodyOrigin.X, WorldWidth - bodyOrigin.X);
            bodyPos.Y = MathHelper.Clamp(bodyPos.Y, bodyOrigin.X, WorldHeight - bodyOrigin.X);
        }
        private void GetRotatedCorners()
        { //get the corners of the rotated rectangle around this tank
            Vector2 bodyPos2D = new Vector2(bodyPos.X, bodyPos.Y);
            topLeft = Vector2.Transform(new Vector2(-bodyTex.Bounds.Center.X, -bodyTex.Bounds.Center.Y), Matrix.CreateRotationZ(bodyAngle)) + bodyPos2D;
            topRight = Vector2.Transform(new Vector2(bodyTex.Bounds.Center.X, -bodyTex.Bounds.Center.Y), Matrix.CreateRotationZ(bodyAngle)) + bodyPos2D;
            bottomLeft = Vector2.Transform(new Vector2(-bodyTex.Bounds.Center.X, bodyTex.Bounds.Center.Y), Matrix.CreateRotationZ(bodyAngle)) + bodyPos2D;
            bottomRight = Vector2.Transform(new Vector2(bodyTex.Bounds.Center.X, bodyTex.Bounds.Center.Y), Matrix.CreateRotationZ(bodyAngle)) + bodyPos2D;
            corners = new Vector2[MaxCorners] { topLeft, topRight, bottomLeft, bottomRight };
        }

        private void PlaceTreads(GameTime gameTime)
        {
            timeToPlaceTreads += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Math.Abs(speed*3));
            if (timeToPlaceTreads > MaxTimeToPlaceTreads)
            {
                timeToPlaceTreads = 0;
                tankTreads.Add(new TankTread(new Vector2(bodyPos.X, bodyPos.Y), bodyAngle, speed));
            }
        }

        private void CentreCamera()
        {
            if (bodyPos.X > xBorder && bodyPos.X < WorldWidth - xBorder &&
                bodyPos.Y > yBorder && bodyPos.Y < WorldHeight - yBorder)
            {
                Camera.Pos = new Vector2(bodyPos.X, bodyPos.Y);
            }
            else
            {
                if (bodyPos.X > xBorder && bodyPos.X < WorldWidth - xBorder)
                    Camera.Pos = new Vector2(bodyPos.X, Camera.Pos.Y);
                if (bodyPos.Y > yBorder && bodyPos.Y < WorldHeight - yBorder)
                    Camera.Pos = new Vector2(Camera.Pos.X, bodyPos.Y);
            }
        }       
        public void Draw(SpriteBatch spriteBatch, Texture2D turretTex, Texture2D treadTex)
        {
            foreach (TankTread t in tankTreads)
                t.Draw(spriteBatch, treadTex);  

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, bodyTransform * Camera.Transform);
            spriteBatch.Draw(bodyTex, Vector2.Zero, null, Color.White, 0f, new Vector2(bodyTex.Bounds.Center.X, bodyTex.Bounds.Center.Y), 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, turretTransform * Camera.Transform);
            spriteBatch.Draw(turretTex, Vector2.Zero, null, Color.White, 0f, new Vector2(30, 23), 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            foreach (Bullet b in bullets)
                b.Draw(spriteBatch);
        }

        public void DrawLight(SpriteBatch spriteBatch, Texture2D lightMask)
        {
            foreach (Bullet b in bullets)
                b.DrawLight(spriteBatch, lightMask);
            spriteBatch.Draw(lightMask, new Vector2(bodyPos.X, bodyPos.Y), null, Color.White, 0f, new Vector2(lightMask.Width / 2, lightMask.Height / 2), 1f, SpriteEffects.None, 1f);
        }

        public void Reset()
        {
            timeToPlaceTreads = 0;
            bodyAngle = 0f;
            turretAngle = 0f;
            speed = 0f;
            velocity = Vector3.Zero;
            bodyPos = new Vector3(WorldWidth / 2, WorldHeight / 2, 0);
            bodyTransform = Matrix.CreateTranslation(bodyPos) * Matrix.CreateRotationZ(bodyAngle);
            alive = true;
        }

        public Vector3 Position { get { return bodyPos; } }

        public RotatedRectangle RRect
        {
            get { return new RotatedRectangle(corners); }
        }

        public BoundingSphere BS
        {
            get { return new BoundingSphere(bodyPos, bodyTex.Width / 2); }
        }

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }
        public bool Destroyed
        {
            set { destroyed = value; }
        }

        public Texture2D Texture
        {
            get { return bodyTex; }
        }
        public float Rotation
        {
            get { return bodyAngle; }
        }
        public Matrix Transform
        {
            get { return bodyTransform; }
        }

        public int BulletDamage { get { return bulletDamage; } }
        public List<Bullet> Bullets { get { return bullets; } }
    }
}
