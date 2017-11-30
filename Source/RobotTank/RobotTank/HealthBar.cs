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
using System.Xml;
using System.IO;

namespace RobotTank
{
    class HealthBar
    //Contains data and methods for energy
    {
        int health;
        int droppingHealth;
        int maxHealth;
        Texture2D healthBackground; //texture for the bar behind the green->red bar of the players health
        Texture2D currentHealthTex; //texture for the green->red bar of the players health
        Texture2D droppingBarTex; //texture for the bar that decreases slower behind on the bar
        //position and size of the health bar
        Rectangle healthRect;
        Rectangle backgroundHealthRect;
        Rectangle droppingBarRect;
        //colors of the bars
        Color curHealthBarColor;
        Color healthStartBarColor;
        Color healthBarEndColor;
        Color droppingBarColor;
        SpriteFont font; //to draw energy left , 2 here because 1 for char name
        int healthRegen;
        int timer; //timer for 1second to regen health by healthRegen amount
        int droppingTimer; //timer for backdrop colour
        int x, y;
        int barMaxLength;

        public HealthBar(int x, int y, Color startColor, Color endColor, Color droppingColor, int _maxHealth, int startHealthRegen, int barMaxLength)
        {//set initial values
            maxHealth = _maxHealth;
            this.barMaxLength = barMaxLength;
            health = maxHealth;
            droppingHealth = maxHealth;
            this.x = x;
            this.y = y;
            backgroundHealthRect = new Rectangle(x, y, barMaxLength + 4, 28);
            healthRect = new Rectangle(x + 2, y + 2, barMaxLength, 24);
            droppingBarRect = healthRect;
            healthStartBarColor = startColor;
            healthBarEndColor = endColor;
            curHealthBarColor = healthStartBarColor;
            droppingBarColor = droppingColor;
            healthRegen = startHealthRegen; //regen this much energy a second
            timer = 0;
            droppingTimer = 0; 
        }

        public void LoadContent(Texture2D _healthbarbackgroundTex, Texture2D _currentHealthTex, SpriteFont font)
        { //load in textures and fonts
            this.font = font;
            currentHealthTex = _currentHealthTex; //load this in game and past texture in instead of loading for every bar
            healthBackground = _healthbarbackgroundTex;
            droppingBarTex = currentHealthTex;
        }


        private void GetHealthBarLength()
        //Get the new length of the health bar
        {
            healthRect.Width = (int)((barMaxLength) * ((float)health / maxHealth)); //Get a percentage of the players remaining health and multiply this by the
            //length of the healthbar to make the texture representing health smaller
            //e.g if currentHealth/maxHealth is 20/100. Then 250 * 0.2 is 50. The green rectangle is now 50 

            droppingBarRect.Width = (int)((barMaxLength) * ((float)droppingHealth / maxHealth));
            curHealthBarColor = Color.Lerp(healthBarEndColor, healthStartBarColor, (float)health * 2 / maxHealth);
        }

        public void Update(GameTime gameTime)
        {
            Regen(gameTime);
            UpdateDroppingHealth(gameTime);
        }

        public bool Update(GameTime gameTime, Vector2 pos)
        { //enemy health doesnt regen and follows enemy pos
            bool alive = true;
            backgroundHealthRect.X  = (int)pos.X - 50;
            backgroundHealthRect.Y = (int)pos.Y - 85;
            healthRect.X = (int)pos.X - 48;
            healthRect.Y = (int)pos.Y - 83;
            droppingBarRect.X = healthRect.X;
            droppingBarRect.Y = healthRect.Y;
            UpdateDroppingHealth(gameTime);
            if (health <= 0)
                alive = false;
            return alive;
        }

        private void UpdateDroppingHealth(GameTime gameTime)
        {//update the health that drops behind the main health bar
            droppingTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (droppingTimer > 2 / (droppingHealth - health + 1)) //every 10ms , plus 1 so not to divide by 0
            {
                if (health < droppingHealth)
                {
                    droppingHealth--;
                    GetHealthBarLength();
                }
                droppingTimer = 0;
            }
        }
        private void Regen(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds;
            if (health > 0)
            {
                if (timer > 1000) //every second
                {
                    if (health + healthRegen <= maxHealth)
                    {
                        health += healthRegen;
                        if (droppingHealth < health)
                            droppingHealth = health;
                        GetHealthBarLength();
                    }
                    else
                    {
                        health = maxHealth;
                        droppingHealth = health;
                    }

                    timer = 0;
                }
            }
        }

        public void ChangeHealth(int amount)
        //To decrease the players health by the amount passed to it and may decrease lives. Check if they have been killed and restart or exit.
        {
            if (amount < 0) //if amount is negative
            {
                if (health + amount > 0) //If the damage dealt will not kill the player by putting their health 0 or lower 
                {
                    health += amount;
                }
                else
                { //player is dead
                    health = 0;
                }
            }
            else //if positive
            {
                if (health + amount > maxHealth) //if it would increase the players health above the max then set it to the max
                {
                    health = maxHealth;
                    droppingHealth = health;
                }
                else
                {
                    health += amount;
                    droppingHealth = health;
                }
            }
            GetHealthBarLength();
        }

        public void ResetHealth()
        {
            health = maxHealth;
            GetHealthBarLength();
        }

        public void Draw(SpriteBatch spriteBatch) 
        //Draw the health and score of the player
        {
            //camera?
            spriteBatch.Draw(healthBackground, backgroundHealthRect, Color.White);
            spriteBatch.Draw(droppingBarTex, droppingBarRect, droppingBarColor);
            spriteBatch.Draw(currentHealthTex, healthRect, curHealthBarColor);
            spriteBatch.DrawString(font, "Armour " + health+ "/" + maxHealth, 
                           new Vector2(backgroundHealthRect.X +  (healthBackground.Width / 6), backgroundHealthRect.Y + 2), Color.White);
        }

        public int Health
        //Property to get the health amount
        {
            get { return health; }
            set { health = value; }
        }

        public int RegenRate { set { healthRegen = value;  } } 
    }
}
