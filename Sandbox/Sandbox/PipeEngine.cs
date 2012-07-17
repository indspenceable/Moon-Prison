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
    class PipeEngine
    {

        int height; //height of the space to fill
        int width; //width etc.
        int numPipes; //number of pipes to place
        float density; //number between 1 and 0 representing how clustered the pipes should be
        private int placedPipes = 0;
        Random rand = new Random();
        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public const int TILE_HEIGHT = 25;
        public const int TILE_WIDTH = 25;

        LinkedList<Pipe> pipes = new LinkedList<Pipe>();
        Texture2D texture;

        public PipeEngine(int _height, int _width, int _numPipes, float _density)
        {

            height = _height;
            width = _width;
            numPipes = _numPipes;
            density = 1 - _density;

            constructPipes(rand.Next(width), rand.Next(height), Pipe.ALL_CONNECTORS[rand.Next(4)], -1);
            constructPipes(rand.Next(width), rand.Next(height), Pipe.ALL_CONNECTORS[rand.Next(4)], -1);
            constructPipes(rand.Next(width), rand.Next(height), Pipe.ALL_CONNECTORS[rand.Next(4)], -1);
            /*for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    pipes.AddLast(new Pipe(j + 1, i + 1, Pipe.ALL_CONNECTORS[i][j]));
                }
            }*/

        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(@"Sprites/pipes");
        }

        private void constructPipes(int x, int y, int[] posTypes, int backwards)
        {
            
            if(placedPipes > numPipes || x > width || x < 0 || y > height || y < 0 || hasPipeAt(x,y)){
                pipes.AddFirst(new Pipe(x, y, Pipe.getEndPiece(backwards)));
                return;
            }

            int r = rand.Next(posTypes.Length);

            int piece = posTypes[r];
            pipes.AddFirst(new Pipe(x, y, piece));
            placedPipes++;

            if (piece == Pipe.VERT_STRAIGHT || piece == Pipe.HORIZ_STRAIGHT)
            {
                Vector2 change = new Vector2(0, 0);
                int baseline = (int)(density * 5);
                int limit = baseline + rand.Next(baseline);
                if (piece == Pipe.VERT_STRAIGHT)
                {
                    if (backwards == DOWN)
                    {
                        change = new Vector2(0, -1);
                    }
                    else if (backwards == UP)
                    {
                        change = new Vector2(0, 1);
                    }
                }
                else if (piece == Pipe.HORIZ_STRAIGHT)
                {
                    if (backwards == LEFT)
                    {
                        change = new Vector2(1, 0);
                    }
                    else
                    {
                        change = new Vector2(-1, 0);
                    }
                }
                //add in the extra pieces
                for (int i = 0; i < limit; i++)
                {
                    x = x + (int)change.X;
                    y = y + (int)change.Y;
                    pipes.AddFirst(new Pipe(x, y, piece));
                }
            }
            int choose = rand.Next(4);
            int count = 0;
            while(count < 4){

            //make the recursive calls
            if(Array.IndexOf(Pipe.LEFT_CONNECTORS,piece) != -1 && backwards != LEFT && choose == LEFT){
                constructPipes(x - 1, y, Pipe.RIGHT_CONNECTORS, RIGHT);
            }
            if(Array.IndexOf(Pipe.RIGHT_CONNECTORS,piece) != -1 && backwards != RIGHT && choose == RIGHT){
                constructPipes(x + 1, y, Pipe.LEFT_CONNECTORS, LEFT);
            }
            if(Array.IndexOf(Pipe.UP_CONNECTORS,piece) != -1 && backwards != UP && choose == UP){
                constructPipes(x, y - 1, Pipe.DOWN_CONNECTORS, DOWN);
            }
            if(Array.IndexOf(Pipe.DOWN_CONNECTORS,piece) != -1 && backwards != DOWN && choose == DOWN){
                constructPipes(x, y + 1, Pipe.UP_CONNECTORS, UP);
            }

            choose = (choose + 1) % 4;
            count++;
            }


           
        }

        public void Draw(SpriteBatch sb, Vector2 origin)
        {

            foreach (Pipe p in pipes)
            {
                sb.Draw(texture, p.getDest(origin), p.getSource(), Color.White);
            }
        }

        bool hasPipeAt(int x, int y)
        {
            foreach (Pipe p in pipes)
            {
                if (p.x == x && p.y == y)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
