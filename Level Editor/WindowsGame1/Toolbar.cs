using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace LevelEditor
{
    public class Toolbar
    {
        private int width;
        private int height;
        //public LinkedList<Tool> elements;
        private Texture2D[] textures;
        public Vector2 position;
        private int tileWidth;
        private int numTools;

        public Toolbar(Texture2D[] textures, Vector2 size, Vector2 position)
        {

            this.width = (int)size.X; this.height = (int)size.Y;
            this.position = position;
            this.textures = textures;
            numTools = textures.Length;
            tileWidth = (int)size.X / numTools;

        }

        public void Draw(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < numTools; x++)
            {

                Rectangle dest = new Rectangle((int)position.X + x * tileWidth, (int)position.Y, tileWidth, height);
                spriteBatch.Draw(textures[x], dest, Color.AliceBlue);

            }
        }

        public int parseClick(MouseState ms, int currentTool)
        {

            if (ms.Y > position.Y + height) return currentTool;

            Vector2 temp = new Vector2();
            int relx = (int)(ms.X - position.X);
            temp.X = (relx - (relx % tileWidth)) / tileWidth;

            if (temp.X > numTools) return currentTool;

            return (int)temp.X;
        }
    }
}
