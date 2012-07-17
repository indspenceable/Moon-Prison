using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace MoonPrison
{
    public class Enemy : Sprite
    {
 
        //booleans, pretty self-explanatory
        public Boolean isDead;
        public Boolean isPatrolling;
        public Boolean isInvestigating;
        public Boolean canFallWhilePatrolling;
        private Boolean hasStartedLazer;
        public Boolean shouldFire;

        public Sprite exlcamationPoint;
        private Boolean drawExclamationPoint;
        public Sprite questionMark;
        private Boolean drawQuestionMark;
        private const float QUESTION_SCALE = .4f; 

        public Beam lazerBeam;

        //press 1 for bullets, 2 for lazers!
        public const int WEAPON_DEFAULT = 1;
        public int weaponType;

        //patrol range is the distance (in pixels) that the guard will travel left or right
        public const float PATROL_RANGE = 4 * Level.tileWidth;
        public float patrolRange;

        //the speed at which the guard will move, right now, for all movement
        public const int PATROLSPEED = 50;
        public int patrolspeed;

        //the distance up or down in which the enemy can see the player.  IN PIXELS
        public const int VERT_VIEW_RANGE = 50;
        public int vertViewRange;

        //distance left of right for which the enemy can see the player. IN PIXELS
        public const int HORIZ_VIEW_RANGE = 300;
        public int horizViewRange;

        //the range at which a guard can hear noise IN PIXELS
        public const int SOUND_RANGE = 500;
        public int soundRange;

        //INVESTIGATE_MAX_WAIT is how long (in seconds) waitTimer will count for before the guard resumes patrolling
        public const int INVESTIGATE_MAX_WAIT = 5;
        public int investigateMaxWait;

        //fire delays, in seconds (?)
        public const float MIN_FIRE_INTERVAL = .2f;
        public float fireInterval;
        public const float LAZER_DURATION = 5f;
        public float lazerDuration;
        public const float LAZER_CHARGE_TIME = 2.5f;
        public float lazerChargeTime;

        private Boolean facingRight;

        private Particle_Engine.LazerChargeEmitter emitter; 

        /*
         * non - customizable variables
         * destination is the source of the sound that the guard should get as close to as possible
         * waitTimer counts how long the guard has been investigating, once it gets there
         * patrolStart keeps track of where the enemey started patrolling
         * timeSinceLastFire is used to determine when enemies shoot
         */
        private Vector2 destination;
        private float waitTimer;
        //the start point is needed to determine distances
        private float patrolStart;
        private float timeSinceLastFire;

        //assumes square tiles and enemies.  makes the player 1/2 of the size of a tile
        public const float ENEMYSCALE = (float)Level.tileHeight / 200;

        //basic constructor at some point more of these variables will be XML read and passed in instead of hard-coded.
        public Enemy(int startX, int startY) : base(new Vector2(startX, startY)) 
        {
            this.Scale = ENEMYSCALE;
            this.lazerBeam = new Beam();
            this.exlcamationPoint = new Sprite();
            exlcamationPoint.Scale = .4f;
            questionMark = new Sprite();
            questionMark.Scale = QUESTION_SCALE;
            this.isDead = false;
            this.isPatrolling = true;
            this.isInvestigating = false;
            this.canFallWhilePatrolling = false;
            this.hasStartedLazer = false;
            this.onGround = false;
            this.maxSpeed = 200f;
            this.patrolStart = startX;
            this.velocity.X = (float) PATROLSPEED;
            timeSinceLastFire = 0;
            this.patrolspeed = PATROLSPEED;
            this.patrolRange = PATROL_RANGE;
            this.vertViewRange = VERT_VIEW_RANGE;
            this.horizViewRange = HORIZ_VIEW_RANGE;
            this.soundRange = SOUND_RANGE;
            this.investigateMaxWait = INVESTIGATE_MAX_WAIT;
            this.lazerChargeTime = LAZER_CHARGE_TIME;
            this.lazerDuration = LAZER_DURATION;
            this.fireInterval = MIN_FIRE_INTERVAL;
            this.weaponType = WEAPON_DEFAULT;
            emitter = new Particle_Engine.LazerChargeEmitter(this.position, lazerChargeTime);
        }

        //update things.  will eventually check line-of-sight, shooting, and walking
        public int Update(float dt, Player player, Bullet[] bullets,Tile[,] layout, Boolean noise,Boolean fallNoise, Tile[] nearby)
        {
            Vector2 playerPosition = player.position;
            int retval = -1;
            //if the guard is patrolling, let it patrol            
            if (this.isPatrolling)
            {
                drawQuestionMark = false;
                if (patrolRange == 0 && Math.Abs(this.position.X - patrolStart) <= 5)
                {
                    velocity.X = 0;
                }
                else if (position.X >= patrolStart + patrolRange || this.collideRight)
                {
                    velocity.X = -patrolspeed;
                }
                else if (position.X <= patrolStart - patrolRange || this.collideLeft)
                {
                    velocity.X = patrolspeed;
                }

            }
            //if it can see the player, shoot at it!
            if (this.canSeePlayer(playerPosition, layout))
            {
                drawQuestionMark = false;
                drawExclamationPoint = true;   
                //dont resume patrol if we can see the player!
                waitTimer = 0;
                if (weaponType == 1)
                {
                    timeSinceLastFire += dt;
                    if (timeSinceLastFire > fireInterval)
                    {
                        fireBullet(this.position - playerPosition, bullets);
                        timeSinceLastFire = 0;
                        retval = 1;
                    }
                }
                else if (weaponType == 2)
                {
                    if (!hasStartedLazer)
                    {
                        hasStartedLazer = true;
                        timeSinceLastFire = 0;
                        Particle_Engine.ParticleEngine.removeEmmiter(emitter);
                        emitter = new Particle_Engine.LazerChargeEmitter(this.position, lazerChargeTime);
                        Particle_Engine.ParticleEngine.addEmitter(emitter);
                    }
                    else
                    {
                        timeSinceLastFire += dt;
                        velocity.X = 0;
                        if (timeSinceLastFire > lazerChargeTime)
                        {
                            shouldFire = true;
                        }
                        if (timeSinceLastFire + 1 > lazerChargeTime) retval = 2;
                    }
                }
                isPatrolling = false;
                isInvestigating = true;
                destination = playerPosition;
            }
            else
            {
                if (hasStartedLazer) Particle_Engine.ParticleEngine.removeEmmiter(emitter);
                shouldFire = false;
                drawExclamationPoint = false;
                hasStartedLazer = false;
                timeSinceLastFire = fireInterval + LAZER_CHARGE_TIME + 100;
            }
            //if there is noise while patrolling
            if (noise)
            {
                Vector2 distance = player.theHook.position - this.position;
                //if the guard can actually hear it
                if (distance.Length() < soundRange)
                {
                    waitTimer = 0;
                    this.isPatrolling = false;
                    this.isInvestigating = true;
                    drawQuestionMark = true;
                    this.destination = player.theHook.position;
                }
            }
            if (fallNoise)
            {
                Vector2 distance = player.position - this.position;
                //if the guard can actually hear it
                if (distance.Length() < soundRange)
                {
                    waitTimer = 0;
                    this.isPatrolling = false;
                    drawQuestionMark = true;
                    this.isInvestigating = true;
                    this.destination = player.position;
                }
            }
            //if we're investigating something (like a noise, say)
            if (this.isInvestigating)
            {
                drawQuestionMark = true;
                Vector2 distance = this.destination - this.position;
                //if we're close or if the next tile isn't solid, stop or if we seem to have gotten stuck
                if (Math.Abs(distance.X) <= 5 || !this.nextTileIsSolid(nearby) || collideLeft || collideRight)
                {
                    this.velocity.X = 0;
                    waitTimer += dt;
                    if (waitTimer > investigateMaxWait)
                    {
                        isInvestigating = false;
                        drawQuestionMark = false;
                        isPatrolling = true;
                        this.velocity.X = -patrolspeed * ((this.position.X - this.patrolStart) / (Math.Abs(this.position.X - this.patrolStart)));
                        waitTimer = 0;
                    }
                }
                //otherwise continue (or start) moving at PATROLSPEED towards the noise
                else if (distance.X * velocity.X <= 0 && !hasStartedLazer)
                {
                    this.velocity.X = patrolspeed * (distance.X / Math.Abs(distance.X));
                }
            }
            if (this.hasStartedLazer)
            {
                if (timeSinceLastFire - lazerChargeTime >= lazerDuration)
                {
                    timeSinceLastFire = 0;
                    this.hasStartedLazer = false;
                }
            }
            if (velocity.X < -.1f) facingRight = false;
            if (velocity.X > .1f) facingRight = true;
            if (!nextTileIsSolid(nearby))
            {
                this.velocity.X *= -1;
            }
            applyGravity();
            base.Update(dt);
            return retval;
        }

        private Boolean nextTileIsSolid(Tile[] nearby)
        {
            Boolean shouldMove = true;
            if (nearby.Length >= 9)
            {
                Tile nextTile = getNextTile(nearby);
                shouldMove = nextTile.isSolid;
            }
            return shouldMove;
        }

        //gets the next tile that the enemy will be walking on - assumes that motion is left to right ONLY
        private Tile getNextTile(Tile[] nearby)
        {
            if (velocity.X > 0 || (this.velocity == Vector2.Zero && (this.destination.X - this.position.X) > 0))
            {
                return nearby[8];
            }
            else if (velocity.X < 0 || (this.velocity == Vector2.Zero && (this.destination.X - this.position.X < 0)))
            {
                return nearby[2];
            }
            else //velocity is zero, choose either
            {
                if (!nearby[2].isSolid) return nearby[2];
                else return nearby[8];
            }
        }

        //determine whether or not the enemy has a direct line of sight to the player
        private Boolean canSeePlayer(Vector2 playerPosition, Tile[,] layout)
        {
            Vector2 distance = this.position - playerPosition;
            //if the player is within the viewing corridor and isn't too far away
            if ((Math.Abs(this.position.Y - playerPosition.Y) <= vertViewRange) && isFacingPlayer(distance) && distance.Length() <= horizViewRange)
            {
                Line meToPlayer = new Line(this.position, playerPosition);
                foreach (Tile block in layout)
                {
                    if (block.isSolid)
                    {
                        Vector2 tilesize = new Vector2(Level.tileWidth, Level.tileHeight);
                        Line diag1 = new Line(block.position - tilesize /2, block.position + tilesize / 2);
                        Line diag2 = new Line(new Vector2(block.position.X - Level.tileWidth /2, block.position.Y + Level.tileHeight / 2), new Vector2(block.position.X + Level.tileWidth / 2, block.position.Y - Level.tileHeight / 2));
                        if (Intersects(meToPlayer, diag1) || Intersects(meToPlayer, diag2))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        //does the actual firing
        private void fireBullet(Vector2 ray, Bullet[] bullets)
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                if (!bullets[i].isVisible)
                {
                    bullets[i].position = this.position;
                    bullets[i].velocity = -1 * Bullet.BULLET_MAX_SPEED * Vector2.Normalize(ray);
                    bullets[i].isVisible = true;
                    break;
                }
            }
        }

        //check to see if the enemy is moving (and therefore facing) in the direction of the player
        private Boolean isFacingPlayer(Vector2 distance)
        {
            //return true if and only if we are pointing in the same direction as the vector between us and the player
            //this is its own method in part to make the line above a little shorter, and in part because we may want to know this at other times
            return ((distance.X > 0 && velocity.X <= 0) || (distance.X < 0 && velocity.X >= 0)) ;
        }

        //enemies need gravity too
        private void applyGravity()
        {
            if (!this.onGround)
            {
                velocity.Y += 12;
            }
            else
            {
                velocity.Y = 0;
            }
        }


        public void Draw(SpriteBatch spriteBatch, Vector2 Origin, Player player)
        {
            if (drawExclamationPoint)
            {
                exlcamationPoint.position = this.position - new Vector2(0, exlcamationPoint.getHeight() + 3);
                exlcamationPoint.Draw(spriteBatch, Origin);
            }
            else if (drawQuestionMark)
            {
                questionMark.position = this.position - new Vector2(0, questionMark.getHeight() + 3);
                questionMark.Draw(spriteBatch, Origin);
            }

            if (shouldFire)
            {
                Vector2 normalMeToHookForReal = Vector2.Normalize(this.position - player.position);
                if (player.position.X < this.position.X)
                {
                    lazerBeam.rotation = (float)Math.Acos(Vector2.Dot(normalMeToHookForReal, Vector2.Normalize(new Vector2(position.X, 0) - new Vector2(position.X, Level.ScreenH))));
                }
                else
                {
                    lazerBeam.rotation = -1f * (float)Math.Acos(Vector2.Dot(normalMeToHookForReal, Vector2.Normalize(new Vector2(position.X, 0) - new Vector2(position.X, Level.ScreenH))));
                }
                lazerBeam.position = (this.position + player.position)/2;
                lazerBeam.DrawScaled(spriteBatch, Origin, new Vector2(Beam.LAZER_WIDTH_SCALE, (player.position - this.position).Length() / lazerBeam.getHeight()));
            }
            if (facingRight) { 
                base.Draw(spriteBatch, Origin);
            } else {
                spriteBatch.Draw(spriteTexture, position - Origin, Source, tint, rotation, origin, Scale,
                    SpriteEffects.FlipHorizontally, 0);
                }

        }


        //line intersection code courtesy of http://forums.xna.com/forums/p/280/1022.aspx
        private Boolean Intersects(Line firstLine, Line secondLine)
        {
            double Ua = ((secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.StartPos.Y - secondLine.StartPos.Y) - (secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.StartPos.X - secondLine.StartPos.X)) /
                    ((secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.EndPos.X - firstLine.StartPos.X) - (secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.EndPos.Y - firstLine.StartPos.Y));

            double Ub = ((firstLine.EndPos.X - firstLine.StartPos.X) * (firstLine.StartPos.Y - secondLine.StartPos.Y) - (firstLine.EndPos.Y - firstLine.StartPos.Y) * (firstLine.StartPos.X - secondLine.StartPos.X)) /
                  ((secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.EndPos.X - firstLine.StartPos.X) - (secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.EndPos.Y - firstLine.StartPos.Y));

            if (Ua >= 0.0f && Ua <= 1.0f && Ub >= 0.0f && Ub <= 1.0f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //my own little line class.  fuck XNA for not having a generic pair class.
    class Line
    {
        public Vector2 StartPos;
        public Vector2 EndPos;

        public Line(Vector2 start, Vector2 end)
        {
            StartPos = start;
            EndPos = end;
        }
    }
}
