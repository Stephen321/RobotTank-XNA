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
    class Scorekeeper
    //Contains data and methods for health, lives and score.
    {

        int score;

        Texture2D highScoreTexture; //texture for the background of the highscore table
        Rectangle highScoreRect; //position , width , height of the highscore table
        Texture2D scoreboard; //texture for the scoreboard at the top of the screen which has the players health and score
        SpriteFont font; //to draw score and highscores
        SpriteFont bigFont;
        const int MaxHighScores = 5;
        int[] highScores = new int[MaxHighScores];
        string[] highScoreNames = new string[MaxHighScores];
        int[] highScoreWaves = new int[MaxHighScores];
        int screenWidth;
        const int ScoreboardHeight = 60;
        HealthBar playerHealthBar;
        const int PlayerMaxHealth = 100;
        const int LifeImageOffset = 80;
        const int LifeImageWidth = 30;
        const int LifeImageHeight = 50;
        const int LifesSeperator = 10;

        const int TotalPlayerLives = 3;
        Texture2D lifeImage;
        int playerLives;
        const int EnemiesKilledLifeUp = 12;
        int enemiesKilled;
        const int RegenRate = 3;


        public Scorekeeper(int screenWidth)
        {//set initial values
            score = 0;
            enemiesKilled = 0;
            highScoreRect = new Rectangle(55, 420, 350, 150);
            this.screenWidth = screenWidth;
            playerHealthBar = new HealthBar(350, 10, Color.Green, Color.Red, Color.Orange, PlayerMaxHealth, 0, 300);
            playerLives = TotalPlayerLives;
        }

        public void LoadContent(Texture2D _scoreboard, SpriteFont theFont, SpriteFont theFont2, Texture2D highscoreT, Texture2D healthBG, Texture2D currentHealth, Texture2D lifeImage)
        { //load in textures and fonts
            highScoreTexture = highscoreT;
            font = theFont;
            bigFont = theFont2;
            scoreboard = _scoreboard;
            playerHealthBar.LoadContent(healthBG, currentHealth, theFont);
            this.lifeImage = lifeImage;
        }

        public void ChangeHealth(int amount)
        {
            playerHealthBar.ChangeHealth(amount);
        }
        public void UpdateHealth(GameTime gameTime)
        {
            playerHealthBar.Update(gameTime);
        }

        public void ResetHealth()
        {
            playerHealthBar.ResetHealth();
        }
        public bool CheckPlayerAlive()
        {
            if (playerHealthBar.Health <= 0)
                return false;
            else
                return true;

        }

        public void UpdateHighScores(string playerName, int wave)
        {//update the highscores with the new players name and their score

            //need another array that is one big and then sort that, last one is dropped from the table. Otherwise this score might not be higher than the current lowest
            int[] temp = new int[MaxHighScores + 1]; //create a temp array 1 bigger than the highscores array
            string[] stringTemp = new string[MaxHighScores + 1]; //create one for the string array aswell
            int[] tempWaves = new int[MaxHighScores + 1]; //temp array for wave amount

            highScores.CopyTo(temp, 0); //copy over the highscores arrays
            highScoreNames.CopyTo(stringTemp, 0);
            highScoreWaves.CopyTo(tempWaves, 0);
            temp[temp.Length - 1] = score; //last element of the temporary array is set to the new score
            stringTemp[stringTemp.Length - 1] = playerName; //last element of temp string array is set to new name 
            tempWaves[tempWaves.Length - 1] = wave;
            Sort(temp, stringTemp, tempWaves); //sort both arrays in descending order

            for (int i = 0; i < MaxHighScores; i++) //last element of the sorted array will be lost
            {
                highScoreNames[i] = stringTemp[i]; //put all the values in the temp arrays back into the normal arrays
                highScores[i] = temp[i];
                highScoreWaves[i] = tempWaves[i];
            }
        }

        private void Sort(int[] array, string[] stringArray, int[] wavesArray)
        { //sort the array with the biggest value first
            int n = array.Length; //the amount of numbers
            int maxElem; //current maximum element of the pass

            for (int pass = 0; pass < n; pass++)
            {
                maxElem = pass;
                for (int currentElem = pass + 1; currentElem < n; currentElem++)
                {
                    if (array[maxElem] <= array[currentElem])
                        maxElem = currentElem; //get the max element of this pass
                }
                //swap the values in the int array and also the relevant string in the string array, and the wave 
                int temp = array[pass];
                string stringTemp = stringArray[pass];
                int wavesTemp = wavesArray[pass];
                array[pass] = array[maxElem];
                stringArray[pass] = stringArray[maxElem];
                wavesArray[pass] = wavesArray[maxElem];
                array[maxElem] = temp;
                stringArray[maxElem] = stringTemp;
                wavesArray[maxElem] = wavesTemp;
            }          
        }

        public void WriteXML(XmlTextWriter outFile)
        { //write the highscors out to a xml file
            outFile.WriteStartElement("HighScoreTable");

            for (int i = 0; i < MaxHighScores; i++)
            {
                outFile.WriteStartElement("HighScore");

                outFile.WriteStartElement("name");
                outFile.WriteString(highScoreNames[i]);
                outFile.WriteEndElement();

                outFile.WriteStartElement("score");
                outFile.WriteString(highScores[i].ToString());
                outFile.WriteEndElement();


                outFile.WriteStartElement("wave");
                outFile.WriteString(highScoreWaves[i].ToString());
                outFile.WriteEndElement();

                outFile.WriteEndElement();
            }
            outFile.WriteEndElement();
        }

        public void ReadXML(XmlTextReader inFile)
        { //read in the highscores from the xml file
            string elementName;
            int curHighScore = -1; //which highscore to read into

            while (inFile.Read())
            {
                elementName = "";
                XmlNodeType nType = inFile.NodeType;
                if (nType == XmlNodeType.Element) //if the node is an element node
                {
                    elementName = inFile.Name.ToString();
                }
                if (elementName == "HighScore")
                    curHighScore++;
                else if (elementName == "name")
                    highScoreNames[curHighScore] = inFile.ReadString();
                else if (elementName == "score")
                    highScores[curHighScore] = Convert.ToInt32(inFile.ReadString());
                else if (elementName == "wave")
                    highScoreWaves[curHighScore] = Convert.ToInt32(inFile.ReadString());
            }

        }
        public void ChangeScore(int scoreChange)
        //Increase or decrease the score by the amount passed to it
        {
            score += scoreChange;
        }

        public void Reset()
        //Reset back to defaults
        {
            ResetHealth();
            playerLives = TotalPlayerLives;
            score = 0;
        }

        public void EnableRegen()
        {
            playerHealthBar.RegenRate = RegenRate;
        }
        public void DisableRegen()
        {
            playerHealthBar.RegenRate = 0;
        }

        public void ResetHighScores()
        {//reset highscores back to default
            for (int i = 0; i < MaxHighScores; i++)
            {
                highScores[i] = 0;
                highScoreNames[i] = " ";
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        //Draw the health and score of the player
        {
            spriteBatch.Draw(scoreboard, new Rectangle(0, 0, screenWidth, ScoreboardHeight), Color.White);
            spriteBatch.DrawString(font, "Score: " + String.Format("{0:D4}", score), new Vector2(screenWidth - 250, 5), Color.White);
            spriteBatch.DrawString(font, "Enemies Killed: " + String.Format("{0:D3}", enemiesKilled), new Vector2(screenWidth - 285, 25), Color.White);
            playerHealthBar.Draw(spriteBatch);

            int xOffset = LifeImageOffset;
            for(int i = 0; i < playerLives; i++)
            {
                spriteBatch.Draw(lifeImage, new Rectangle(xOffset, 5, LifeImageWidth, LifeImageHeight), Color.White);
                xOffset += LifeImageWidth + LifesSeperator;
            }
            spriteBatch.DrawString(font, "Lives: ", new Vector2(10, 15), Color.White);
            if (playerLives == 0)
                spriteBatch.DrawString(bigFont, "YOU ARE DEAD!", new Vector2(LifeImageOffset, 10), Color.White);
        }

        public void DrawHighScores(SpriteBatch spriteBatch)
        {//draw the highscores to the screen
            Vector2 highscorePos = new Vector2(highScoreRect.X + 20, highScoreRect.Y + 15); // position is slightly within the rectangle for the texture
            spriteBatch.Draw(highScoreTexture, highScoreRect, Color.White * 0.8f);
            spriteBatch.DrawString(bigFont, "HighScore Table: ", highscorePos, Color.SteelBlue);
            for (int i = 0; i < MaxHighScores; i++)
            {
                highscorePos.Y += 20; //increase the pos to move the next highscore down
                spriteBatch.DrawString(font, (i + 1).ToString() + ". " + highScoreNames[i] + ": " + highScores[i] + " Wave: " + highScoreWaves[i], highscorePos, Color.SteelBlue);
            }

        }

        public int Score
        //Property to get the score
        {
            get { return score; }
            set { score = value; }
        }
        public int Lives
        //Property to get the lives
        {
            get { return playerLives; }
            set { playerLives = value; }
        }

        public int EnemiesKilled
        {
            get { return enemiesKilled; }
            set 
            {
                 enemiesKilled = value;
                 if (enemiesKilled % EnemiesKilledLifeUp == 0 && playerLives < TotalPlayerLives)
                 {
                     playerLives++;
                     DisplayLifeUpMessage = true;
                 }
            }
        }

        public bool DisplayLifeUpMessage { get; set; } //cant be in this class as it must be timed and this isnt updated every frame
    }
}
