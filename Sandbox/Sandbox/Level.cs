using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace MoonPrison
{
    public class Level
    {
        public Player player;
        public Tile[,] layout;
        public Crosshair crosshair;

        public const int tileHeight = (int) (ScreenH / 16);
        public const int tileWidth = (int)(ScreenW / 21);
        public Texture2D[] textures = new Texture2D[7];

        //this should be the filename that was used to load the level - used for saving / loading
        //this is just the name of the level file WITHOUT .txt. or .xml extensions
        //(e.g. test)
        public String levelName;

        public LinkedList<Enemy> enemies;
        private Enemy contentHolder;
        public Bullet[] bullets;
        public LinkedList<Vector2> usedCoins;
        public Dictionary<Vector2, int> usedButtons;

        public int levelH;
        public int levelW;

        public const int ScreenH = 768;
        public const int ScreenW = 1024;
        const float scrollAmmount = 100;

        //keeps track of whether or not a sound has been made
        public Boolean noise;
        public Boolean buttonNoise;
        public Boolean gameOver;
        public Boolean levelCompleted;
        private Boolean inEasterMode;

        public Vector2 origin = new Vector2(0, 0);
        private Vector2 minVis = new Vector2();
        private Vector2 maxVis = new Vector2();

        private Boolean eastermode;
        public const int NUM_BUTTON_TEXTURES = 4;
        private Texture2D[] buttonTextures = new Texture2D[NUM_BUTTON_TEXTURES];
        
        //Background stuff
        private Texture2D background;
        private const int BG_TILE_HEIGHT = 600;
        private const int BG_TILE_WIDTH = 600;

        private PipeEngine pipeEngine;

        public Level()
        {
            gameOver = false;
            levelCompleted = false;
            eastermode = false;
            //hooray for things being in the right place!
            this.player = new Player();
            crosshair = new Crosshair();
            enemies = new LinkedList<Enemy>();
            contentHolder = new Enemy(-1, -1);
            bullets = new Bullet[50];
            usedCoins = new LinkedList<Vector2>();
            usedButtons = new Dictionary<Vector2, int>();
            pipeEngine = new PipeEngine(128, 128, 3000, 0f);

            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i] = new Bullet(Vector2.Zero);
            }
        }


        public void LoadContent(ContentManager Content)
        {
            inEasterMode = false;

            Particle_Engine.ParticleEngine.clearEmitters();
            pipeEngine.LoadContent(Content);

            textures[Tile.RED] = Content.Load<Texture2D>(@"Sprites/hookable");
            textures[Tile.GREEN] = Content.Load<Texture2D>(@"Sprites/plainTileBlue");
            textures[Tile.BLUE] = Content.Load<Texture2D>(@"Sprites/danger");
            textures[Tile.ORANGE] = Content.Load<Texture2D>(@"Sprites/Purple_Tile");
            textures[Tile.COIN] = Content.Load<Texture2D>(@"Sprites/coin2-3-2");
            textures[Tile.PORTAL] = Content.Load<Texture2D>(@"Sprites/portal");

            player.LoadContent(Content, @"Sprites/player1");
            //Get the animation loop set up
            Texture2D[] playerAnimation = new Texture2D[9];
            for (int i = 0; i < 5; i++)
            {
                playerAnimation[i] = Content.Load<Texture2D>(@"Sprites/player" + i);
            }
            playerAnimation[5] = playerAnimation[3];
            playerAnimation[6] = playerAnimation[4];
            playerAnimation[7] = playerAnimation[3];
            playerAnimation[3] = playerAnimation[1];
            playerAnimation[4] = playerAnimation[0];
            playerAnimation[8] = Content.Load<Texture2D>(@"Sprites/player-jump");


            player.textures = playerAnimation;

            background = Content.Load<Texture2D>(@"Sprites/background");

            player.theHook.LoadContent(Content, @"Sprites/hook");
            player.theHook.beam.LoadContent(Content, @"Sprites/beam");
            crosshair.LoadContent(Content, @"Sprites/crosshair");

            foreach (Enemy e in enemies)
            {
                e.LoadContent(Content, @"Sprites/enemy");
                e.exlcamationPoint.LoadContent(Content, @"Sprites/exclamation");
                e.lazerBeam.LoadContent(Content, @"Sprites/lazerbeam");
                e.questionMark.LoadContent(Content, @"Sprites/question");
            }

            contentHolder.LoadContent(Content, @"Sprites/enemy");
            contentHolder.exlcamationPoint.LoadContent(Content, @"Sprites/exclamation");
            contentHolder.lazerBeam.LoadContent(Content, @"Sprites/lazerbeam");

            buttonTextures[3] = Content.Load<Texture2D>(@"Sprites/greenOff");
            buttonTextures[0] = Content.Load<Texture2D>(@"Sprites/greenOn");
            buttonTextures[1] = Content.Load<Texture2D>(@"Sprites/redOn");
            buttonTextures[2] = Content.Load<Texture2D>(@"Sprites/blueOn");

            foreach (Bullet bull in bullets)
            {
                bull.LoadContent(Content,@"Sprites/Bullet");
            }

        }

        // Used in easter-egging.  Loads the old content.
        public void LoadContent_OLD(ContentManager Content)
        {
            inEasterMode = true;

            textures[Tile.RED] = Content.Load<Texture2D>(@"Sprites/redsquare");
            textures[Tile.GREEN] = Content.Load<Texture2D>(@"Sprites/greensquare");
            textures[Tile.BLUE] = Content.Load<Texture2D>(@"Sprites/bluesquare");
            textures[Tile.ORANGE] = Content.Load<Texture2D>(@"Sprites/orangesquare");
            textures[Tile.COIN] = Content.Load<Texture2D>(@"Sprites/hook");
            textures[Tile.PORTAL] = Content.Load<Texture2D>(@"Sprites/lazer");

            player.LoadContent(Content, @"Sprites/player_old");

            Texture2D[] temp = new Texture2D[1];
            temp[0] = Content.Load<Texture2D>(@"Sprites/player_old");
            player.textures = temp;
            player.Scale = 2.5f;

            player.theHook.LoadContent(Content, @"Sprites/coin");
            player.theHook.beam.LoadContent(Content, @"Sprites/portal");
            crosshair.LoadContent(Content, @"Sprites/bullet");

            foreach (Enemy e in enemies)
            {
                e.LoadContent(Content, @"Sprites/enemy1");
                e.exlcamationPoint.LoadContent(Content, @"Sprites/exclamation");
                e.lazerBeam.LoadContent(Content, @"Sprites/lazerbeam");
            }

            foreach (Bullet bull in bullets)
            {
                bull.LoadContent(Content, @"Sprites/crosshair");
            }
            eastermode = true;

        }

        private void setLeft(Sprite s, float location)
        {
            s.position.X = location + s.getWidth() / 2;
            s.resetRopeLength();
        }
        private void setRight(Sprite s, float location)
        {
            s.position.X = location - s.getWidth() / 2;
            s.resetRopeLength();
        }
        private void setTop(Sprite s, float location)
        {
            s.position.Y = location + s.getHeight() / 2;
            s.resetRopeLength();
        }
        private void setBottom(Sprite s, float location)
        {
            s.position.Y = location - s.getHeight() / 2;
            s.resetRopeLength();
        }
        private float getLeft(Sprite s) { return getLeft(s, 1.0f); }
        private float getRight(Sprite s) { return getRight(s, 1.0f); }
        private float getTop(Sprite s) { return getTop(s, 1.0f); }
        private float getBottom(Sprite s) { return getBottom(s, 1.0f); }

        private float getLeft(Sprite s,float degree){ return s.position.X - degree*s.getWidth() / 2; }
        private float getRight(Sprite s,float degree) { return s.position.X + degree*s.getWidth() / 2; }
        private float getTop(Sprite s,float degree) { return s.position.Y - degree*s.getHeight() / 2; }
        private float getBottom(Sprite s,float degree) { return s.position.Y + degree*s.getHeight() / 2; }

        private void resolveSolidCollisions(Sprite tile, Sprite actor)
        {

            //Note -
            // I would like to change this to only trigger onGround collisions and collideUp
            // collisions if the player is within the middle 1/3 of the player. likewise,
            // side collisions should only work for the middle 1/3 of the side of the player.
            // This should make collisions with walls work better while not breaking general collisions.
            // however, the flip side is that corners would be slightly awkward
            
            
            /*tile.position.X - tile.getWidth() / 2 < player.position.X + player.getWidth() / 6.0f &&
            tile.position.X + tile.getWidth()/2 > player.position.X - player.getWidth() / 6.0f &&
            tile.position.Y - tile.getHeight()/2 < player.position.Y + player.getWidth()/ 6.0f &&
            tile.position.Y + tile.getHeight() / 2 > player.position.Y - player.getWidth() / 6.0f*/

            float ACTOR_BOTTOM = getBottom(actor);
            float ACTOR_TOP = getTop(actor);
            float TILE_BOTTOM = getBottom(tile);
            float TILE_TOP = getTop(tile);

            if (ACTOR_BOTTOM < TILE_BOTTOM &&
                //Middle third
                getLeft(tile) < getRight(actor, 1 / 3.0f) &&
                getRight(tile) > getLeft(actor, 1 / 3.0f))
            {
                actor.onGround = true;
                if (getBottom(actor) > getTop(tile)) setBottom(actor, getTop(tile)); 
            }
            else if (ACTOR_TOP > TILE_TOP &&
                        getLeft(tile) < getRight(actor, 1 / 3.0f) &&
                        getRight(tile) > getLeft(actor, 1 / 3.0f))
            {
                actor.collideUp = true;
                if (getTop(actor) < getBottom(tile)) setTop(actor, getBottom(tile));
            }
            else if (getRight(actor) < getRight(tile) &&
                getTop(tile) < getBottom(actor, 1 / 3.0f) &&
                getBottom(tile) > getTop(actor, 1 / 3.0f))
            {
                actor.collideRight = true;
                if (getRight(actor) > getLeft(tile)) setRight(actor, getLeft(tile));
            }
            else if (getLeft(actor) > getLeft(tile)&&
                getTop(tile) < getBottom(actor, 1 / 3.0f) &&
                getBottom(tile) > getTop(actor, 1 / 3.0f))
            {
                actor.collideLeft = true;
                if (getLeft(actor) < getRight(tile)) setLeft(actor, getRight(tile));
            }
        }

        private void checkCollisions()
        {
            ArrayList nearPlayer = getNearestTiles2(player.position);
            
            player.onGround = false;
            player.collideRight = false;
            player.collideLeft = false;
            player.collideUp = false;

            noise = false;

            foreach (Tile tile in nearPlayer)
            {
                if (player.collides(tile) && !player.isDead)
                {
                    if (tile.isDeath &&
                        tile.position.X - tile.getWidth() / 2 < player.position.X + player.getWidth() / 3.0f &&
                        tile.position.X + tile.getWidth()/2 > player.position.X - player.getWidth() / 3.0f &&
                        tile.position.Y - tile.getHeight()/2 < player.position.Y + player.getWidth()/ 3.0f &&
                        tile.position.Y + tile.getHeight() / 2 > player.position.Y - player.getWidth() / 3.0f
                        )
                    {
                        player.isDead = true;
                        gameOver = true;
                        Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.ExplosionEmitter(player.position));
                        MoonPrison.displayMessage("You Lose!");
                    }
                    if (tile.isPortal)
                    {
                        levelCompleted = true;
                    }
                    if (tile.isMotionSensor)
                    {
                        Button b = (Button)tile;
                        useButton(b);
                    }
                    if (tile.isCoin &&
                        tile.position.X - tile.getWidth() / 2 < player.position.X + player.getWidth() / 2.0f &&
                        tile.position.X + tile.getWidth() / 2 > player.position.X - player.getWidth() / 2.0f &&
                        tile.position.Y - tile.getHeight() / 2 < player.position.Y + player.getWidth() / 2.0f &&
                        tile.position.Y + tile.getHeight() / 2 > player.position.Y - player.getWidth() / 2.0f)
                    {
                        player.numCoins++;
                        tile.assignBooleans(0);
                        usedCoins.AddLast(new Vector2(tile.position.X / tileWidth - .5f, tile.position.Y / tileHeight - .5f));
                    }
                    if (tile.isSolid)
                    {
                        resolveSolidCollisions(tile, player);
                    }
                }
            } //end tile to player collisions


            //NEAR THE HOOK
            ArrayList nearHook = getNearestTiles2(player.theHook.position);

            foreach (Tile tile in nearHook)
            {
                if (player.theHook.isHooked == false && player.theHook.collides(tile) && player.hookExtended == true)
                {
                    if (tile.isButton && ! tile.isMotionSensor)
                    {
                        Button button = (Button)tile;
                        useButton(button);
                        player.stopHook();
                        Vector2 key = new Vector2(tile.position.X / tileWidth -.5f, tile.position.Y / tileHeight -.5f);
                        if (usedButtons.ContainsKey(key))
                        {
                            usedButtons[key]++;
                        }
                        else
                        {
                            usedButtons.Add(key, 1);
                        }
                    }

                    //SET THE PLAYER TO START SWINGING
                    if (tile.isHookable )
                    {
                        player.theHook.position = tile.position;
                        if (tile.isLoud)
                        {
                            noise = true;
                        }

                        //Particles!
                        Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.SparksEmitter(player.theHook.position));    

                        player.theHook.isHooked = true;
                        player.theHook.velocity = Vector2.Zero;

                        //The player is swinging
                        {
                            Vector2 meToHook = player.theHook.position - player.position;
                            Vector3 newDir3 = Vector3.Cross(new Vector3(meToHook, 0), new Vector3(0, 0, 1));
                            if (newDir3 != Vector3.Zero) newDir3.Normalize();
                            float velLength = player.velocity.Length();
                            Vector2 newDir2 = new Vector2(newDir3.X, newDir3.Y);
                            player.velocity = Vector2.Dot(newDir2, player.velocity) * newDir2;
                            if (player.velocity != Vector2.Zero) player.velocity.Normalize();
                            player.velocity *= velLength;
                        }
                        player.swinging = true;

                        player.theHook.length = (player.position - player.theHook.position).Length();
                    }
                    if (tile.isSolid)
                    {
                            //if tile is loud, play sound, but only once... this relies on loud object also being solid
                            if (tile.isLoud && player.theHook.velocity != Vector2.Zero)
                            {
                                noise = true;
                            }

                            if (!tile.isHookable)
                            {
                                player.stopHook();
                            }
                    }
                }
            }
            foreach (Enemy e in enemies)
            {
                e.onGround = false;
                e.collideLeft = false;
                e.collideRight = false;
                e.collideUp = false;

                ArrayList nearEnemy = getNearestTiles2(e.position);
                foreach (Tile tile in nearEnemy)
                {
                    if (e.collides(tile) && tile.isSolid && !e.isDead)
                    {
                        resolveSolidCollisions(tile, e);
                    }
                }
                if (player.collides(e) || e.shouldFire && !player.isDead)
                {
                    player.isDead = true;
                    gameOver = true;
                    Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.ExplosionEmitter(player.position));
                    MoonPrison.displayMessage("You Lose!");
                }
            }
          
                    foreach (Bullet b in bullets)
                    {
                        ArrayList nearBullet = getNearestTiles2(b.position);
                        
                        if (b.isVisible)
                        {
                            foreach (Tile tile in nearBullet)
                            {
                                if (tile.isSolid && tile.collides(b))
                                {
                                    b.isVisible = false;
                                }
                            }
                            if (b.collides(player) && !player.isDead)
                            {
                                player.isDead = true;
                                gameOver = true;
                                Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.ExplosionEmitter(player.position));
                                MoonPrison.displayMessage("You Lose!");
                            }
                        }
                    }
        }

        //make the button do something
        public void useButton(Button button)
        {
            //change each tile to its new type
                string action = button.getAction();
                if (action == "enemy")
                {
                    buttonNoise = true;
                    enemies.AddLast(FileIOManager.createEnemyWithParams(button.enemyParams));
                    Enemy e = enemies.Last.Value;
                    e.spriteTexture = contentHolder.Texture;
                    e.exlcamationPoint = contentHolder.exlcamationPoint;
                    e.lazerBeam = contentHolder.lazerBeam;
                    e.Source = new Rectangle(0, 0, e.spriteTexture.Width, e.spriteTexture.Height);
                    e.origin = new Vector2(e.spriteTexture.Width / 2, e.spriteTexture.Height / 2);
                }
                else if (action == "changeTiles")
                {
                    buttonNoise = true;
                    foreach (Vector3 point in button.getTileArrayForUse())
                    {
                        layout[(int)point.X, (int)point.Y].assignBooleans((int)point.Z);
                    }
                }
                else if (action == "displayMessage")
                {
                    MoonPrison.displayMessage(button.messages.First.Value);
                    if (button.cycles) {
                        button.messages.AddLast(button.messages.First.Value);
                    }
                    button.messages.RemoveFirst();
                }
        }

        /// <summary>
        /// Gets the 9 tiles that are hopefully nearest to the given position.  The actual indices ARE IMPORTANT - don't change them without good reason
        /// </summary>
        /// <param name="entityPosition">The position of the object to get tiles nearby to</param>
        /// <returns></returns>

        public Tile[] getNearbyTiles(Vector2 entityPosition)
        {
            int nearestX = (int) (entityPosition.X-tileWidth/2) / tileWidth;
            int nearestY = (int) (entityPosition.Y-tileHeight/2) / tileHeight;

            Tile[] temp;
            if (nearestX > 0 && nearestY > 0)
            {
                temp = new Tile[9];
                temp[0] = layout[nearestX, nearestY];
                temp[1] = (layout[nearestX, nearestY - 1]);
                temp[2] = (layout[nearestX, nearestY + 1]);
                temp[3] = (layout[nearestX - 1, nearestY]);
                temp[4] = (layout[nearestX - 1, nearestY - 1]);
                temp[5] = (layout[nearestX - 1, nearestY + 1]);
                temp[6] = (layout[nearestX + 1, nearestY]);
                temp[7] = (layout[nearestX + 1, nearestY - 1]);
                temp[8] = (layout[nearestX + 1, nearestY + 1]);
            }          
            //things with negative positions are actually outside of the level, and shouldn't collide with anything
            else
            {
                temp = new Tile[1];
                temp[0] = layout[0, 0];
            }
            return temp;
        }

        ArrayList getNearestTiles2(Vector2 pos)
        {
            int cX = (int)pos.X / tileWidth;
            int cY = (int)pos.Y / tileHeight;
            ArrayList rtn = new ArrayList();
            for (int a = -1; a <= 1; a++)
            {
                for (int b = -1; b <= 1; b++)
                {
                    if (cX + a >= 0 && cX + a < levelW &&
                         cY + b >= 0 && cY + b < levelH)
                    {
                        rtn.Add(layout[cX+a,cY+b]);
                    }
                }
            }
            return rtn;
        }

        //main game update method, does input handling
        public int[] Update(float dt, MouseState ms, KeyboardState ks, bool isActive)
        {
            //Add dt to the rotation for tiles.
            Tile.tilerotation += dt;
            int[] retval = new int[6];
            int temp2 = player.numCoins;
            Boolean fallNoise = false;
            buttonNoise = false;
            if (!player.isDead)
            {
               fallNoise = player.Update(dt, ks, ms, origin, isActive, crosshair.position);
            }
            crosshair.Update(ms);
            int delta = 4 * tileWidth;
            foreach (Enemy e in enemies)
            {
                if (!e.isDead && e.position.X >= minVis.X - delta && e.position.Y >= minVis.Y - delta && e.position.X <= maxVis.X + delta && e.position.Y <= maxVis.Y + delta)
                {
                    int temp = e.Update(dt, player, bullets, layout, noise, fallNoise, getNearbyTiles(e.position));
                    if (temp >= retval[2]) retval[2] = temp;
                }
            }

            foreach (Bullet bull in bullets)
            {
                if (bull.isVisible)
                {
                    bull.Update(dt);
                }
            }
            float tempX = player.position.X - ScreenW / 2;
            float tempY = player.position.Y - ScreenH / 2;
            if (ks.IsKeyDown(Keys.Right))
            {

                if (origin.X + scrollAmmount >= tempX)
                {
                    origin.X += 3;
                }
            }
            if (ks.IsKeyDown(Keys.Left))
            {
                if (origin.X - scrollAmmount <= tempX)
                {
                    origin.X -= 3;
                }
            }
            if (ks.IsKeyDown(Keys.Up))
            {
                if (origin.Y + scrollAmmount > tempY)
                {
                    origin.Y -= 3;
                }
            }
            if (ks.IsKeyDown(Keys.Down))
            {
                if (origin.Y - scrollAmmount < tempY)
                {
                    origin.Y += 3;
                }
            }

            updateOrigin(player.position);
            checkCollisions();

            if (noise) retval[0] = 1;
            if (buttonNoise) retval[1] = 1;
            if (fallNoise) retval[3] =1;
            if (player.isDead) retval[4] = 1;
            if (player.numCoins > temp2) retval[5] = 1;
            return retval;
        }



        //draw the entities!
        public void Draw(SpriteBatch spriteBatch)
        {

            
            if (!inEasterMode)
            {
                for (int x = -((int)(.8*origin.X) % BG_TILE_WIDTH); x < ScreenW; x = x + BG_TILE_WIDTH)
                {
                    for (int y = -((int)(.8*origin.Y) % BG_TILE_HEIGHT); y < ScreenH; y = y + BG_TILE_HEIGHT)
                    {
                        Rectangle dest = new Rectangle(x, y, BG_TILE_WIDTH, BG_TILE_HEIGHT);
                        spriteBatch.Draw(background, dest, Color.AliceBlue);
                    }
                }
                pipeEngine.Draw(spriteBatch, origin);
            }

            Vector2 temp = new Vector2();
            temp.X = (origin.X - (origin.X % tileWidth)) / tileWidth; temp.Y = (origin.Y - (origin.Y % tileHeight)) / tileHeight;
            minVis = temp * tileWidth;
            maxVis = minVis + new Vector2(ScreenW, ScreenH);
            Particle_Engine.LazerBlockEmitter.numberOfEmittersDisplayed = 0;
            for (int x = (int)temp.X; x <= temp.X + 64 && x < levelW; x++)
            {
                for (int y = (int) temp.Y; y <= temp.Y + 64 && y < levelH; y++)
                {
                    
                    if (layout[x, y].type != Tile.BLANK && layout[x,y].type != 7 && (!(layout[x,y].type == Tile.ORANGE && !eastermode)))
                    {
                        Rectangle dest = new Rectangle(x * tileWidth - (int)origin.X, y * tileHeight - (int)origin.Y, tileWidth, tileHeight);
                        Rectangle destDeath = new Rectangle((int)((x + 0.5) * tileWidth - origin.X), (int)((y+0.5) * tileHeight - (int)origin.Y), tileWidth, tileHeight);
                        if (layout[x, y].isDeath)
                            spriteBatch.Draw(textures[layout[x, y].type], destDeath, null, Color.AliceBlue, Tile.tilerotation, new Vector2(textures[layout[x,y].type].Width/2, textures[layout[x,y].type].Height/2), SpriteEffects.None, 0);
                        else
                            spriteBatch.Draw(textures[layout[x, y].type], dest, Color.White);
                    }
                    if (layout[x, y].isDeath )
                    {
                        //Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.LazerBlockEmitter(layout[x, y].position, tileWidth, tileHeight));
                    }
                    if (layout[x, y].isChangedbyButton || layout[x,y].type == 7)
                    {
                        Particle_Engine.ParticleEngine.addEmitter(new Particle_Engine.ChangingBlockEmitter(layout[x, y].position, tileWidth, tileHeight));
                    }
                    if (layout[x, y].type == Tile.ORANGE && !eastermode)
                    {
                        Rectangle dest = new Rectangle(x * tileWidth - (int)origin.X, y * tileHeight - (int)origin.Y, tileWidth, tileHeight);
                        spriteBatch.Draw(buttonTextures[layout[x,y].getTextureNum()], dest, Color.AliceBlue);
                    }
                }
            }
            Particle_Engine.ParticleEngine.Draw(spriteBatch, this.origin);
            if (!player.isDead)
            {
                player.theHook.Draw(spriteBatch, origin, player.position);
                player.Draw(spriteBatch, origin);
                crosshair.Draw(spriteBatch, Vector2.Zero);
            }

            foreach (Bullet bull in bullets)
            {
                if (bull.isVisible)
                {
                    bull.Draw(spriteBatch, origin);
                }
            }

            foreach (Enemy e in enemies)
            {
                if (!e.isDead)
                    e.Draw(spriteBatch, origin, player);
            }
        }


        //Updates the center of the screen to be where the player is.
        private void updateOrigin(Vector2 position)
        {

            float tempX = position.X - ScreenW / 2;
            if (origin.X != tempX)
            {
                if (origin.X < tempX)
                {
                    //We need to scroll right
                    if (origin.X + scrollAmmount > tempX)
                    {
                    }
                    else
                    {
                        origin.X += tempX - origin.X - scrollAmmount;
                    }
                }
                else
                {
                    //we need to scroll left.
                    if (origin.X - scrollAmmount < tempX)
                    {
                    }
                    else
                    {
                        origin.X -= origin.X - scrollAmmount -tempX;
                    }
                }
            }

            if (origin.X < 0) origin.X = 0;
  
            if (origin.X > levelW * tileWidth - ScreenW) 
                origin.X = levelW * Level.tileWidth - ScreenW;

            float tempY = position.Y - ScreenH / 2;
            if (origin.Y != tempY)
            {
                if (origin.Y < tempY)
                {
                    //We need to scroll up
                    if (origin.Y + scrollAmmount > tempY)
                    {
                    }
                    else
                    {
                        origin.Y += tempY - origin.Y - scrollAmmount;
                    }
                }
                else
                {
                    //we need to scroll down.
                    if (origin.Y - scrollAmmount < tempY)
                    {
                    }
                    else
                    {
                        origin.Y -= origin.Y - scrollAmmount - tempY;
                    }
                }
            }
            if (origin.Y < 0) origin.Y = 0;
            if (origin.Y > levelH * tileHeight - ScreenH)
                origin.Y = levelH * Level.tileHeight - ScreenH;
        }
    }
}
