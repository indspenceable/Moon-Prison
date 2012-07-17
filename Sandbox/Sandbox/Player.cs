using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoonPrison
{
    public class Player : Sprite
    {
        private const int DELTA_V = 110;
        private const float DELTA_AIR = 14f;

        public bool hookExtended;
        public bool swinging;
        public Boolean isDead;

        public Hook theHook;
        public int numCoins;

        public int cooldown = 0;
        public int cooldown2 = 0;

        private Boolean facingRight = true;

        private float falltime = 0;

        public Texture2D[] textures;
        public int animationIndex = 0;
        public float animationTimer;
        public const float ANIMATION_DELAY = .15f; //Milliseconds
        public bool walking = false;



        //assumes square tiles and players.  makes the player 1/2 of the size of a tile
        public const float PLAYERSCALE = (float) Level.tileHeight / 200;

        public const int JUMP_AMOUNT = 293; //max before jumping into death
        public const float FALLTIME = 1f;

        bool MOUSEDOWN;
        private Boolean spacedown;

        KeyboardState keyboard;

        /*
         * Constructor. Initializes the variables, and nothing else.
         */
        public Player()
        {
            theHook = new Hook();
            onGround = false;
            hookExtended = false;
            swinging = false;
            this.maxSpeed = 400f;
            this.terminalVelocity = 700f;
            isDead = false;
            this.Scale = PLAYERSCALE;
            this.numCoins = 0;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 Origin)
        {
            spriteTexture = textures[animationIndex];
            Source = new Rectangle(0, 0, spriteTexture.Width, spriteTexture.Height);
            origin = new Vector2(spriteTexture.Width / 2, spriteTexture.Height / 2);
            if (facingRight)
            {
                base.Draw(spriteBatch, Origin);
            }
            else
            {
                spriteBatch.Draw(spriteTexture, position - Origin, Source, tint, rotation, origin, this.Scale,
                    SpriteEffects.FlipHorizontally, 0);
            }
        }

        protected override void collisionLeft()
        {
            if (swinging)
            {
                if (collideLeft && velocity.X < 0)
                {
                    velocity.X *= -.7f;
                }
            }
            else
            {
                base.collisionLeft();
            }
        }
        protected override void collisionRight()
        {
            if (swinging)
            {
                if (collideRight && velocity.X > 0)
                {
                    velocity.X *= -.7f;
                }
            }
            else
            {
                base.collisionRight();
            }
        }
        protected override void collisionUp()
        {
            if (swinging)
            {
                if (collideUp && collideLeft == collideRight)
                {
                    if (velocity.Y < 0)
                    {
                        velocity = new Vector2(0, velocity.Length());
                    }
                    theHook.length = (theHook.position - this.position).Length() + 2;
                }
            }
            else
            {
                base.collisionUp();
            }
        }
        



        //Changes the velocity based on a number of factors, then tells
        //Sprite to change the actual position.
        public Boolean Update(float dt, KeyboardState ks, MouseState ms, Vector2 origin, bool isActive, Vector2 crossPos)
        {
            parseInput(ks, ms, origin, isActive, crossPos);
            theHook.Update(dt);
            theHook.bound = new BoundingBox(new Vector3(theHook.position.X - theHook.getWidth() / 3, theHook.position.Y - theHook.getHeight() / 3, -1), new Vector3(theHook.position.X + theHook.getWidth() / 3, theHook.position.Y + theHook.getHeight() / 3, 1));
            slowDown();
            checkHookDistance();

            animationTimer -= dt;
            if (Math.Abs(velocity.X) > .1 && onGround)
            {
                walking = true;
            }
            else if (onGround)
            {
                animationIndex = 0;
                walking = false;
            }
            else
            {
                walking = false;
                animationIndex = textures.Length - 1;
            }

            if (walking && animationTimer <= 0)
            {
                animationIndex = (animationIndex + 1) % Math.Max((textures.Length - 1),1);
                animationTimer = ANIMATION_DELAY;
            }
            


            keyboard = ks;

            //swing if and only if the hook is hooked
            if (theHook.isHooked)
            {
                //Will call apply gravity from within swing
                swing(dt);
            }
            else
            {
                applyGravity();
            }
            if (velocity.X < -.1f) facingRight = false;
            if (velocity.X > .1f) facingRight = true;

            float tempfall = 0;
            if (onGround || velocity.Y < 0 || swinging)
            {
                tempfall = falltime;
                falltime = 0;
            } 
            if (!onGround && velocity.Y != 0 && !swinging) falltime += dt;

            base.Update(dt);

            return (tempfall > FALLTIME);
        }
        /*
         * moves left or right if the key is down to do so
         */
        private void parseInput(KeyboardState ks, MouseState ms, Vector2 origin, bool isActive, Vector2 crossPos)
        {
            //Deal with input
            //Only when not swinging
            if (onGround && !swinging)
            {
                if (ks.IsKeyDown(Keys.D))
                {
                    //Change position to the right!
                    velocity.X += DELTA_V;
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    //slide to the left!
                    velocity.X -= DELTA_V;
                }
            }
            else
            {
                if (!swinging)
                {
                    if (ks.IsKeyDown(Keys.D))
                    {
                        //Change position to the right!
                        velocity.X += DELTA_V;
                    }
                    if (ks.IsKeyDown(Keys.A))
                    {
                        //slide to the left!
                        velocity.X -= DELTA_V;
                    }
                }
                else
                {
                    //Get the direction to the rope.
                    
                    if (ks.IsKeyDown(Keys.D) && (theHook.position-position).X > 0)
                    {
                        //Change position to the right!
                        velocity.X += DELTA_AIR/2;
                    }
                    if (ks.IsKeyDown(Keys.A) && (theHook.position - position).X < 0)
                    {
                        //slide to the left!
                        velocity.X -= DELTA_AIR/2;
                    }
                }
            }

            if ((ks.IsKeyDown(Keys.Space)) && (onGround || swinging) && !spacedown)
            {
                if (swinging)
                {
                    swinging = false;
                    stopHook();
                }
                spacedown = true;
                velocity.Y = -JUMP_AMOUNT;
                onGround = false;
            }
            if (ks.IsKeyUp(Keys.Space)) spacedown = false;
            if (ms.RightButton == ButtonState.Pressed && theHook.isHooked)
            {
                stopHook();
            }

            if (ks.IsKeyDown(Keys.S))
            {
                theHook.length += 2;
                if (theHook.length > Hook.HOOK_MAX_LENGTH)
                {
                    theHook.length = Hook.HOOK_MAX_LENGTH;
                }
            }
            if (ks.IsKeyDown(Keys.W))
            {
                Vector2 toHook = theHook.position - position;
                float theta = (float)(180 * Math.Acos(toHook.X/toHook.Length()) / Math.PI);
                bool noCollision = true ;
                if (theta >= 90 - 50 && theta < 90 + 50)
                    noCollision = !collideUp && noCollision;
                if (theta >= 180 - 50 && theta < 180 + 50)
                    noCollision = !collideLeft && noCollision;
                if (theta >= 270 - 50 && theta < 270 + 50)
                    noCollision = !onGround && noCollision;
                if (theta >= 270 + 40 || theta < 90-40)
                    noCollision = !collideRight && noCollision;
                
                if (noCollision && theHook.length > 2 )
                {
                    theHook.length -= 2;
                }
            }
            if (onGround && !ks.IsKeyDown(Keys.W)) 
            {
                theHook.length = (this.position - theHook.position).Length();
            }

            if (ms.LeftButton == ButtonState.Pressed) {
                if (cooldown == 0 && isActive && MOUSEDOWN == false)
                {
                    MOUSEDOWN = true;

                    cooldown = 10;

                    Vector2 grappleDirection = new Vector2();
                    grappleDirection = crossPos - (this.position - origin);
                    grappleDirection.Normalize();
                    theHook.position = this.position;
                    theHook.velocity = Vector2.Zero;
                    theHook.velocity = grappleDirection * Hook.GRAPPLESPEED;
                    theHook.draw = true;
                    hookExtended = true;
                    theHook.isHooked = false;
                    onGround = false;
                }
            }else {
                MOUSEDOWN = false;
            }
            if (cooldown > 0) cooldown--;
            if (cooldown2 > 0) cooldown2--;

        }

        /*
         * Stops the hook from moving or appearing.  This seemed like something we were doing a lot.
         */
        public void stopHook()
        {
            theHook.draw = false;
            hookExtended = false;
            theHook.isHooked = false;
            swinging = false;
            theHook.velocity = Vector2.Zero;
        }

        public override void resetRopeLength()
        {
            if (this.hookExtended && (theHook.position - position).Length() - theHook.length > 1.0f)
            {
                theHook.length = (theHook.position - position).Length();
            }
        }

        /*
         * Applies the gravity, if not resting on the ground
         */
        private void applyGravity()
        {
            if (!onGround)
            {
                velocity.Y += 11;
            }
            else
            {
                velocity.Y = 0;
            }
        }
        /*
         * Makes sure the player doesn't move too fast
         */
        private void slowDown()
        {
            if (!swinging || (swinging && onGround)) velocity.X *= 0.67f;
            //else if (!swinging) velocity.X *= .95f;
        }

        /*
         * Make sure the hook hasn't one farther than its max length - that is no grappling out of range 
         */
        private void checkHookDistance()
        {
            if (hookExtended)
            {
                Vector2 distance = this.position - this.theHook.position;
                if (distance.Length() > Hook.HOOK_MAX_LENGTH)
                {
                    stopHook();
                }
            }
        }


        /*
         * If the hook is extended, moves the player toward the hook.
         * However, if the player is already at the hook, tells the hook that it
         * is not extended and doesn't move.
         */
        private void swing(float dt)
        {
            Vector2 meToHook = theHook.position - this.position;
            //Dannytempt
            //First, set the distance
            if (meToHook.Length() >= theHook.length)
            {
                //fix the position 
                Vector2 meToHookNormal = new Vector2(meToHook.X, meToHook.Y);
                meToHookNormal.Normalize();
                meToHookNormal = meToHook - theHook.length * meToHookNormal;               
                this.position += meToHookNormal;

                //Apply gravity
                float velLength = velocity.Length();
                applyGravity();
                if (meToHook.Y < 0)
                {
                    //Get the direction vector on which I should be going.
                    Vector3 newDir3 = Vector3.Cross(new Vector3(meToHook, 0), new Vector3(0, 0, 1));
                    if (newDir3 != Vector3.Zero) newDir3.Normalize();
                    Vector2 newDir2 = new Vector2(newDir3.X, newDir3.Y);
                    velocity = Vector2.Dot(newDir2, velocity) * newDir2;
                 }
            }
            else
            {
                applyGravity();
            }
            //for collisions to work... maybe
            this.bound = new BoundingBox(new Vector3(position.X-this.width/2, position.Y-this.height/2, -1), new Vector3(position.X + this.width/2, position.Y + this.height/2, 1));
        }

    }
}
