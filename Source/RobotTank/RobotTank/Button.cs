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
    class Button
    {
        Texture2D texture;
        Vector2 position;
        string text;
        SpriteFont font;
        Vector2 textOffset;
        int width;
        int height;
        bool clicked;

        public Button()
        {
            position = Vector2.Zero;
            text = "";
            textOffset = new Vector2(35, 15);
            clicked = false;
        }

        public void LoadContent(Texture2D texture, SpriteFont font)
        {
            this.texture = texture;
            this.font = font;
            width = texture.Width;
            height = texture.Height;
        }

        public void Draw(SpriteBatch sB)
        {
            sB.Draw(texture, position, Color.White);
            sB.DrawString(font, text, position + new Vector2(texture.Width / 2 - textOffset.X, texture.Height / 2 - textOffset.Y), Color.Black);
        }
        public void Draw(SpriteBatch sB, float opacity)
        {
            sB.Draw(texture, position, Color.White * opacity);
            sB.DrawString(font, text, position + new Vector2(texture.Width / 2 - textOffset.X, texture.Height / 2 - textOffset.Y), Color.Black);
        }
        public void CheckClicked(MouseState previousMouseState)
        { //check if the player has clicked the area of the screen of where this button is
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, width, height);
            if (rect.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)) &&
                previousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released)
            {
                // if (playSounds)
                //  click.Play(); //this will play the click sound 
                clicked = true;
            }
            else
                clicked = false;
        }
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public bool Clicked
        {
            get { return clicked; }
        }
    }
}
