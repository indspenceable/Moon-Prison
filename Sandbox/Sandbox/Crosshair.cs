using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoonPrison
{
    public class Crosshair : Sprite
    {
        public float SCALE = .30f;

        public Crosshair()
        {
            this.Scale = SCALE;
        }

        public void Update(MouseState ms)
        {
            //set the crosshair to be where the mouse is.
            this.position.X = ms.X;
            this.position.Y = ms.Y;
            this.bound = new BoundingBox(new Vector3(position.X - this.width / 2, position.Y - this.height / 2, -1), new Vector3(position.X + this.width / 2, position.Y + this.height / 2, 1));
        }
    }
}
