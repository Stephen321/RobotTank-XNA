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
    class Sprite
    {
        int curFrame; //current frame of the animation
        Rectangle sourceRect; //rectangle of the sprite on the spritesheet
        Vector2 startLocation; //position of the sprite on the screen
        Texture2D spriteSheet;
        bool alive; //if the explosion is alive and should be drawn
        int timer; //timer for how often to change frame
        int frameWidth;//frame dimensions
        int frameHeight;
        int totalCols;
        int totalRows;
        bool playOnce;
        float rotation;
        int timePerFrame;

        public Sprite(Texture2D _spriteSheet, Vector2 location, float rotation, int frameWidth, int frameHeight, bool playOnce, int timePerFrame)
        {
            //set values
            this.rotation = rotation;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.startLocation = location;
            this.timePerFrame = timePerFrame;
            alive = true;
            spriteSheet = _spriteSheet;
            curFrame = 0;

            totalCols = spriteSheet.Width / frameWidth;
            totalRows = spriteSheet.Height / frameHeight;
            this.playOnce = playOnce;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (alive)
            {
                UpdateFrames(gameTime);
                spriteBatch.Draw(spriteSheet, startLocation, sourceRect, Color.White, rotation, new Vector2(frameWidth / 2, frameHeight / 2), 1f, SpriteEffects.None, 1f);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 pos)
        { //continuely update where this is drawn
            if (alive)
            {
                UpdateFrames(gameTime);
                spriteBatch.Draw(spriteSheet, pos, sourceRect, Color.White, rotation, new Vector2(frameWidth / 2, frameHeight / 2), 1f, SpriteEffects.None, 1f);
            }
        }

        private void UpdateFrames(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds;
            if (timer > timePerFrame) //every this many milliseconds
            {
                curFrame++; //change frame
            }

            int row;
            int col;
            row = curFrame / totalCols; //gets the row for the current frame
            col = curFrame % totalRows; //gets the col for the current frame

            //uses these to get the rectangle of the curFrame on the sprite sheet
            sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);

            if (curFrame == totalRows * totalCols)
            {
                if (playOnce)
                    alive = false;  //finished playing
                curFrame = 0;
                timer = 0;
            }
        }

        public bool Alive
        {
            get { return alive; }
        }
        
        public Vector2 Position
        {
            get { return startLocation; }
        }

        public int CurrentFrame
        {
            get { return curFrame; }
        }
    }  
}
