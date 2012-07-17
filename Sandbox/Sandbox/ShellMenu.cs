using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    class ShellMenu
    {
        //some constants about where buttons get to be
        private const int BUTTON_SPACER = 30;
        private const int BUTTON_START_POINT = 200;

        public Boolean isDisplayed;
        public SpriteFont font;
        private string[] MENU_ITEMS = new string[5] {"StartGame", "JumptoLevel","Load","Save",  "Quit" };
        private MenuButton[] buttonList;
        private OpenFileDialog fileO;
        private OpenFileDialog fileL;
        private SaveFileDialog fileS;
        private Crosshair cross;
        private Sprite title = new Sprite();

        public ShellMenu()
        {
            isDisplayed = true;
            buttonList = new MenuButton[MENU_ITEMS.Length];
            fileO = new OpenFileDialog();
            fileS = new SaveFileDialog();
            fileL = new OpenFileDialog();
            fileS.InitialDirectory = StorageContainer.TitleLocation + FileIOManager.SAVE_PREFIX;
            fileO.InitialDirectory = StorageContainer.TitleLocation + FileIOManager.SAVE_PREFIX;
            fileL.InitialDirectory = StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX;
            fileS.Filter = "MoonPrison Save Files|*" + FileIOManager.SAVE_EXTENSION;
            fileO.Filter = "MoonPrison Save Files|*" + FileIOManager.SAVE_EXTENSION;
            fileL.Filter = "Level Files|*.level";
            fileO.Title = "Load Files";
            cross = new Crosshair();
            for (int i = 0; i < MENU_ITEMS.Length; i++)
            {
                buttonList[i] = new MenuButton(MENU_ITEMS[i]);
            }
        }

        public void LoadContent(ContentManager Content)
        {
            title.LoadContent(Content, @"Sprites/moonprison");
            title.position = new Vector2(Level.ScreenW / 2, title.getHeight()/2 + 15);
            cross.LoadContent(Content, @"Sprites/crosshair");
            bool alternator = true;
            foreach (MenuButton button in buttonList)
            {
                Sprite temp = new Sprite();
                temp.LoadContent(Content, @"Sprites/head");
                temp.Scale = .2f;
                Sprite temp2 = new Sprite();
                temp2.LoadContent(Content, @"Sprites/head");
                temp2.Scale = .2f;
                button.LoadContent(Content,@"Sprites/button" + button.text);
                button.loadHead(temp,temp2);
                alternator = !alternator;
            }
        }

        public void Update(MouseState ms, KeyboardState ks, MoonPrison game)
        {
            Vector2 mousePos = new Vector2(ms.X, ms.Y);
            cross.Update(ms);

            for (int i = 0; i < buttonList.Length; i++)
            {
                if (cross.collides(buttonList[i]))
                {
                    buttonList[i].mouseOver = true;
                }
                else
                {
                    buttonList[i].mouseOver = false;
                }
                buttonList[i].Update(0);
            }

            if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Boolean onButton = false;
                string name = "";
                for (int i = 0; i < buttonList.Length; i++ )
                {
                    if (cross.collides(buttonList[i]))
                    {
                        onButton = true;
                        name = buttonList[i].text;
                    }
                }

                if (onButton)
                {
                    if (name == "Quit")
                    {
                        game.Exit();
                    }
                    else if (name == "Save")
                    {
                        fileS.ShowDialog();
                        if (fileS.FileName != "")
                        {
                            game.saveGame(fileS.FileName);
                            this.isDisplayed = false;
                        }
                    }
                    else if (name == "Load")
                    {
                        fileO.ShowDialog();
                        if (fileO.FileName != "")
                        {
                            game.loadGame(fileO.FileName);
                            this.isDisplayed = false;
                            game.hasEaster = false;
                            game.playNextSong();
                        }
                    }
                    else if (name == "JumptoLevel")
                    {
                        fileL.ShowDialog();
                        if (fileL.FileName != "" && fileL.FileName.Contains("."))
                        {
                            fileL.FileName = fileL.FileName.Remove(fileL.FileName.IndexOf("."));
                            game.level = FileIOManager.loadLevel(fileL.FileName,game.Content);
                            game.level.LoadContent(game.Content);
                            game.linearOrder = false;
                            game.hasEaster = false;
                            this.isDisplayed = false;
                            game.playNextSong();
                        }
                    }
                    else if (name == "StartGame")
                    {
                        game.level = FileIOManager.loadLevel(StorageContainer.TitleLocation + FileIOManager.LEVEL_PREFIX + game.LEVEL_ORDER[0],game.Content);
                        game.level.LoadContent(game.Content);
                        game.levelNumber = 0;
                        game.linearOrder = true;
                        game.hasEaster = false;
                        this.isDisplayed = false;
                        game.playNextSong();
                    }
                }
            }
        }

        //draws the buttons a ways apart from each other
        public void Draw(SpriteBatch spritebatch)
        {
        
            spritebatch.Begin();
            //spritebatch.DrawString(font, "Welcome to the Moon Prison", new Vector2((Level.ScreenW / 2 - .5f * font.MeasureString("Welcome to the Moon Prison").X), 0), Color.AntiqueWhite);
            for (int i = 0; i < buttonList.Length; i++)
            {
                float startX = (Level.ScreenW / 2) - (buttonList[i].getWidth() / 2);
                buttonList[i].position = new Vector2(startX + buttonList[i].getWidth() / 2, BUTTON_START_POINT + i * (buttonList[i].getHeight() + BUTTON_SPACER));
                buttonList[i].bound = new BoundingBox(new Vector3(buttonList[i].position.X - buttonList[i].getWidth() / 2, buttonList[i].position.Y - buttonList[i].getHeight() / 2, -1), new Vector3(buttonList[i].position.X + buttonList[i].getWidth() / 2, buttonList[i].position.Y + buttonList[i].getHeight() / 2, 1));
                buttonList[i].Draw(spritebatch, Vector2.Zero);
            }
            title.Draw(spritebatch, Vector2.Zero);
            cross.Draw(spritebatch, Vector2.Zero);
            spritebatch.End();
        }

    }

    class MenuButton : Sprite
    {
        public string text;
        public bool mouseOver = false;
        public Sprite head;
        public Sprite head2;
        private bool headLoaded;
        private float rotationalVelocity = 0;
        private bool right;
        private bool displayHead = false;

        public MenuButton(string Text)
        {
            text = Text;
        }

        public void loadHead(Sprite _head, Sprite _head2)
        {
            head = _head;
            head2 = _head2;
            head.position.X = position.X - this.size()/2 - head.size()/2 ;
            head2.position.X = position.X + this.size() / 2 + head2.size() / 2;
            head.position.Y = position.Y;
            head2.position.Y = position.Y;
            
        }

        public void Update(float dt)
        {

            if (mouseOver)
            {
                rotationalVelocity = .2f;
                displayHead = true;
                head.tint = Color.White;
                head2.tint = Color.White;
            }
            else if(rotationalVelocity < .05)
            {
                displayHead = false;
            }
            rotationalVelocity *= .95f;
            head.tint = new Color(head.tint.ToVector3()*.92f);
            head2.tint = new Color(head2.tint.ToVector3() * .92f);
            head.rotation += rotationalVelocity;
            head2.rotation += rotationalVelocity;
            base.Update(dt);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            if (!headLoaded)
            {
                loadHead(head, head2);
                headLoaded = true;
            }
            if (displayHead)
            {
                head.Draw(spriteBatch, Vector2.Zero);
                head2.Draw(spriteBatch, Vector2.Zero);
            }
            base.Draw(spriteBatch, origin);
            
        }
    }
}
