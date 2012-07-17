using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelEditor
{
    public class Enemy
    {
        public int num;
        public int x; public int y;
        public const int WEAPON_DEFAULT = 1;
        public const float PATROL_RANGE = 4 * 48;
        public const int PATROLSPEED = 50;
        public const int VERT_VIEW_RANGE = 50;
        public const int HORIZ_VIEW_RANGE = 300;
        public const int SOUND_RANGE = 500;
        public const int INVESTIGATE_MAX_WAIT = 5;
        public const float MIN_FIRE_INTERVAL = .2f;
        public const float LAZER_DURATION = 5f;
        public const float LAZER_CHARGE_TIME = 2.5f;

        public int weaponType = WEAPON_DEFAULT;
        public float patrolRange = PATROL_RANGE;
        public int patrolspeed = PATROLSPEED;
        public int vertViewRange = VERT_VIEW_RANGE;
        public int horizViewRange = HORIZ_VIEW_RANGE;
        public int soundRange = SOUND_RANGE;
        public int investigateMaxWait = INVESTIGATE_MAX_WAIT;
        public float fireInterval = MIN_FIRE_INTERVAL;
        public float lazerDuration = LAZER_DURATION;
        public float lazerChargeTime = LAZER_CHARGE_TIME;

        public Enemy(int _x, int _y, int _num)
        {
            x = _x; y = _y; num = _num;
        }


    }
}
