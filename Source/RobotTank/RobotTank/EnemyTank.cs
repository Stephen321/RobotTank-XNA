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
    class EnemyTank
    {
        Vector3 velocity;
        protected float rotation;

        const int BulletBorder = 100; //distance outside of the screen the bullets will be removed at
        
        List<Bullet> bullets = new List<Bullet>();
        List<TankTread> tankTreads = new List<TankTread>();
        int timeToPlaceTreads;
        const int MaxTimeToPlaceTreads = 500;
        KeyboardState previousKeyboardState = Keyboard.GetState();
        int WorldWidth, WorldHeight;
        int xBorder, yBorder; //half the screen width
        bool firing;
        int leftOfCamera;
        int rightOfCamera;
        int topOfCamera;
        int bottomOfCamera;

        Matrix bodyTransform;
        Matrix turretTransform;
        int onScreenTimer;
        const int onScreenMaxTime = 500;

        protected static Random rnd = new Random();

        Vector2 topLeft;
        Vector2 topRight;
        Vector2 bottomLeft;
        Vector2 bottomRight;
        Texture2D texture;
        Texture2D turretTexture;
        Vector2[] corners;
        const int MaxCorners = 4;

        protected int ClosestDistance = 200;
        const int CameraBorder = 200;


        bool alive;
        Texture2D rocketTex;
        protected const int HealthBarLength = 150;

        int firingLength;
        float firingLengthTimer;

        protected Vector3 pos;
        protected HealthBar healthBar;
        protected int fireRate;
        float reloadingTimer;
        protected int bulletDamage;
        protected int maxHealth;
        protected int regenRate;
        protected float speedChange;
        protected float maxSpeed;
        protected float speed;
        protected int scoreAmount;
        protected float turretAngle;
        protected bool reverse;
        Vector2 bodyOrigin;
        protected Vector2 turretOrigin;
        SoundEffect rocketFire;

        public EnemyTank(int _WorldWidth, int _WorldHeight, Vector3 playerPos, Texture2D _texture, Texture2D _turretTexture, Texture2D rocketTex)
        {
            WorldWidth = _WorldWidth;
            WorldHeight = _WorldHeight;
            xBorder = WorldWidth / 8;  //world width is 4 times bigger than screen so /8 is half the screen width
            yBorder = WorldHeight / 8;
            firing = false;
            timeToPlaceTreads = 0;
            rotation = (float)(rnd.NextDouble() * MathHelper.TwoPi);
            speed = 0f;
            regenRate = 0;
            speedChange = 0.15f; 
            fireRate = 1100;
            maxHealth = 100;
            maxSpeed = 3f;
            turretAngle = 0;
            velocity = Vector3.Zero;
            onScreenTimer = 0;
            texture = _texture; 
            reloadingTimer = 0;
            firingLength = rnd.Next(500, 1500);
            alive = true;
            bulletDamage = 5;
            scoreAmount = 25;
            reverse = false;
            turretTexture = _turretTexture;
            this.rocketTex = rocketTex;

            bodyOrigin = new Vector2(texture.Bounds.Center.X, texture.Bounds.Center.Y);
            turretOrigin = new Vector2(19, 21);

            int randomPosX = rnd.Next(100, WorldWidth - 100);
            int randomPosY = rnd.Next(100, WorldHeight - 100);
            SetCameraSides(playerPos);
            while (randomPosX > leftOfCamera && randomPosX < rightOfCamera &&
                    randomPosY > topOfCamera && randomPosY < bottomOfCamera) //while its inside the camera then keep finding new positions
            {
                randomPosX = rnd.Next(100, WorldWidth - 100);
                randomPosY = rnd.Next(100, WorldHeight - 100);
            }
            pos = new Vector3(randomPosX, randomPosY, 0);
            healthBar = new HealthBar((int)pos.X - 50, (int)pos.Y - 30, Color.Green, Color.Red, Color.Orange, maxHealth, regenRate, HealthBarLength);
        }

        private void SetCameraSides(Vector3 playerPos)
        {
            leftOfCamera = (int)playerPos.X - xBorder;
            rightOfCamera = (int)playerPos.X + xBorder;
            topOfCamera = (int)playerPos.Y - yBorder;
            bottomOfCamera = (int)playerPos.Y + yBorder;
        }
        public void Update(GameTime gameTime, Vector3 playerPos, float playerAngle, bool playSounds)
        {
            PlaceTreads(gameTime);
            GetRotatedCorners();
            SetCameraSides(playerPos);
            UpdateBulletsTreads();
            bool onScreen = CheckOnScreen(gameTime);

            //if (!onScreen)
            //    Move();

            if (onScreen)
            {
                OnScreenUpdate(gameTime, playerPos, playerAngle, playSounds);
            }

            alive = healthBar.Update(gameTime, new Vector2(pos.X, pos.Y));
            bodyTransform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(pos);
            turretTransform = Matrix.CreateRotationZ(turretAngle) * bodyTransform;
            previousKeyboardState = Keyboard.GetState();
        }

        protected void Fire(GameTime gameTime, bool playSounds)
        { //what to do when firing 
            if (reloadingTimer < fireRate) //if it has still not reloaded
                reloadingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; //increase this reload timer
            else
            {  //it has reloaded
                reloadingTimer = 0f;
                if (rnd.NextDouble() > 0.2f && playSounds) //only play sound 80% of the time and if playsounds is true
                    rocketFire.Play();
                bullets.Add(new Bullet(new Vector2(pos.X, pos.Y), new Vector2(90, 0),turretAngle + rotation, rocketTex));  //fire a rocket
            }
        }

        protected virtual void OnScreenUpdate(GameTime gameTime, Vector3 playerPos, float playerAngle, bool playSounds)
        {//update this stuff when on the board
            Rotate(playerPos, playerAngle);
            CheckFiring(gameTime); //check if firing or moving

            if (firing) //if firing
            {
                Stop();
                Fire(gameTime, playSounds);
            }
            else  //if not firing 
            {
                Move(playerPos);
            }
        }

        private void CheckFiring(GameTime gameTime)
        {//check if firing or moving
            if (firingLengthTimer < firingLength)
            {
                firingLengthTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                firingLengthTimer = 0f;
                firing = !firing; //firing is true for firingLength amount of time and then false for the same amount 
            }
        }

        public void LoadContent(SpriteFont theFont, Texture2D healthBG, Texture2D currentHealth, SoundEffect rocketFire)
        { //load in textures and font
            healthBar.LoadContent(healthBG, currentHealth, theFont);
            this.rocketFire = rocketFire;
        }

        private bool CheckOnScreen(GameTime gameTime)
        {
            bool onScreen = false;
            if (pos.X > leftOfCamera - CameraBorder && pos.X < rightOfCamera + CameraBorder &&
               pos.Y > topOfCamera - CameraBorder && pos.Y < bottomOfCamera + CameraBorder)
            {
                onScreenTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (onScreenTimer > onScreenMaxTime)
                    onScreen = true;
            }
            else
            {
                onScreenTimer = 0;
            }
            return onScreen;
        }

        private void UpdateBulletsTreads()
        {

            for (int i = 0; i < bullets.Count; i++)
            { //update + remove bullets
                bullets[i].Update();
                if (bullets[i].Position.X < -BulletBorder ||
                    bullets[i].Position.X > WorldWidth + BulletBorder ||
                    bullets[i].Position.Y < -BulletBorder ||
                    bullets[i].Position.Y > WorldHeight + BulletBorder ||
                    !bullets[i].Alive)
                {
                    bullets.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < tankTreads.Count; i++)
            {//remove treads that have opacity of 0
                if (tankTreads[i].Dead)
                {
                    tankTreads.RemoveAt(i);
                    i--;
                }
            }
        }

        public void ChangeHealth(int amount)
        {
            healthBar.ChangeHealth(amount);
        }
        public void ResetHealth()
        {
            healthBar.Health = maxHealth;
        }

        protected virtual void Rotate(Vector3 playerPos, float playerAngle)
        {
            Vector2 targetDir = new Vector2(playerPos.X - pos.X, playerPos.Y - pos.Y);
            rotation = (float)(Math.Atan2(targetDir.Y, targetDir.X));
        } 

        private void GetRotatedCorners()
        {
            Vector2 pos2D = new Vector2(pos.X, pos.Y);
            topLeft = Vector2.Transform(new Vector2(-texture.Bounds.Center.X, -texture.Bounds.Center.Y), Matrix.CreateRotationZ(rotation)) + pos2D;
            topRight = Vector2.Transform(new Vector2(texture.Bounds.Center.X, -texture.Bounds.Center.Y), Matrix.CreateRotationZ(rotation)) + pos2D;
            bottomLeft = Vector2.Transform(new Vector2(-texture.Bounds.Center.X, texture.Bounds.Center.Y), Matrix.CreateRotationZ(rotation)) + pos2D;
            bottomRight = Vector2.Transform(new Vector2(texture.Bounds.Center.X, texture.Bounds.Center.Y), Matrix.CreateRotationZ(rotation)) + pos2D;
            corners = new Vector2[MaxCorners] { topLeft, topRight, bottomLeft, bottomRight };
        }

        protected virtual void Move(Vector3 playerPos)
        {
            if (!Intersect1 && !Intersect2 && !Intersect3 && (playerPos - pos).Length() > ClosestDistance)
            {
                if (reverse)
                {
                    if (speed - speedChange > -maxSpeed)
                        speed -= speedChange;
                }
                else
                {
                    if (speed + speedChange < maxSpeed)
                        speed += speedChange;
                }
                velocity = new Vector3((float)Math.Cos(rotation) * speed, (float)Math.Sin(rotation) * speed, 0);
                pos += velocity;
            }
            else
                velocity = new Vector3((float)Math.Cos(rotation) * speed, (float)Math.Sin(rotation) * speed, 0); //keep updatubg velocity to see where the bounding sphere would be and if it would no longer be colliding
        }

        private void Stop()
        {
            if (speed - speedChange * 4 > 0)
                speed -= speedChange * 4; //slow down 4 times faster than speeding up
            else
                speed = 0; //decelerate to 0 speed

        }

        private void PlaceTreads(GameTime gameTime)
        {
            timeToPlaceTreads += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Math.Abs(speed*3));
            if (timeToPlaceTreads > MaxTimeToPlaceTreads)
            {
                timeToPlaceTreads = 0;
                tankTreads.Add(new TankTread(new Vector2(pos.X, pos.Y), rotation, speed));
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tankTreadTex, GameTime gameTime)
        {
            foreach (TankTread t in tankTreads)
                t.Draw(spriteBatch, tankTreadTex);

            //body
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, bodyTransform * Camera.Transform);
            spriteBatch.Draw(texture, Vector2.Zero, null, Color.White, 0f, bodyOrigin, 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            //turret
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, turretTransform * Camera.Transform);
            spriteBatch.Draw(turretTexture, Vector2.Zero, null, Color.White, 0f, turretOrigin, 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            foreach (Bullet b in bullets)
                b.Draw(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Camera.Transform);
            healthBar.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void DrawLight(SpriteBatch spriteBatch, Texture2D lightMask)
        {
            foreach (Bullet b in bullets)
                b.DrawLight(spriteBatch, lightMask);
        }

        public bool CanMove { get; set; }
        public bool Intersect1 { get; set; }
        public bool Intersect2 { get; set; }
        public bool Intersect3 { get; set; }
        public RotatedRectangle RRect { get { return new RotatedRectangle(corners); } }
        public BoundingSphere BS { get { return new BoundingSphere(pos, texture.Width / 2); } }

        public bool Alive { get { return alive; } set { alive = value; } }
        //public BoundingSphere PlayerSphere { get; set; }
        public List<Bullet> Bullets { get { return bullets; } }
        public int BulletDamage { get { return bulletDamage; } }
        public Texture2D Texture { get { return texture; } }
        public Vector3 Position { get { return pos; } }
        public int ScoreAmount { get { return scoreAmount; } }
        public BoundingSphere NextBS { get { return new BoundingSphere( pos + velocity*4, texture.Width / 2); } } //the positon of the bouding sphere in the next frame
    }
}
