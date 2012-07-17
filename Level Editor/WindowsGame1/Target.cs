using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LevelEditor
{
    public class Target
    {

        public bool isDeleted;

        public int x;
        public int y;
        public int type;

        public Target(int x, int y, int type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }

        public Target(Vector3 vec)
        {
            x = (int)vec.X;
            y = (int)vec.Y;
            type = (int)vec.Z;
        }

    }
}
