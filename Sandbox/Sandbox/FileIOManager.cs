using System;
using System.Collections.Generic;
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
    class FileIOManager
    {

        //constants that define where we are looking for things and what we're calling them
        public const String SAVE_PREFIX = ".\\..\\..\\..\\Content\\Saves\\";
        public const String LEVEL_PREFIX = ".\\..\\..\\..\\Content\\Levels\\";
        public const String SAVE_EXTENSION = ".sav";

        public static Level loadLevel(string fileName,ContentManager Content)
        {

            Level level = new Level();
            level.levelName = fileName.Split("\\".ToCharArray()).Last();

            //read in the level layout
            FileStream file = new FileStream(fileName + ".level", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader fileIn = new StreamReader(file);
            Vector2 endpos = Vector2.Zero;

            level.levelW = int.Parse(fileIn.ReadLine());
            level.levelH = int.Parse(fileIn.ReadLine());

            level.layout = new Tile[level.levelW, level.levelH];

            for (int y = 0; y < level.levelH; y++)
            {
                String line = fileIn.ReadLine();
                String[] buffer = line.Split(new char[] { ' ' });

                for (int x = 0; x < level.levelW; x++)
                {
                    level.layout[x, y] = new Tile(int.Parse(buffer[x]));
                    Vector2 position = new Vector2();
                    position.X = x * Level.tileWidth + Level.tileWidth / 2; position.Y = y * Level.tileHeight + Level.tileHeight / 2;
                    level.layout[x, y].position = position;
                    level.layout[x, y].setWidth(Level.tileWidth);
                    level.layout[x, y].setHeight(Level.tileHeight);
                    level.layout[x, y].updateBound();
                }
            }
            fileIn.Close();
            file.Close();

            //read the associated XML file
            XmlTextReader xml = new XmlTextReader(fileName + ".xml");
            while (xml.Read())
            {
                if (xml.Name == "level" && xml.HasAttributes)
                {
                    int x = int.Parse(xml.GetAttribute("endX"));
                    int y = int.Parse(xml.GetAttribute("endY"));
                    level.layout[x, y].assignBooleans(6);
                    int px = int.Parse(xml.GetAttribute("startX"));
                    int py = int.Parse(xml.GetAttribute("startY"));
                    level.player.position = new Vector2(px * Level.tileWidth + Level.tileWidth / 2, py * Level.tileHeight);
                }

                //if the node is a tag
                if (xml.NodeType == XmlNodeType.Element)
                {
                    //if the tag is a button
                    if (xml.Name == "button")
                    {
                        Dictionary<string, string> enemyFields = new Dictionary<string, string>();
                        Dictionary<string, string> buttonFields = new Dictionary<string, string>();
                        LinkedList<string> messages = new LinkedList<string>();
                        int buttonX = int.Parse(xml.GetAttribute("x"));
                        int buttonY = int.Parse(xml.GetAttribute("y"));
                        int numUses = int.Parse(xml.GetAttribute("uses"));
                        string[] useActions = new string[numUses];
                        Vector3[][] targets = new Vector3[numUses][];
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
                                        level.layout[x, y].isChangedbyButton = true;
                                    }
                                    else if (xml.NodeType == XmlNodeType.Element && xml.Name == "enemy")
                                    {
                                        xml.MoveToAttribute(0);
                                        for (int i = 0; i < xml.AttributeCount; i++)
                                        {
                                            enemyFields.Add(xml.Name, xml.Value);
                                            xml.MoveToNextAttribute();
                                        }
                                    }
                                    else if (xml.NodeType == XmlNodeType.Element && xml.Name == "display")
                                    {
                                        messages.AddLast(xml.GetAttribute("message"));
                                    }
                                    xml.Read();
                                }
                                targets[number] = useTargets.ToArray();
                            }
                            xml.Read();
                        }

                        Button b = new Button();
                        b.useActions = useActions;
                        b.enemyParams = enemyFields;
                        b.messages = messages;
                        b.tiles = targets.ToArray();
                        foreach (KeyValuePair<string, string> entry in buttonFields)
                        {
                            if (entry.Key == "uses")
                            {
                                b.totalUses = int.Parse(entry.Value);
                                b.isMultipleUse = (b.totalUses > 1);
                            }
                            else if (entry.Key == "cycles")
                            {
                                b.cycles = Boolean.Parse(entry.Value);
                            }
                            else if (entry.Key == "sensor")
                            {
                                b.isMotionSensor = Boolean.Parse(entry.Value);
                            }
                        }
                        if (b.isMotionSensor)
                        {
                            b.assignBooleans(7);
                        }
                        
                        level.layout[buttonX, buttonY] = b;
                        Vector2 position = new Vector2();
                        position.X = buttonX * Level.tileWidth + Level.tileWidth/2; position.Y = buttonY * Level.tileHeight+Level.tileHeight/2;
                        level.layout[buttonX, buttonY].position = position;
                        level.layout[buttonX, buttonY].setWidth(Level.tileWidth);
                        level.layout[buttonX, buttonY].setHeight(Level.tileHeight);
                        level.layout[buttonX, buttonY].origin = new Vector2(Level.tileWidth, Level.tileHeight) * .05f;
                        level.layout[buttonX, buttonY].updateBound();

                    }
                    //the player xml name sets player initial position
                    else if (xml.Name == "player")
                    {
                        int playerX = int.Parse(xml.GetAttribute("x"));
                        int playerY = int.Parse(xml.GetAttribute("y"));
                        level.player.position = new Vector2(playerX * Level.tileWidth, playerY * Level.tileHeight);
                    }
                    //enemy expects an initial position
                    else if (xml.Name == "enemy")
                    {
                        Dictionary<string,string> fields = new Dictionary<string,string>();
                        xml.MoveToAttribute(0);
                        for (int i = 0; i < xml.AttributeCount; i++)
                        {
                            fields.Add(xml.Name,xml.Value);
                            xml.MoveToNextAttribute();
                        }
                        level.enemies.AddLast(FileIOManager.createEnemyWithParams(fields));
                    }
                }
            }
            xml.Close();

            return level;
        }

        /// <summary>
        /// This currently just saves the name of the level to the /Content/Saves directory in xml form
        /// </summary>
        /// <param name="level">the level to be saved</param>
        /// <param name="fileName">the name of the file to save it to, no extensions, no paths</param>
        public static void saveGame(Level level, String fileName) {
            if (!level.player.isDead)
            {
                //create a new writer for the file using UTF-8
                XmlTextWriter xml = new XmlTextWriter(fileName, Encoding.UTF8);
                String filename = level.levelName.Split("\\".ToCharArray()).Last();
                //write the level's filename
                xml.WriteStartElement("Level");
                xml.WriteAttributeString("fileName", filename);

                //write out <player x="?" y="?" coins="?"></player>
                xml.WriteStartElement("player");
                xml.WriteAttributeString("x", level.player.position.X.ToString());
                xml.WriteAttributeString("y", level.player.position.Y.ToString());
                xml.WriteAttributeString("coins", level.player.numCoins.ToString());
                xml.WriteFullEndElement();

                //write out each coin as <coin x="?" y="?"></coin> where ? is an integer
                foreach (Vector2 coin in level.usedCoins)
                {
                    xml.WriteStartElement("coin");
                    xml.WriteAttributeString("x", coin.X.ToString());
                    xml.WriteAttributeString("y", coin.Y.ToString());
                    xml.WriteFullEndElement();
                }
                //write out each used button as <button x="?" y="?" numTimes="?"></button> Where ? is an integer
                foreach (KeyValuePair<Vector2, int> button in level.usedButtons)
                {
                    xml.WriteStartElement("button");
                    xml.WriteAttributeString("x", button.Key.X.ToString());
                    xml.WriteAttributeString("y", button.Key.Y.ToString());
                    xml.WriteAttributeString("numTimes", button.Value.ToString());
                    xml.WriteFullEndElement();
                }
                //writeout hook position & data
                xml.WriteStartElement("hook");
                xml.WriteAttributeString("x", level.player.theHook.position.X.ToString());
                xml.WriteAttributeString("y", level.player.theHook.position.Y.ToString());
                xml.WriteAttributeString("extended", level.player.hookExtended.ToString());
                xml.WriteFullEndElement();

                //write </Level> and close the IO operations
                xml.WriteFullEndElement();
                xml.Close();
                MoonPrison.displayMessage("Game Saved");
            }
        }

        public static Level loadGame(String saveGameName, ContentManager Content)
        {
            //read the save file, initialize variables
            XmlTextReader xml = new XmlTextReader(saveGameName);
            String levelName = "";
            Level level;
            int startingCoins = 0;
            Vector2 playerpos = new Vector2();
            Vector2 hookpos = new Vector2();
            Boolean extended = false;
            LinkedList<Vector2> usedCoins = new LinkedList<Vector2>();
            Dictionary<Vector2, int> usedButtons = new Dictionary<Vector2, int>();
            //start reading
            while (xml.Read())
            {
                //if what has just been read is a tag like <Level>
                if (xml.NodeType == XmlNodeType.Element)
                {
                    //if it actually was <Level>
                    if (xml.Name == "Level")
                    {
                        //get the filename
                        levelName = xml.GetAttribute("fileName");
                    }
                    //get tne player's position and number of coins
                    else if (xml.Name == "player")
                    {
                        float x = float.Parse(xml.GetAttribute("x"));
                        float y = float.Parse(xml.GetAttribute("y"));
                        startingCoins = int.Parse(xml.GetAttribute("coins"));
                        playerpos = new Vector2(x, y);
                    }
                    //if the name is coin, read it in and add it to list of used coins. x, y should be ints that represent locations in the layout
                    else if (xml.Name == "coin")
                    {
                        int x = int.Parse(xml.GetAttribute("x"));
                        int y = int.Parse(xml.GetAttribute("y"));
                        usedCoins.AddLast(new Vector2(x, y));
                    }
                    //if its a button, read it in and add it to the list of used buttons.  x, y, should be ints that represent locations in the layout
                    else if (xml.Name == "button")
                    {
                        int x = int.Parse(xml.GetAttribute("x"));
                        int y = int.Parse(xml.GetAttribute("y"));
                        int times = int.Parse(xml.GetAttribute("numTimes"));
                        usedButtons.Add(new Vector2(x, y), times);
                    }
                    else if (xml.Name == "hook")
                    {
                        float x = float.Parse(xml.GetAttribute("x"));
                        float y = float.Parse(xml.GetAttribute("y"));
                        extended = Boolean.Parse(xml.GetAttribute("extended"));
                        hookpos = new Vector2(x,y);
                    }
                }
            }
            xml.Close();

            //if we got a filename
           if (levelName != "")
            {
                //make the level
                level = FileIOManager.loadLevel(StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX + levelName, Content);
                level.player.numCoins = startingCoins;
                level.player.position = playerpos;
                level.player.hookExtended = extended;
                level.player.theHook.draw = extended;
                level.player.theHook.position = hookpos;
                level.usedButtons = usedButtons;
                level.usedCoins = usedCoins;
                //FIRST use the buttons... then use the coins...
                foreach (KeyValuePair<Vector2, int> button in usedButtons)
                {
                    for (int i = 0; i < button.Value; i++)
                    {
                        level.useButton((Button)level.layout[(int)button.Key.X, (int)button.Key.Y]);
                    }
                }
                //make all the used coins blank and use all of the used buttons the appropriate number of times
                foreach (Vector2 coin in usedCoins)
                {
                    Tile[,] layout = level.layout;
                    Tile tile = level.layout[(int)coin.X, (int)coin.Y];
                    tile.assignBooleans(0);
                }
            }
            else 
            {
                //this should probably display something and let the user retry, but I guess that'll happen with GUI stuff / menus
                throw new FileNotFoundException("The file " + saveGameName + " was not found");
            }
            MoonPrison.displayMessage("Game Loaded");
            return level;
        }

        public static Enemy createEnemyWithParams(Dictionary<string, string> fields)
        {
            int enemyX = int.Parse(fields["x"]);
            int enemyY = int.Parse(fields["y"]);

            Enemy e = new Enemy((int) ((enemyX+.5) * Level.tileWidth), (int) ((enemyY+ .5) * Level.tileHeight));

            foreach (KeyValuePair<string, string> entry in fields)
            {
                if (entry.Key == "patrolspeed")
                {
                    e.patrolspeed = int.Parse(entry.Value);
                    e.velocity.X = (float)-e.patrolspeed;
                }
                else if (entry.Key == "patrolRange")
                {
                    e.patrolRange = int.Parse(entry.Value);
                }
                else if (entry.Key == "maxSpeed")
                {
                    e.maxSpeed = float.Parse(entry.Value);
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
