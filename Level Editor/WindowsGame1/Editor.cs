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
using System.IO;
using System.Text;
using System.Xml;

namespace LevelEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Editor : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont Font1;
        
        //constants for encoding levels
        public const int BLANK = 0;
        public const int GREEN = 1;
        public const int RED = 2;
        public const int BLUE = 3;
        public const int BUTTON = 4;
        public const int COIN = 5;
        public const int ENEMY = 6;
        public const int TARGET = 7;
        public const int DELETE = 8;
        public const int START = 9;
        public const int END = 10;

        //List for keeping track of buttons
        LinkedList<Button> buttons;
        bool targetMode;
        Button currentButton;

        //enemies
        LinkedList<Enemy> enemies;
        
        //Start and end points for the level
        VectorInt start;
        VectorInt end;

        //variables for drawing
        public int tileHeight;
        public int tileWidth;
        public const int NUM_TOOLS = 11;
        //unscrollable tools are ones that must be selected from the taskbar. Things that aren't tiles. Unscrollable tools should always have higher constants than scrollable tools.
        public const int NUM_TILES = 7;
        public Texture2D[] textures = new Texture2D[NUM_TOOLS];
        public Toolbar toolbar;

        //Size of the screen and of the level
        //Powers of 2 are nice
        public const int TOOLBAR_HEIGHT = 30;
        public int LEVELH = 17;
        public int LEVELW = 64;
        public const int SCREENH = 768 + TOOLBAR_HEIGHT;
        public const int SCREENW = 1024;
        public const int GAMESCREENX = 1024;
        public const int GAMESCREENY = 768;
        public const int GAMETILESX = GAMESCREENX / 64;
        public const int GAMETILESY = GAMESCREENY / 64;
        public const int VISIBLE_TILES_X = 21;
        public const int VISIBLE_TILES_Y = 16;
        public const int SCROLL_BOUNDARY = 20;
        public const int SCROLL_SENSITIVITY = 10;
        //flag true if you want mouse scrolling
        public const bool MOUSE_SCROLL = false;

        public Rectangle mouseRect;

        //The file to be loaded/saved
        public const String FILENAME = "level";
        public const int LEVEL_NUMBER = 13;

        //The tile to be placed
        public int currentTool = 0;
        //whether or not the mouse is currently dragging out a region 
        public bool dragging = false;
        public VectorInt dragStart;
        //Variables for selecting button targets

        //to make switching colors reasonable
        public int cooldown = 0;
        

        //The current level, loaded in from a file of the given name (above) if it exists
        public int[,] tiles;
        public VectorInt origin;

        public Editor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferHeight = SCREENH + TOOLBAR_HEIGHT;
            graphics.PreferredBackBufferWidth = SCREENW;
            graphics.ApplyChanges();

            tileHeight = (SCREENH - TOOLBAR_HEIGHT)/VISIBLE_TILES_Y;
            tileWidth = SCREENW / VISIBLE_TILES_X;

            origin = new VectorInt(0,0);
            dragStart = new VectorInt(0,0);
            start = new VectorInt(1, 1);
            end = new VectorInt(2, 2);

            buttons = new LinkedList<Button>();
            enemies = new LinkedList<Enemy>();
            //mouseRect = new Rectangle(0, 0, GAMETILESX * tileSize, GAMETILESY * tileSize);

            //this.IsMouseVisible = true;

            FileStream file = new FileStream(FILENAME + LEVEL_NUMBER + ".level", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader fileIn = new StreamReader(file);

            //Throws away the dimension values. This could certainly be changed to improve the editor.
            if (!fileIn.EndOfStream)
            {
                LEVELW = int.Parse(fileIn.ReadLine());
                LEVELH = int.Parse(fileIn.ReadLine());
            }

            tiles = new int[LEVELW, LEVELH];

            if (!fileIn.EndOfStream)
            {
                for (int y = 0; y < LEVELH; y++)
                {
                    String line = fileIn.ReadLine();
                    String[] buffer = line.Split(new char[] { ' ' });

                    for (int x = 0; x < LEVELW; x++)
                    {
                        int type = int.Parse(buffer[x]);
                        tiles[x, y] = type;
                    }
                }


                XmlTextReader xml = new XmlTextReader(FILENAME + LEVEL_NUMBER + ".xml");
                while (xml.Read())
                {
                    if (xml.Name == "level" && xml.HasAttributes)
                    {
                        int x = int.Parse(xml.GetAttribute("endX"));
                        int y = int.Parse(xml.GetAttribute("endY"));
                        end = new VectorInt(x, y);

                        x = int.Parse(xml.GetAttribute("startX"));
                        y = int.Parse(xml.GetAttribute("startY"));
                        start = new VectorInt(x, y);
                    }

                    //if the node is a tag
                    if (xml.NodeType == XmlNodeType.Element)
                    {
                        //if the tag is a button
                        if (xml.Name == "button")
                        {


                            Dictionary<string, string> enemyFields = new Dictionary<string, string>();
                            Dictionary<string, string> buttonFields = new Dictionary<string, string>();
                            int buttonX = int.Parse(xml.GetAttribute("x"));
                            int buttonY = int.Parse(xml.GetAttribute("y"));
                            int numUses = int.Parse(xml.GetAttribute("uses"));
                            string[] useActions = new string[numUses];
                            LinkedList<Vector3>[] targets = new LinkedList<Vector3>[numUses];
                            xml.MoveToAttribute(0);
                            for (int i = 0; i < xml.AttributeCount; i++)
                            {
                                buttonFields.Add(xml.Name, xml.Value);
                                xml.MoveToNextAttribute();
                            }

                            //while we haven't gotten to the end of the button
                            xml.Read();
                            while (xml.Name != "button")
                            {
                                if (xml.NodeType == XmlNodeType.Element && xml.Name.StartsWith("use"))
                                {
                                    LinkedList<Vector3> useTargets = new LinkedList<Vector3>();
                                    int number = int.Parse(xml.Name.Substring(3));
                                    string variety = xml.GetAttribute("type");
                                    useActions[number] = variety;
                                    xml.Read();
                                    while (!xml.Name.StartsWith("use"))
                                    {

                                        //read in nodes, which are all targets
                                        //each target has a point corresponding to a tile in the layout and a type to change that tile to
                                        if (xml.NodeType == XmlNodeType.Element && xml.Name == "target")
                                        {
                                            int x = int.Parse(xml.GetAttribute("x"));
                                            int y = int.Parse(xml.GetAttribute("y"));
                                            int type = int.Parse(xml.GetAttribute("type"));
                                            useTargets.AddLast(new Vector3(x, y, type));
                                        }
                                        /*else if (xml.NodeType == XmlNodeType.Element && xml.Name == "enemy")
                                        {
                                            xml.MoveToAttribute(0);
                                            for (int i = 0; i < xml.AttributeCount; i++)
                                            {
                                                enemyFields.Add(xml.Name, xml.Value);
                                                xml.MoveToNextAttribute();
                                            }
                                        }*/
                                        xml.Read();
                                    }
                                    targets[number] = useTargets;
                                }
                                xml.Read();
                            }

                            Button b = new Button(buttonX, buttonY);
                            //b.useActions = useActions;
                            //b.enemyParams = enemyFields;

                            foreach (LinkedList<Vector3> l in targets)
                            {
                                b.addUse(l);
                            }

                            buttons.AddLast(b);
                        }
                        else if (xml.Name == "enemy")
                        {
                            Dictionary<string, string> fields = new Dictionary<string, string>();
                            xml.MoveToAttribute(0);
                            for (int i = 0; i < xml.AttributeCount; i++)
                            {
                                fields.Add(xml.Name, xml.Value);
                                xml.MoveToNextAttribute();
                            }
                            enemies.AddLast(createEnemyWithParams(fields));
                        }
                    }
                }

                xml.Close();
            }

            else
            {
                for (int x = 0; x < LEVELW; x++)
                {
                    for (int y = 0; y < LEVELH; y++)
                    {

                        tiles[x, y] = 0;
                        if (x == 0 || y == 0 || x == LEVELW - 1 || y == LEVELH - 1) tiles[x,y] = 1;
                    }
                }
                
            }

            fileIn.Close();
            file.Close();

            


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


            textures[RED] = Content.Load<Texture2D>(@"redsquare");
            textures[GREEN] = Content.Load<Texture2D>(@"greensquare");
            textures[BLUE] = Content.Load<Texture2D>(@"bluesquare");
            textures[BLANK] = Content.Load<Texture2D>(@"crosshair");
            textures[BUTTON] = Content.Load<Texture2D>(@"orangesquare");
            textures[COIN] = Content.Load<Texture2D>(@"coin");
            textures[ENEMY] = Content.Load<Texture2D>(@"enemy");
            textures[TARGET] = Content.Load<Texture2D>(@"target");
            textures[DELETE] = Content.Load<Texture2D>(@"delete");
            textures[START] = Content.Load<Texture2D>(@"start");
            textures[END] = Content.Load<Texture2D>(@"end");
            


            Font1 = Content.Load<SpriteFont>(@"font");

            toolbar = new Toolbar(textures, new Vector2(SCREENW, TOOLBAR_HEIGHT), Vector2.Zero);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            save();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            updateScrolling(ms, ks);

            if (ks.IsKeyDown(Keys.Escape))
            {
                targetMode = false;
            }

            //mouseRect.Center = new Point(ms.X,ms.Y);

            if (ms.RightButton == ButtonState.Pressed && cooldown == 0)
            {
                currentTool = (currentTool + 1) % NUM_TILES;
                cooldown = 10;
            }

            if (cooldown != 0) cooldown--;

            if (ms.LeftButton == ButtonState.Pressed && !dragging)
            {
                if (ms.Y < toolbar.position.Y + TOOLBAR_HEIGHT)
                {
                    currentTool = toolbar.parseClick(ms, currentTool);
                    //If target was selected turn on target mode
                    if (currentTool == TARGET)
                    {
                        targetMode = true;
                        currentButton = null;
                    }
                }
                else if (targetMode)
                {
                    VectorInt click = getTileIndex(ms.X, ms.Y);
                    if (tiles[click.X, click.Y] == BUTTON && currentButton == null)
                    {
                        currentButton = getButtonAt(click);
                    }
                    else if (currentButton != null && currentTool == TARGET && cooldown == 0)
                    {
                        cooldown = 20;
                        if (ks.IsKeyDown(Keys.N))
                        {
                            currentButton.addUse();
                        }
                        else
                        {
                            currentButton.nextUse();
                        }
                    }
                    else if (currentButton != null && currentTool == DELETE)
                    {
                        currentButton.getCurrentUse().deleteTarget(click.X, click.Y);
                    }
                    else if (currentButton != null && currentTool != TARGET)
                    {
                        currentButton.getCurrentUse().addTarget(click.X, click.Y, currentTool);
                    }

                }
                else
                {
                    dragging = true;
                    dragStart.X = ms.X; dragStart.Y = ms.Y;
                }
            }

            if(dragging && ms.LeftButton == ButtonState.Released){
                    dragging = false;

                    VectorInt click = getTileIndex(ms.X, ms.Y);
                    int x = click.X; int y = click.Y;

                    dragStart = getTileIndex(dragStart.X, dragStart.Y);

                    //So you can drag in any direction
                    if (x < dragStart.X)
                    {
                        int tmp = x;
                        x = dragStart.X;
                        dragStart.X = tmp;
                    }
                    if (y < dragStart.Y)
                    {
                        int tmp = y;
                        y = dragStart.Y;
                        dragStart.Y = tmp;
                    }

                    for (int m = dragStart.X; m <= x; m++)
                    {
                        for (int n = dragStart.Y; n <= y; n++)
                        {
                            if (m >= 0 && m < LEVELW && n >= 0 && n < LEVELH)
                            {
                                if(currentTool < NUM_TILES) tiles[m, n] = currentTool;
                                if (currentTool == DELETE)
                                {
                                    deleteButton(m, n);
                                    deleteEnemy(m, n);
                                    tiles[m, n] = BLANK;
                                }
                                if (currentTool == BUTTON) createButton(m ,n);
                                if (currentTool == ENEMY) enemies.AddLast(new Enemy(m, n, enemies.Count));
                                if (currentTool == START)
                                {
                                    start.X = m; start.Y = n;
                                }
                                else if (currentTool == END)
                                {
                                    end.X = m; end.Y = n;
                                }
                            }
                        }
                    }
             }

            if (ks.IsKeyDown(Keys.Space) && cooldown == 0)
            {
                save();
                cooldown = 10;
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

            Vector2 position = new Vector2();
            position.X = 0; position.Y=0;

            spriteBatch.Begin();

            Vector2 pos = new Vector2();
            pos.X = (origin.X - (origin.X % tileWidth)) / tileWidth; pos.Y = (origin.Y - (origin.Y % tileHeight)) / tileHeight;

            for (int x = (int)pos.X; x <= pos.X + VISIBLE_TILES_X + 10 && x < LEVELW; x++)
            {
                for (int y = (int)pos.Y; y <= pos.Y + VISIBLE_TILES_Y + 10 && y < LEVELH; y++)
                {
                    if (tiles[x, y] != 0)
                    {
                        Rectangle dest = new Rectangle(x * tileWidth - (int)origin.X, TOOLBAR_HEIGHT + y * tileHeight - (int)origin.Y, tileWidth, tileHeight);
                        spriteBatch.Draw(textures[tiles[x, y]], dest, Color.AliceBlue);
                        if (tiles[x, y] == ENEMY) spriteBatch.DrawString(Font1, ""+getEnemyAt(new VectorInt(x,y)).num, new Vector2(x * tileWidth - origin.X, TOOLBAR_HEIGHT + y * tileHeight - origin.Y), Color.Wheat);
                    }
                }
            }

            toolbar.Draw(spriteBatch);
            drawButtons(spriteBatch);

            spriteBatch.DrawString(Font1, ""+targetMode, Vector2.Zero, Color.Wheat);

            spriteBatch.DrawString(Font1, "Start", new Vector2(tileWidth * start.X - origin.X, TOOLBAR_HEIGHT + tileHeight * start.Y - origin.Y), Color.Wheat);
            spriteBatch.DrawString(Font1, "End", new Vector2(tileWidth * end.X - origin.X, TOOLBAR_HEIGHT + tileHeight * end.Y - origin.Y), Color.Wheat);

            MouseState ms = Mouse.GetState();
            Rectangle rect = new Rectangle(ms.X, ms.Y, tileWidth, tileHeight);
            spriteBatch.Draw(textures[currentTool], rect, Color.AliceBlue);
            Vector2 temp = new Vector2();
            int adjY = ms.Y - TOOLBAR_HEIGHT + (int)origin.Y; int adjX = ms.X + (int)origin.X;
            temp.X = (adjX - (adjX % tileWidth)) / tileWidth; temp.Y = (adjY - (adjY % tileHeight)) / tileHeight;
            Vector2 mouse = new Vector2(ms.X, ms.Y);

            spriteBatch.DrawString(Font1, temp.X + ", " + temp.Y, mouse, Color.Wheat);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected void save()
        {
            int counter = 0;
            FileStream file = new FileStream(FILENAME + counter, FileMode.OpenOrCreate, FileAccess.Write);
            bool overwrite = true;
            
            while (overwrite)
            {
                file.Close();
                file = new FileStream(FILENAME + counter + ".level", FileMode.OpenOrCreate, FileAccess.Write);
                overwrite = false;
                if (file.Length > 0)
                {
                    counter++;
                    overwrite = true;
                }
            }

            StreamWriter sr = new StreamWriter(file);
            sr.Write(LEVELW + "\n"); sr.Write(LEVELH + "\n");

            for (int y = 0; y < LEVELH; y++)
            {
                for (int x = 0; x < LEVELW; x++)
                {
                    if (tiles[x, y] != ENEMY)
                    {
                        sr.Write(tiles[x, y] + " ");
                    }
                    else
                    {
                        sr.Write(0 + " ");
                    }
                }
                sr.Write("\n");
            }
            
            sr.Close();
            file.Close();

            XmlTextWriter writer = new XmlTextWriter(FILENAME + counter + ".xml", null);
            //writer.WriteStartDocument();
            writer.WriteStartElement("level");

            writer.WriteAttributeString("endX", ""+end.X);
            writer.WriteAttributeString("endY", "" + end.Y);
            writer.WriteAttributeString("startX", "" + start.X);
            writer.WriteAttributeString("startY", "" + start.Y);

            foreach (Enemy e in enemies)
            {
                writer.WriteComment("Enemy #" + e.num);
                writer.WriteStartElement("enemy");
                writer.WriteAttributeString("x", "" + e.x);
                writer.WriteAttributeString("y", "" + e.y);
                writer.WriteAttributeString("weaponType", "" + e.weaponType);
                writer.WriteAttributeString("patrolRange", "" + e.patrolRange);
                writer.WriteAttributeString("patrolspeed", "" + e.patrolspeed);
                writer.WriteAttributeString("vertViewRange", "" + e.vertViewRange);
                writer.WriteAttributeString("lazerChargeTime", "" + e.lazerChargeTime);
                writer.WriteAttributeString("horizViewRange", "" + e.horizViewRange);
                writer.WriteAttributeString("soundRange", "" + e.soundRange);
                writer.WriteAttributeString("investigateMaxWait", "" + e.investigateMaxWait);
                writer.WriteAttributeString("fireInterval", "" + e.fireInterval);
                writer.WriteAttributeString("lazerDuration", "" + e.lazerDuration);
                writer.WriteFullEndElement();

            }
            foreach (Button b in buttons)
            {
                writer.WriteStartElement("button");
                writer.WriteStartAttribute("x");
                writer.WriteString("" + b.x);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("y");
                writer.WriteString("" + b.y);
                writer.WriteEndAttribute();
                writer.WriteAttributeString("uses", "" + b.uses.Count);

                int n = 0;

                foreach (Use u in b.uses)
                {

                    writer.WriteStartElement("use" + n);
                    writer.WriteAttributeString("type", "changeTiles");

                    foreach (Target t in u.targets)
                    {
                        writer.WriteStartElement("target");
                        writer.WriteStartAttribute("x");
                        writer.WriteString("" + t.x);
                        writer.WriteEndAttribute();
                        writer.WriteStartAttribute("y");
                        writer.WriteString("" + t.y);
                        writer.WriteEndAttribute();
                        writer.WriteStartAttribute("type");
                        writer.WriteString("" + t.type);
                        writer.WriteEndAttribute();
                        writer.WriteEndElement();
                    }
                    writer.WriteFullEndElement();
                    n++;
                }
                writer.WriteFullEndElement();
            }

            writer.WriteFullEndElement();
            //writer.WriteEndDocument();
            writer.Close();
        }

        private void updateScrolling(MouseState ms, KeyboardState ks)
        {
            if (MOUSE_SCROLL)
            {
                if (ms.X < SCROLL_BOUNDARY)
                {
                    origin.X -= SCROLL_SENSITIVITY;
                }
                if (ms.X > SCREENW - SCROLL_BOUNDARY)
                {
                    origin.X += SCROLL_SENSITIVITY;
                }
                if (ms.Y < TOOLBAR_HEIGHT + SCROLL_BOUNDARY)
                {
                    origin.Y -= SCROLL_SENSITIVITY;
                }
                if (ms.Y > SCREENH - SCROLL_BOUNDARY)
                {
                    origin.Y += SCROLL_SENSITIVITY;
                }
            }

            if(ks.IsKeyDown(Keys.Left)){
                origin.X -= SCROLL_SENSITIVITY;
            }
            if(ks.IsKeyDown(Keys.Right))
            {
                origin.X += SCROLL_SENSITIVITY;
            }
            if(ks.IsKeyDown(Keys.Up)){
                origin.Y -= SCROLL_SENSITIVITY;
            }
            if(ks.IsKeyDown(Keys.Down)){
                origin.Y += SCROLL_SENSITIVITY;
            }


            
            if (origin.X > LEVELW * tileWidth - VISIBLE_TILES_X * tileWidth) origin.X = LEVELW * tileWidth - VISIBLE_TILES_X * tileWidth - 1;
            if (origin.Y > LEVELH * tileHeight - VISIBLE_TILES_Y * tileHeight) origin.Y = LEVELH * tileHeight - VISIBLE_TILES_Y * tileHeight - 1;
            if (origin.X < 0) origin.X = 0;
            if (origin.Y < 0) origin.Y = 0;
            
        }

        //Returns the tile index for the specified x y (screen) position
        public VectorInt getTileIndex(int x, int y){
            return new VectorInt(Math.Max(0,(int)(x + origin.X) / tileWidth),Math.Max(0,(int)(y - TOOLBAR_HEIGHT + origin.Y) / tileHeight));
        }

        //returns the button with the specified tile index
        public Button getButtonAt(VectorInt index){

            foreach(Button b in buttons){

                if(b.x == index.X && b.y == index.Y){
                    return b;
                }
            }
            return null;
        }

        public Enemy getEnemyAt(VectorInt index)
        {

            foreach (Enemy e in enemies)
            {

                if (e.x == index.X && e.y == index.Y)
                {
                    return e;
                }
            }
            return null;
        }

        public void drawButtons(SpriteBatch spriteBatch){
            
            Color alpha = new Color(new Vector4(30,30,30, 100));
            foreach (Button b in buttons)
            {
                if (b.uses.Count > 0)
                {
                    foreach (Target t in b.getCurrentUse().targets)
                    {
                        Rectangle dest = new Rectangle(t.x * tileWidth - (int)origin.X, TOOLBAR_HEIGHT + t.y * tileHeight - (int)origin.Y, tileWidth/2, tileHeight);
                        spriteBatch.Draw(textures[t.type], dest, alpha);
                    }

                    spriteBatch.DrawString(Font1, "" + b.currentUse, new Vector2(tileWidth * b.x - (int)origin.X, TOOLBAR_HEIGHT + tileHeight * b.y - (int)origin.Y), Color.Wheat);
                }
            }
                
        }

        public void createButton(int x, int y)
        {
            buttons.AddFirst(new Button(x, y));
        }

        public void deleteButton(int x, int y)
        {
            foreach (Button b in buttons)
            {
                if (b.x == x && b.y == y)
                {
                    buttons.Remove(b);
                    return;
                }
            }
        }

        public void deleteEnemy(int x, int y)
        {
            foreach (Enemy e in enemies)
            {
                if (e.x == x && e.y == y)
                {
                    enemies.Remove(e);
                    return;
                }
            }
        }

        public Enemy createEnemyWithParams(Dictionary<string, string> fields)
        {
            int enemyX = int.Parse(fields["x"]);
            int enemyY = int.Parse(fields["y"]);

            Enemy e = new Enemy(enemyX, enemyY, enemies.Count);

            foreach (KeyValuePair<string, string> entry in fields)
            {
                if (entry.Key == "patrolspeed")
                {
                    e.patrolspeed = int.Parse(entry.Value);
                }
                else if (entry.Key == "patrolRange")
                {
                    e.patrolRange = int.Parse(entry.Value);
                }
                else if (entry.Key == "investigateMaxWait")
                {
                    e.investigateMaxWait = int.Parse(entry.Value);
                }
                else if (entry.Key == "vertViewRange")
                {
                    e.vertViewRange = int.Parse(entry.Value);
                }
                else if (entry.Key == "horizViewRange")
                {
                    e.horizViewRange = int.Parse(entry.Value);
                }
                else if (entry.Key == "soundRange")
                {
                    e.soundRange = int.Parse(entry.Value);
                }
                else if (entry.Key == "weaponType")
                {
                    e.weaponType = int.Parse(entry.Value);
                }
                else if (entry.Key == "lazerChargeTime")
                {
                    e.lazerChargeTime = float.Parse(entry.Value);
                }
                else if (entry.Key == "lazerDuration")
                {
                    e.lazerDuration = float.Parse(entry.Value);
                }
                else if (entry.Key == "fireInterval")
                {
                    e.fireInterval = float.Parse(entry.Value);
                }
            }
            return e;
        }

    }
}
