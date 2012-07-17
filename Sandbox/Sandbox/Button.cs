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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MoonPrison
{
    public class Button : Tile
    {
        public Vector3[][] tiles;
        public Dictionary<string, string> enemyParams;
        public LinkedList<string> messages;
        public Boolean isMultipleUse;
        public Boolean isActivated;
        public Boolean cycles;
        public int totalUses;
        //these are the actions that button performs on use index
        public string[] useActions;

        private int useCount;

        public Button () :base(4)
        {
            this.tiles = new Vector3[0][];
            useActions = new string[0];
            enemyParams = new Dictionary<string,string>();
            this.isActivated = false;
            this.isMultipleUse = false;
            this.cycles = false;
            this.totalUses = 1;
            this.useCount = -1;
        }

        public override int getTextureNum()
        {
            if (useCount == -1)
            {
                return  Level.NUM_BUTTON_TEXTURES - 1;
            }
            else
            {
                return (useCount % (totalUses)) % (Level.NUM_BUTTON_TEXTURES - 1);
            }
        }

        public Vector3[] getTileArrayForUse()
        {
            return tiles[useCount];
        }

        public string getAction()
        {

            useCount++;
            if (cycles)
            {
                useCount = useCount % totalUses;
            }
            if (totalUses >= 1 && useCount < totalUses)
            {
                return useActions[useCount];
            }
            else
            {
                useCount--;
                return "null";
            }
        }

        
    }
}
