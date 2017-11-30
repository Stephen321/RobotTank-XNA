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
    class AdvancedEnemy : EnemyTank
    {

        public AdvancedEnemy(int _WorldWidth, int _WorldHeight, Vector3 playerPos, Texture2D _texture, Texture2D _turretTexture, Texture2D bulletSpriteSheet)
            : base(_WorldWidth, _WorldHeight, playerPos, _texture, _turretTexture, bulletSpriteSheet)
        {
            regenRate = 3;
            speedChange = 0.1f;
            fireRate = 600;
            maxHealth = 200;
            maxSpeed = 5f;
            bulletDamage = 3;
            scoreAmount = 50;
            ClosestDistance = 140;
            reverse = Convert.ToBoolean(rnd.Next(2));
            turretOrigin = new Vector2(25, 24);
            healthBar = new HealthBar((int)pos.X - 50, (int)pos.Y - 30, Color.Green, Color.Red, Color.Orange, maxHealth, regenRate, HealthBarLength);
        }

        protected override void Rotate(Vector3 playerPos, float playerAngle)
        {
            Vector2 targetDir = new Vector2(playerPos.X - pos.X, playerPos.Y - pos.Y);
            if (speed > 0)
                rotation = (float)(Math.Atan2(targetDir.Y, targetDir.X) - (Math.PI / 180) * 86); //will circle the player and gradually move closer if circles in forward
            else
                rotation = (float)(Math.Atan2(targetDir.Y, targetDir.X) - (Math.PI / 180) * 94); //will circle the player and gradually move closer if circles in reverse
            turretAngle = (float)(Math.Atan2(targetDir.Y, targetDir.X)) - rotation;
        }

        protected override void OnScreenUpdate(GameTime gameTime, Vector3 playerPos, float playerAngle, bool playSounds)
        {
            Rotate(playerPos, playerAngle);     
            Fire(gameTime, playSounds);
            Move(playerPos);
        }
    }
}
