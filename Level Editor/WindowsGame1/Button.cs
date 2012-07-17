using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace LevelEditor
{
    public class Button
    {
        public bool isDeleted;

        public LinkedList<Use> uses;

        public int currentUse = 0;

        public int x; public int y;

        public Button(int x, int y)
        {
            this.x = x;
            this.y = y;
            uses = new LinkedList<Use>();
            //uses.AddLast(new Use());
        }

        public void nextUse()
        {
            if (uses.Count != 0)
            currentUse = (currentUse + 1) % uses.Count;
        }

        public Use getCurrentUse()
        {
            if (uses.Count == 0) addUse();
            return uses.ElementAt<Use>(currentUse);
        }

        public void addUse()
        {
            uses.AddLast(new Use());
        }

        public void addUse(LinkedList<Vector3> list)
        {
            uses.AddLast(new Use(list));
        }
        

    }
}
