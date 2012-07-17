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
    public class Hook : Sprite
    {
        private const int HOOK_MAX_SPEED = 7000;
        public const float HOOK_MAX_LENGTH = Level.ScreenW / 2;

        public Boolean isHooked = false;
        public const float SCALE = Player.PLAYERSCALE + .05f;
        public const int GRAPPLESPEED = 700;
        public Boolean draw = false;

        public float length;
        public Beam beam;

        //this creates a basic hook with no position
        //assign it the player's position?
        public Hook()
            : base()
        {
            this.beam = new Beam();
            this.maxSpeed = HOOK_MAX_SPEED;
            this.terminalVelocity = HOOK_MAX_SPEED;
            this.Scale = SCALE;
            this.length = HOOK_MAX_LENGTH;
        }


        //if we want to draw the hook, draw it!
        public void Draw(SpriteBatch spriteBatch, Vector2 level_origin, Vector2 playerPosition)
        {
            if (this.draw)
            {
                this.Scale = SCALE;
                //sets hook / beam rotation
                Vector2 meToHook = this.position - playerPosition;
                Vector2 normalMeToHookForReal = Vector2.Normalize(meToHook);
                if (playerPosition.X < this.position.X)
                {
                    this.rotation = (float)Math.Acos(Vector2.Dot(normalMeToHookForReal, Vector2.Normalize(new Vector2(position.X, 0) - new Vector2(position.X, Level.ScreenH))));
                }
                else
                {
                    this.rotation = -1f * (float)Math.Acos(Vector2.Dot(normalMeToHookForReal, Vector2.Normalize(new Vector2(position.X, 0) - new Vector2(position.X, Level.ScreenH))));
                }

                beam.rotation = this.rotation;
                beam.position = (this.position + playerPosition) / 2;
                beam.DrawScaled(spriteBatch, level_origin, new Vector2(Beam.ROPE_WIDTH_SCALE, (playerPosition - this.position).Length() / beam.getHeight()));
                base.Draw(spriteBatch, level_origin);
            }
        }


    }
}
