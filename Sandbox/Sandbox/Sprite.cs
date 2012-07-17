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
    public class Sprite
    {
        public Vector2 position;
        public Vector2 velocity;
        //public BoundingBox bound;
        public Color tint = Color.White;
        public float rotation = 0f;
        public float maxSpeed;
        public float terminalVelocity;
        private const float TERMINAL_VELOCITY = 500f;

        public bool onGround = false;
        public bool collideRight = false;
        public bool collideLeft = false;
        public bool collideUp = false;



        private float scale = 1.0f;
        public Texture2D spriteTexture;
        private Rectangle source;
        protected float height;
        protected float width;

        public BoundingBox bound;

        public Vector2 origin = Vector2.Zero;

        public virtual void resetRopeLength()
        {

        }

        /// <summary>
        /// checks collisions between this sprite and some other sprite, hopefully.
        /// </summary>
        /// <param name="other">the sprite that this may or may not be colliding with.</param>
        /// <returns>true if they are colliding, false otehrwise</returns>
        public bool collides(Sprite other)
        {
            return this.bound.Intersects(other.bound);
        }
   

        /// <summary>
        /// non-basic constructor.
        /// </summary>
        /// <param name="velocity">initial velocity</param>
        /// <param name="position">initial position</param>
        public Sprite(Vector2 velocity, Vector2 position)
        {
            this.velocity = velocity;
            this.position = position;
            this.maxSpeed = 0f;
            this.terminalVelocity = TERMINAL_VELOCITY;
            this.bound = new BoundingBox(new Vector3(position.X - this.width / 2, position.Y - this.height / 2, -1), new Vector3(position.X + this.width / 2, position.Y + this.height / 2, 1));
            origin = Vector2.Zero;
        }

        /// <summary>
        /// constructor on a single variable
        /// </summary>
        /// <param name="position">the position the sprite should have</param>
        public Sprite(Vector2 position) : this(new Vector2(), position)
        { }

        /// <summary>
        /// basic constructor
        /// </summary>
        public Sprite()
            : this(new Vector2(), new Vector2())
        { }
 
        /// <summary>
        /// gets the texture used to create the sprite
        /// </summary>
        public Texture2D Texture
        {
            get { return spriteTexture; }
        }

        /// <summary>
        /// mutator method for scale value, both gets and sets
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                height = source.Height * scale;
                width = source.Width * scale;
            }
        }

        /// <summary>
        /// gets the effective size of the sprite
        /// </summary>
        /// <returns>the largest of height and width</returns>
        public float size()
        {
            return Math.Max(this.height, this.width);
        }


        /// <summary>
        /// For reading from a sprite-sheet.
        /// </summary>
        public Rectangle Source
        {
            get { return source; }
            set
            {
                source = value;
                height = source.Height * scale;
                width = source.Width * scale;
            }
        }



        /// <summary>
        /// Height and Width are read only, so that they can't be changed outside the class.
        /// they are calculated from the Source rectangle's width and height and the scale factor
        /// </summary>
        public float getHeight()
        {
            return height;
        }


        public void setHeight(float h)
        {
            height = h;
        }


        /// <summary>
        /// Height and Width are read only, so that they can't be changed outside the class.
        /// they are calculated from the Source rectangle's width and height and the scale factor
        /// </summary>
        public float getWidth()
        {
            return width;
        }


        public void setWidth(float w)
        {
            width = w;
        }

        /// <summary>
        /// Loads the given asset as a Texture2D using the contentManager.
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="assetName"></param>
        public void LoadContent(ContentManager contentManager, string assetName)
        {
            spriteTexture = contentManager.Load<Texture2D>(assetName);
            Source = new Rectangle(0, 0, spriteTexture.Width, spriteTexture.Height);
            origin = new Vector2(spriteTexture.Width/2, spriteTexture.Height/2);
        }


        /// <summary>
        /// Draws the sprite with the given spriteBatch.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void 
            Draw(SpriteBatch spriteBatch, Vector2 Origin)
        {
            spriteBatch.Draw(spriteTexture, position - Origin, Source, tint,rotation, origin, Scale,
                SpriteEffects.None, 0);
        }

        public virtual void DrawScaled(SpriteBatch spriteBatch, Vector2 Origin, Vector2 scale)
        {
            spriteBatch.Draw(spriteTexture, position - Origin, Source, tint, rotation, origin, scale,
    SpriteEffects.None, 0);
            }

        protected virtual void collisionLeft() {
            if (collideLeft)
            {
                if (velocity.X < 0)
                {
                    velocity.X = 0f;
                }
            }
        }
        protected virtual void collisionRight() {
            if (collideRight)
            {
                if (velocity.X > 0)
                {
                    velocity.X = 0;
                }
            }
        }
        protected virtual void collisionDown() {
            if (onGround)
            {
                if (velocity.Y > 0)
                {
                    velocity.Y = -.5f * velocity.Y;
                }
            }
        }
        protected virtual void collisionUp()
        {
            if (collideUp)
            {
                if (velocity.Y < 0)
                {
                    velocity.Y = 0;
                }
            }
        }


        protected virtual void checkMax() {
                        if (Math.Abs(velocity.X) > maxSpeed)
            {
                velocity.X = maxSpeed * (velocity.X / Math.Abs(velocity.X));
            }
            if (Math.Abs(velocity.Y) > terminalVelocity)
            {
                velocity.Y = terminalVelocity * (velocity.Y / Math.Abs(velocity.Y));
            }
        }

        /// <summary>
        /// Handles basic movement updating.
        /// </summary>
        /// <param name="dt">time elapsed</param>
        public void Update(float dt)
        {
            collisionLeft();
            collisionRight();
            collisionDown();
            collisionUp();
           
            checkMax();

            if (float.IsNaN(this.velocity.X) || float.IsNaN(this.velocity.Y)) this.velocity = Vector2.Zero;
            this.position += this.velocity * dt;
            this.bound = new BoundingBox(new Vector3(position.X - this.width / 2, position.Y - this.height / 2, -1), new Vector3(position.X + this.width / 2, position.Y + this.height / 2, 1));
        }
    }
}
