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
    class Map
    {
        //sound beep
        Texture2D map, dot;
        float scale;
        int width, height;
        Rectangle locationRect;
        Vector2 mapTopLeft;

        public Map(int screenWidth, int screenHeight, int worldWidth, int worldHeight, float scale)
        {
            this.scale = scale;
            width = (int)(worldWidth * scale);
            height = (int)(worldHeight * scale);
            locationRect = new Rectangle(screenWidth - width, screenHeight - height, width, height);
            mapTopLeft = new Vector2(locationRect.X, locationRect.Y);
        }

        public void LoadContent(ContentManager theCM, string mapName, string dotName)
        {
            this.map = theCM.Load<Texture2D>(mapName);
            this.dot = theCM.Load<Texture2D>(dotName);
        }

        public void Draw(SpriteBatch sB, List<Vector2> enemyPositions,List<Vector2> advancedEnemiesPos, List<Vector2> soldiersPos,  Vector2 playerPos)
        {
            sB.Draw(map, locationRect, Color.White);
            sB.Draw(dot, mapTopLeft + playerPos * scale, Color.Green);
            foreach (Vector2 pos in enemyPositions)
                sB.Draw(dot, mapTopLeft + pos * scale, Color.DarkGoldenrod);
            foreach (Vector2 pos in advancedEnemiesPos)
                sB.Draw(dot, mapTopLeft + pos * scale, Color.Red);
            foreach (Vector2 pos in soldiersPos)
                sB.Draw(dot, mapTopLeft + pos * scale, Color.Purple);
        }
    }
}
