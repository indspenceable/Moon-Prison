using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoonPrison.Particle_Engine
{
    class SparksEmitter : Emitter
    {
        public SparksEmitter(Vector2 pos) : base(pos,0.005f,0.1f) { }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            pl.AddLast(new ExplodeParticle(position, 0.2f, Color.Gold));
        }
    }
    class BulletSmokeEmitter : Emitter
    {
        public BulletSmokeEmitter(Vector2 pos) : base(pos, 0.6f, 0.03f) { }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            pl.AddLast(new ScatterParticle(position, 0.3f, Color.Gray));
        }
    }

 // I have an idea of how to reduce the lag...
    // Assuming its from there being too many particles.

    class ChangingBlockEmitter : Emitter
    {
        float w, h;
        public ChangingBlockEmitter(Vector2 pos, float _w, float _h):
            base(pos, 0.1f, 0)
        {
            w = _w;
            h = _h;
        }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            //top
            for (int a = 0; a < 2; a++)
            {
                pl.AddLast(new Particle(new Vector2(position.X - (w / 2) + 
                    ((float)ParticleEngine.r.NextDouble() * w), position.Y - h / 2),0.1f,Color.White));
            }
            //right
            for (int a = 0; a < 2; a++)
            {
                pl.AddLast(new Particle(new Vector2(position.X + (w / 2), position.Y - (h / 2) + 
                    ((float)ParticleEngine.r.NextDouble() * h)),0.1f,Color.White));
            }
            //down
            for (int a = 0; a < 2; a++)
            {
                pl.AddLast(new Particle(new Vector2(position.X - (w / 2) + 
                    ((float)ParticleEngine.r.NextDouble() * w), position.Y + h / 2),0.1f,Color.White));
            }
            //left
            for (int a = 0; a < 2; a++)
            {
                pl.AddLast(new Particle(new Vector2(position.X - (w / 2), position.Y - (h / 2) + 
                    ((float)ParticleEngine.r.NextDouble() * h)),0.1f,Color.White));
            }
        }
    }

    class LazerBlockEmitter : Emitter
    {
        public static int numberOfEmittersDisplayed = 0;
        private float w, h;
        public LazerBlockEmitter(Vector2 pos, float _w, float _h)
            : base(pos, 0.5f, 0)
        {
            w = _w;
            h = _h;
            //numberOfEmittersDisplayed++;
        }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            if ( true )
            {
                Color c;
                switch (Particle_Engine.ParticleEngine.r.Next(5))
                {
                    case (0): c = Color.Aqua; break;
                    case (1): c = Color.Blue; break;
                    case (2): c = Color.Purple; break;
                    case (3): c = Color.LightSkyBlue; break;
                    case (4): c = Color.LightCyan; break;
                    default: c = Color.Red; break;
                }
                pl.AddLast(new Particle(position + new Vector2((float)(ParticleEngine.r.NextDouble() - 0.5) * w, (float)(ParticleEngine.r.NextDouble() - 0.5) * h), 0.1f, c));
            }
        }
    }

    class PortalBlockEmitter : Emitter
    {
        Vector2 tileSize;
        public PortalBlockEmitter(Vector2 pos,float _w, float _h)
            : base(pos, 0.1f, 0.0f)
        {
            tileSize.X = _w;
            tileSize.Y = _h;
        }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            Vector2 offset = new Vector2();
            for (float a = -0.5f; a < 0.5; a += 0.1f)
            {
                offset.X = (float) Particle_Engine.ParticleEngine.r.NextDouble() - 0.5f;
                offset.Y = (float)Particle_Engine.ParticleEngine.r.NextDouble() - 0.5f;
                pl.AddLast(new FallingParticle(position + offset*10 + new Vector2(tileSize.X * a, -tileSize.Y), 2, Color.Honeydew));
            }
        }
        protected void addParticlesOld(LinkedList<Particle> pl)
        {
            Color c;
            switch (Particle_Engine.ParticleEngine.r.Next(5))
            {
                case (0): c = Color.Aqua; break;
                case (1): c = Color.Blue; break;
                case (2): c = Color.Purple; break;
                case (3): c = Color.LightSkyBlue; break;
                case (4): c = Color.LightCyan; break;
                //default: c = Color.Red; break;
            }
            double rads = Particle_Engine.ParticleEngine.r.NextDouble() * 2 * Math.PI;
            float distance = 40.0f;
            for (float a = 0; a < distance; a += 0.5f)
            {
                pl.AddLast(new Particle(position + new Vector2((float)Math.Cos(rads), (float)Math.Sin(rads))*a, 0.05f, Color.Yellow));
            }
            for (int a = 0; a < 50; a++)
            {
                rads = Particle_Engine.ParticleEngine.r.NextDouble() * 2 * Math.PI;
                pl.AddLast(new ScatterParticle(position + new Vector2((float)Math.Cos(rads), (float)Math.Sin(rads)) * (float)Particle_Engine.ParticleEngine.r.NextDouble() * distance, 0.03f, Color.Green));
            }
        }
    }


    class ExplosionEmitter : Emitter
    {
        public ExplosionEmitter(Vector2 pos) : base(pos, 0.001f, 0.3f) { }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            pl.AddLast(new ExplodeParticle(position, 1.0f, Color.Red));
        }
    }
    class LazerChargeEmitter : Emitter
    {
        float chargeDuration;
        public LazerChargeEmitter(Vector2 pos, float chargeDuration)
            : base(pos, 0.0005f, chargeDuration)
        {
            this.chargeDuration = chargeDuration;
        }
        protected override void addParticles(LinkedList<Particle> pl)
        {
            Vector2 temp;
            double angle = ParticleEngine.r.NextDouble() * 2 * Math.PI;
            temp.X = (float)((ParticleEngine.r.NextDouble() * 300) * Math.Cos(angle));
            temp.Y = (float)((ParticleEngine.r.NextDouble() * 300) * Math.Sin(angle));
            pl.AddLast(new GatherParticle(position - temp, chargeDuration, Color.Red, position));
        }
    }
    abstract class Emitter
    {
        public Vector2 position;
        //private int type;

        private float cooldown;
        private float currentTime;
        protected float life;

        protected Emitter(Vector2 _pos, float _cooldown, float _life)
        {
            position = _pos;
            cooldown = _cooldown;
            life = _life;
        }

        public void emit(float dt, LinkedList<Particle> pl)
        {
            life -= dt;
            currentTime = currentTime - dt;
            while (currentTime < 0)
            {
                currentTime += cooldown;
                //pl.AddLast(new Particle(/**/));
                addParticles(pl);
            }
        }

        protected abstract void addParticles(LinkedList<Particle> pl);

        public bool isDone()
        {
            return life < 0;
        }
    }
}
