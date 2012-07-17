using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace MoonPrison
{
    public class Tile : Sprite
    {
        public Boolean isHookable;
        public Boolean isSolid;
        public Boolean isLoud;
        public Boolean isButton;
        public Boolean isDeath;
        public Boolean isCoin;
        public Boolean isPortal;
        public Boolean isMotionSensor;
        public Boolean isChangedbyButton;
        public int type;

        public static float tilerotation = 0;

        //Color constants slash tile type constants.
        public const int GREEN = 1;
        public const int RED = 2;
        public const int BLUE = 3;
        public const int ORANGE = 4;
        public const int COIN = 5;
        public const int PORTAL = 6;
        public const int BLANK = 0;

        /// <summary>
        /// Creates a new tile of the specified type.
        /// </summary>
        public Tile(int type)
        {
            assignBooleans(type);
        }



        //this method decides what boolean values correspond to an integer type.
        public void assignBooleans(int type) {
            this.type = type;
            isHookable = false;
            isSolid = false;
            isLoud = false;
            isDeath = false;
            isCoin = false;
            isPortal = false;
            isMotionSensor = false;
            if (type == 1) //green / solid
            {
                this.isSolid = true;
            }
            else if (type == 2) //red / hookable
            {
                this.isHookable = true;
                this.isSolid = true;
                this.isLoud = true;
            } 
            else if (type == 3) //blue / death
            {
                this.isDeath = true;
            }
            else if (type == 4) //type 4 is button
            {
                this.isButton = true;
                this.isSolid = true;
                this.isLoud = true;
            }
            else if (type == 5) //type 5 is coin
            {
                this.isCoin = true;
            }
            else if (type == 6) //6 is for end-level portals
            {
                this.isPortal = true;
            }
            else if (type == 7) //7 is for motion sensor's
            {
                this.isButton = true;
                this.isMotionSensor = true;
            }
        }

        public void updateBound()
        {
            this.bound = new BoundingBox(new Vector3(position.X - this.width / 2, position.Y - this.height / 2, -1), new Vector3(position.X + this.width / 2, position.Y + this.height / 2, 1));
        }

        public virtual int getTextureNum()
        {
            return 1;
        }
    }
}
