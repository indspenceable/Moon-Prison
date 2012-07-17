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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MoonPrison : Microsoft.Xna.Framework.Game
    {
        public string[] LEVEL_ORDER = new string[14] { "newtutorial","level1","level2", "level3","levelq", "level4", "level6", "level13","level5", "level10", "level14", "level11", "leveld","credits" };
        private string[] MUSIC_CUE_NAMES = new string[3] { "Bass", "Freaky", "Intergalactic Jello" };
        private SoundEffect[] FX = new SoundEffect[7];
        private SoundEffectInstance lazer;
        public int levelNumber = 0;
        private const int BackBufferWidth = Level.ScreenW;
        private const int BackBufferHeight = Level.ScreenH;
        //the number of loops that any message atring will be displayed at the top of the screen
        public const int STRING_DISPLAY_TIME = 400;
        //The number of new lines allowed in a message
        public const int MAX_MESSAGE_LINES = 5;     
        //num pixels to allow on both sides (combined) of a message before starting a new line
        public const int MESSAGE_BORDER = 250;
        private const int EASTER_EGG_THRESHOLD = 10;
        private Boolean easter = false;
        public Boolean hasEaster = false;

        private Boolean isKeyUp = true;

        private static LinkedList<Message> messages;

        /*Game variables*/
        public Level level;
        public Boolean linearOrder;
        public static SpriteFont font;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        ShellMenu menu;
        AudioEngine engine;
        SoundBank soundBank;
        WaveBank waveBank;
        Cue cue;

        public MoonPrison()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.IsFullScreen = true;
        }

        //DONT TOUCH!  needed to draw lines
        private void initDrawStuff()
        {
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, (float)GraphicsDevice.Viewport.Width, (float)GraphicsDevice.Viewport.Height, 0, 1.0f, 1000.0f);
            basicEffect = new BasicEffect(GraphicsDevice, null);
            basicEffect.VertexColorEnabled = true;
            Matrix worldMatrix = Matrix.CreateTranslation(0, 0, 0);
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //set window size, variables all declared above
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;
            graphics.ApplyChanges();

            initDrawStuff();
            menu = new ShellMenu();
            messages = new LinkedList<Message>();

            engine = new AudioEngine("..\\..\\..\\Content\\Sound\\Win\\Music.xgs");
            soundBank = new SoundBank(engine, "..\\..\\..\\Content\\Sound\\Win\\Sound Bank.xsb");
            waveBank = new WaveBank(engine, "..\\..\\..\\Content\\Sound\\Win\\Wave Bank.xwb");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            FX[0] = Content.Load<SoundEffect>(@"Sound/Clank1");
            FX[1] = Content.Load<SoundEffect>(@"Sound/ButtonBeep2");
            FX[2] = Content.Load<SoundEffect>(@"Sound/Gun");
            FX[3] = Content.Load<SoundEffect>(@"Sound/grunt2");
            FX[6] = Content.Load<SoundEffect>(@"Sound/Laser3");
            FX[4] = Content.Load<SoundEffect>(@"Sound/deathnoise2");
            FX[5] = Content.Load<SoundEffect>(@"Sound/Ding");
            lazer = FX[6].CreateInstance();

            font = Content.Load<SpriteFont>(@"Spritefont1");
            
            menu.font = font;
            menu.LoadContent(Content);
            Particle_Engine.ParticleEngine.loadContent(Content);
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //get the new state of the game
            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            removeOldMessages();

            if ((ks.IsKeyDown(Keys.Escape) && isKeyUp))
            {
                menu.isDisplayed = !menu.isDisplayed;
                isKeyUp = false;
            }
            if (ks.IsKeyUp(Keys.Escape)) {
            isKeyUp = true;
            }
            if (ks.IsKeyDown(Keys.F5))
            {
                saveGame(StorageContainer.TitleLocation + FileIOManager.SAVE_PREFIX + "quick-" + level.levelName + FileIOManager.SAVE_EXTENSION);
            }
            if (ks.IsKeyDown(Keys.F6))
            {
                try
                {
                    loadGame(StorageContainer.TitleLocation + FileIOManager.SAVE_PREFIX + "quick-" + level.levelName + FileIOManager.SAVE_EXTENSION);
                }
                catch (Exception e)
                {
                   displayMessage("No quicksave found");
                   e.ToString();
                }
            }
            if (menu.isDisplayed || level == null)
            {
                menu.Update(ms, ks, this);
            }
            else if (easter && ks.IsKeyDown(Keys.Y))
            {
                int tempCoins = level.player.numCoins;
                level = FileIOManager.loadLevel(StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX + LEVEL_ORDER[++levelNumber],Content);
                level.LoadContent_OLD(Content);
                level.player.numCoins = tempCoins;
                easter = false;
                hasEaster = true;
                displayMessage("Resolution succeded: now using 'OLDSKOOL'");
            }
            else
            {
                Particle_Engine.ParticleEngine.update(dt);
                //update components
                if (!level.gameOver)
                {
                    int[] soundNums = level.Update(dt, ms, ks, IsActive);
                    for (int i = 0; i < soundNums.Length; i++)
                    {
                        if (soundNums[i] == 1)
                        {
                            FX[i].Play();
                        }
                    }
                    //if the lazer isnt playing, play it once
                    if (soundNums[2] == 2 && !lazer.State.Equals(SoundState.Playing))
                           lazer.Play();
                    //if it was playing but should stop before firing, then stop it
                    if (soundNums[2] < 2 && lazer.State.Equals(SoundState.Playing))
                    {
                        lazer.Dispose();
                        lazer = FX[FX.Length -1].CreateInstance();
                    }
                }
                else
                {
                    displayMessage("Press 'r' to restart the level or ESC to exit");
                    if (ks.IsKeyDown(Keys.R))
                    {
                        int tempCoins = level.player.numCoins - level.usedCoins.Count;
                        Particle_Engine.ParticleEngine.reset();
                        level = FileIOManager.loadLevel(StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX + level.levelName,Content);
                        if (!hasEaster) level.LoadContent(Content);
                        else level.LoadContent_OLD(Content);
                        level.player.numCoins = tempCoins;
                    }
                }
                if (level.levelCompleted)
                {
                    if (linearOrder)
                    {
                        int tempCoins = level.player.numCoins;
                        if (tempCoins % EASTER_EGG_THRESHOLD == 0 && !hasEaster && levelNumber < LEVEL_ORDER.Length - 1 && tempCoins > 0)
                        {
                            easter = true;
                        }
                        else if (levelNumber < LEVEL_ORDER.Length - 1 && !easter)
                        {
                            Particle_Engine.ParticleEngine.reset();
                            level = FileIOManager.loadLevel(StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX + LEVEL_ORDER[++levelNumber], Content);
                            if (!hasEaster) level.LoadContent(Content);
                            else
                            {
                                hasEaster = false;
                                level.LoadContent_OLD(Content);
                            }
                            level.player.numCoins = tempCoins;
                        }
                    }
                    else
                    {
                        menu.isDisplayed = true;
                    }
                }
            }

                base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (menu.isDisplayed || level == null)
            {
                menu.Draw(spriteBatch);
            }
            else if (easter)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "An error has occured, press y to attempt resolution", new Vector2(BackBufferWidth / 2 - font.MeasureString("An error has occured, press y to attempt resolution").X, BackBufferHeight / 2), Color.BlanchedAlmond);
                spriteBatch.End();
            }
            else
            {

                //Draw things 
                spriteBatch.Begin();
                level.Draw(spriteBatch);
                spriteBatch.DrawString(font, "Coins: " + level.player.numCoins.ToString(), new Vector2(0, 0), Color.White);

                for (int i = 0; i < messages.Count; i++)
                {
                    Message message = messages.ElementAt(i);
                    if (message.timeDisplayed > 0)
                    {
                        spriteBatch.DrawString(font, message.message, new Vector2((BackBufferWidth - font.MeasureString(message.message).X) / 2, i * 20), Color.Black);
                        spriteBatch.DrawString(font, message.message, new Vector2((BackBufferWidth - font.MeasureString(message.message).X) / 2 - 2, i * 20 - 2), Color.White);
                        message.timeDisplayed--;
                    }
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        //do actual game loading - to be called from ShellMenu
        public void loadGame(string fileName)
        {
            MoonPrison.clearMessages();
            level = FileIOManager.loadGame(fileName, Content);
            level.LoadContent(Content);
            if (this.LEVEL_ORDER.Contains(level.levelName))
            {
                linearOrder = true;
                levelNumber = Array.IndexOf(LEVEL_ORDER, level.levelName);
            }
            System.Threading.Thread.Sleep(50);
        }

        public void saveGame(string fileName)
        {
            FileIOManager.saveGame(level, fileName);
        }

        //code reuse!
        public static void displayMessage(string message)
        {
            displayMessage(message, MoonPrison.STRING_DISPLAY_TIME);
        }

        public static void displayMessage(string message, int duration)
        {
            Boolean add = true;
            LinkedList<String> mess = new LinkedList<String>();

            if (font.MeasureString(message).X > BackBufferWidth - MESSAGE_BORDER)
            {
                LinkedList<String> words = new LinkedList<string>(message.Split(null));
                while (words.Count > 0)
                {
                    string temp = "";
                    while (font.MeasureString(temp).X < BackBufferWidth - MESSAGE_BORDER && words.Count > 0)
                    {
                        temp = temp + " " + words.ElementAt(0);
                        words.RemoveFirst();
                    }
                    mess.AddLast(temp);
                }
            }
            else
            {
                mess.AddLast(message);
            }


 
            foreach (Message m in messages)
            {
                if (m.message == mess.ElementAt(0)) add = false;
            }
            if (add)
            {
                foreach (string s in mess)
                {
                    messages.AddLast(new Message(s, duration));
                }
            }
        }

        public static void clearMessages()
        {
            messages = new LinkedList<Message>();
        }

        private static void removeOldMessages()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages.ElementAt(i).timeDisplayed <= 0)
                {
                    messages.Remove(messages.ElementAt(i));
                }
            }
        }

        private string getNextMusicCue()
        {
            Random r = new Random();
            return MUSIC_CUE_NAMES[(r.Next() % MUSIC_CUE_NAMES.Length)];
        }

        public void playNextSong()
        {
            if (cue != null)
            {
                cue.Stop(AudioStopOptions.AsAuthored);
            }
            cue = soundBank.GetCue(getNextMusicCue());
            cue.Play();
        }

        /// <summary>
        /// Draws a line from startPoint to endPoint on the screen.  Note that both positions are in terms of the game
        /// window, NOT position within the level.  To draw lines between obects with relative position substract the origin.
        /// E.g. to draw the rope, call drawLine(level.player.position - level.origin, level.player.theHook.position - level.origin, Color.YELLOW)
        /// Note: This need not be called during a draw method but is totally self-contained and can happen at any time.
        /// </summary>
        /// <param name="startPoint">The point on the screen where the line should start</param>
        /// <param name="endPoint">The point on the screen where the line should end</param>
        /// <param name="lineColor">The color of the line</param>
        public void drawLine(Vector2 startPoint, Vector2 endPoint, Color lineColor)
        {
            VertexPositionColor[] pointList = new VertexPositionColor[2];
            pointList[0] = new VertexPositionColor(new Vector3(startPoint.X, startPoint.Y, 0),lineColor);
            pointList[1] = new VertexPositionColor(new Vector3(endPoint.X, endPoint.Y, 0), lineColor);
            short[] indices = new short[2]{0,1};

           basicEffect.Begin();
           foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
           {
               pass.Begin();
               GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, pointList, 0, 2, indices, 0, 1);
               pass.End();
           }
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList,pointList,0,2,indices,0,1);
            basicEffect.End();
        }
    }

    class Message
    {
        public string message;
        public int timeDisplayed;

        public Message(string m)
        {
            message = m;
            timeDisplayed = MoonPrison.STRING_DISPLAY_TIME;
        }

        public Message(string m, int d)
        {
            message = m;
            timeDisplayed = d;
        }
            
    }
}
