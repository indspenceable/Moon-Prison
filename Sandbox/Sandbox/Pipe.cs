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
    //Ceci ne pas une pipe
    class Pipe
    {
        public const int UP_RIGHT_BEND = 0;
        public const int HORIZ_STRAIGHT = 1;
        public const int DOWN_T = 2;
        public const int UP_LEFT_BEND = 4;
        public const int VERT_STRAIGHT = 5;
        public const int DOWN_END = 6;
        public const int RIGHT_T = 7;
        public const int LEFT_END = 8;
        public const int CROSS = 11;
        public const int LEFT_T = 14;
        public const int UP_END = 16;
        public const int RIGHT_END = 18;
        public const int RIGHT_UP_BEND = 20;
        public const int UP_T = 22;
        public const int LEFT_UP_BEND = 24;

        public static int[] DOWN_CONNECTORS = { UP_RIGHT_BEND, UP_LEFT_BEND, DOWN_T, LEFT_T, RIGHT_T, VERT_STRAIGHT, CROSS };
        public static int[] UP_CONNECTORS = { LEFT_UP_BEND, RIGHT_UP_BEND, UP_T, LEFT_T, RIGHT_T, VERT_STRAIGHT, CROSS };
        public static int[] RIGHT_CONNECTORS = { UP_RIGHT_BEND, RIGHT_UP_BEND, DOWN_T, UP_T, RIGHT_T, HORIZ_STRAIGHT, CROSS };
        public static int[] LEFT_CONNECTORS = { UP_LEFT_BEND, LEFT_UP_BEND, DOWN_T, UP_T, LEFT_T, HORIZ_STRAIGHT, CROSS };
        public static int[][] ALL_CONNECTORS = { DOWN_CONNECTORS, UP_CONNECTORS, RIGHT_CONNECTORS, LEFT_CONNECTORS };

        public int x; public int y; //Coordinates in (pipe) tiles
        int type;

        public Pipe(int _x, int _y, int _type)
        {
            x = _x;
            y = _y;
            type = _type;
        }

        //returns the relevant endpiece
        public static int getEndPiece(int direction)
        {
            if (direction == PipeEngine.LEFT)
            {
                return LEFT_END;
            }
            if (direction == PipeEngine.RIGHT)
            {
                return RIGHT_END;
            }
            if (direction == PipeEngine.UP)
            {
                return UP_END;
            }
            return DOWN_END;
        }

        public Rectangle getSource()
        {
            int x = type % 5;
            int y = type / 5;
            return new Rectangle(x * 50, y * 50, 50, 50);
        }

        public Rectangle getDest(Vector2 origin)
        {
            return new Rectangle(x * PipeEngine.TILE_WIDTH - (int)(.9 *origin.X), y * PipeEngine.TILE_HEIGHT - (int)(.9*origin.Y), PipeEngine.TILE_WIDTH, PipeEngine.TILE_HEIGHT);
        }
    }
}
