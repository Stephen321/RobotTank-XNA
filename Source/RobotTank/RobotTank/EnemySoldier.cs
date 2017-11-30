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
    class EnemySoldier
    {
        Vector3 velocity;
        float rotation;

        const int BulletBorder = 100; //distance outside of the screen the bullets will be removed at

        List<Bullet> bullets = new List<Bullet>();
        KeyboardState previousKeyboardState = Keyboard.GetState();
        int WorldWidth, WorldHeight;
        int xBorder, yBorder; //half the screen width

        Matrix bodyTransform;

        static Random rnd = new Random();

        Texture2D texture;

        int ClosestDistance = 150;
        const int DetectionDistance = 500;
        const int CameraBorder = 200;
        SoundEffect fireSound;


        bool alive;
        Texture2D bulletSpriteSheet;
        const int HealthBarLength = 150;
        Vector3 pos;
        HealthBar healthBar;
        const int FireRate = 150;
        float reloadingTimer;
        int bulletDamage;
        int regenRate;
        const float SpeedChange = 0.01f;
        float speed;
        int scoreAmount;
        float turretAngle;
        bool reverse;
        Vector2 origin;
        const int MaxSpeed = 2;
        const int MaxHealth = 50;
        float timer;
        bool firing;
        int burstLength;
        int moveOnlyLength;

        public EnemySoldier(int _WorldWidth, int _WorldHeight, Vector3 playerPos, Texture2D _texture, Texture2D bulletSpriteSheet)
        {
            WorldWidth = _WorldWidth;
            WorldHeight = _WorldHeight;
            xBorder = WorldWidth / 8;  //world width is 4 times bigger than screen so /8 is half the screen width
            yBorder = WorldHeight / 8;
            rotation = (float)(rnd.NextDouble() * MathHelper.TwoPi);
            burstLength = rnd.Next(300, 900); //random amount of burst of fire time
            moveOnlyLength = rnd.Next(1000, 5000); //random move only time
            speed = 0f;
            regenRate = 0;
            firing = false;
            timer = 0;
            turretAngle = 0;
            velocity = Vector3.Zero;
            texture = _texture;
            reloadingTimer = 0;
            alive = true;
            bulletDamage = 1;
            scoreAmount = 25;
            reverse = false;
            this.bulletSpriteSheet = bulletSpriteSheet;

            origin = new Vector2(texture.Bounds.Center.X, texture.Bounds.Center.Y);

            int randomPosX = rnd.Next(100, WorldWidth - 100);
            int randomPosY = rnd.Next(100, WorldHeight - 100);
            int leftOfCamera = (int)playerPos.X - xBorder;
            int rightOfCamera = (int)playerPos.X + xBorder;
            int topOfCamera = (int)playerPos.Y - yBorder;
            int bottomOfCamera = (int)playerPos.Y + yBorder;

            while (randomPosX > leftOfCamera && randomPosX < rightOfCamera &&
                    randomPosY > topOfCamera && randomPosY < bottomOfCamera) //while its inside the camera then keep finding new positions
            {
                randomPosX = rnd.Next(100, WorldWidth - 100);
                randomPosY = rnd.Next(100, WorldHeight - 100);
            }
            pos = new Vector3(randomPosX, randomPosY, 0);
            healthBar = new HealthBar((int)pos.X - 50, (int)pos.Y - 30, Color.Green, Color.Red, Color.Orange, MaxHealth, regenRate, HealthBarLength);
        }

        private void CheckFiring(GameTime gameTime)
        {//check if in burst or not
            if (timer < burstLength)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; //firing is true while in burst amount of time
                firing = true;
            }
            else if (timer < moveOnlyLength)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; 
                firing = false;
            }
            else
            {
                timer = 0f; 
                burstLength = rnd.Next(300, 900); //random amount of burst of fire time
                moveOnlyLength = rnd.Next(1000, 5000); //random move only time
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPos)
        {
            UpdateBullets();
            CheckFiring(gameTime);
            bool inRange = false;

            if ((playerPos - pos).Length() < DetectionDistance)
                inRange = true;

            if (inRange)
            {
                Rotate(playerPos);
                Fire(gameTime);
                Move(playerPos);
            }

            alive = healthBar.Update(gameTime, new Vector2(pos.X, pos.Y));
            bodyTransform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(pos);
            previousKeyboardState = Keyboard.GetState();
        }

        private void Fire(GameTime gameTime)
        { //what to do when firing 
            if (firing)
            {
                if (reloadingTimer < FireRate) //if it has still not reloaded
                    reloadingTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; //increase this reload timer
                else
                {  //it has reloaded
                    reloadingTimer = 0f;
                    fireSound.Play();
                    bullets.Add(new Bullet(new Vector2(pos.X, pos.Y), new Vector2(50, 10), turretAngle + rotation, bulletSpriteSheet));  //fire a bullet
                }
            }
        }

        public void LoadContent(SpriteFont theFont, Texture2D healthBG, Texture2D currentHealth, SoundEffect fireSound)
        { //load in textures and font
            healthBar.LoadContent(healthBG, currentHealth, theFont);
            this.fireSound = fireSound; 
        }

        private void UpdateBullets()
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
        }

        public void ChangeHealth(int amount)
        {
            healthBar.ChangeHealth(amount);
        }
        public void ResetHealth()
        {
            healthBar.Health = MaxHealth;
        }

        private void Rotate(Vector3 playerPos)
        {
            Vector2 targetDir = new Vector2(playerPos.X - pos.X, playerPos.Y - pos.Y);
            rotation = (float)(Math.Atan2(targetDir.Y, targetDir.X));
        }

        private void Move(Vector3 playerPos)
        {
            if (!Intersect1 && !Intersect2 && !Intersect3 && (playerPos - pos).Length() > ClosestDistance)
            {
                if (reverse)
                {
                    if (speed - SpeedChange > -MaxSpeed)
                        speed -= SpeedChange;
                }
                else
                {
                    if (speed + SpeedChange < MaxSpeed)
                        speed += SpeedChange;
                }
                velocity = new Vector3((float)Math.Cos(rotation) * speed, (float)Math.Sin(rotation) * speed, 0);
                pos += velocity;
            }
            else
                velocity = new Vector3((float)Math.Cos(rotation) * speed, (float)Math.Sin(rotation) * speed, 0); //keep updatubg velocity to see where the bounding sphere would be and if it would no longer be colliding
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, bodyTransform * Camera.Transform);
            spriteBatch.Draw(texture, Vector2.Zero, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 1f);
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
        public BoundingSphere BS { get { return new BoundingSphere(pos, texture.Width / 2); } }

        public bool Alive { get { return alive; } set { alive = value; } }
        //public BoundingSphere PlayerSphere { get; set; }
        public List<Bullet> Bullets { get { return bullets; } }
        public int BulletDamage { get { return bulletDamage; } }
        public Texture2D Texture { get { return texture; } }
        public Vector3 Position { get { return pos; } }
        public int ScoreAmount { get { return scoreAmount; } }
        public BoundingSphere NextBS { get { return new BoundingSphere(pos + velocity * 4, texture.Width / 2); } } //the positon of the bouding sphere in the next frame
    }
}
