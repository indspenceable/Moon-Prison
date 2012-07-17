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

namespace LevelEditor
{
    public class Use
    {

        public LinkedList<Target> targets;

        public Use(LinkedList<Vector3> targets)
        {
            this.targets = new LinkedList<Target>();

            foreach (Vector3 v in targets)
            {
                addTarget(new Target(v));
            }
        }

        public Use() {
            targets = new LinkedList<Target>();
        }

        public void addTarget(Target t)
        {
            targets.AddFirst(t);
        }

        public void addTarget(int x, int y, int type)
        {
            bool alreadyThere = false;

            foreach (Target t in targets)
            {
                if (t.x == x && t.y == y)
                {
                    t.type = type;
                    alreadyThere = true;
                }
            }

            if(!alreadyThere) targets.AddFirst(new Target(x,y,type));
        }

        public void deleteTarget(int x, int y)
        {
            foreach (Target t in targets)
            {
                if (t.x == x && t.y == y)
                {
                    targets.Remove(t);
                    return;
                }
            }
        }

    }
}
