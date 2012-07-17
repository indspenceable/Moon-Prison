using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoonPrison.Particle_Engine
{

    public class Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float life;
        public Color color;

        public Particle(Vector2 _pos, float _life, Color _color)
        {
            position = _pos;
            life = _life;
            color = _color;
        }


        public virtual void update(float dt)
        {
            life -= dt;
        }

        public bool isDone()
        {
            return life < 0;
        }
        public void draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            //ParticleEngine.particleSprite.position = position;
            //ParticleEngine.particleSprite.Draw(spriteBatch, origin);
            //spriteBatch.Draw(ParticleEngine.particleTexture, position-origin, ParticleEngine.sourceRect, color, 0, ParticleEngine.textureOrigin, 0.5f, SpriteEffects.None, 0); 
                
                
           spriteBatch.Draw(ParticleEngine.particleTexture, position-origin-(new Vector2(ParticleEngine.particleTexture.Width, ParticleEngine.particleTexture.Height)/2), color);
        }
    }
    public class FadingParticle : Particle
    {
        float duration;
        public FadingParticle(Vector2 _pos, float _life, Color _color)
            : base(_pos, _life, _color)
        {
            duration = _life;
        }
        public override void update(float dt)
        {
            base.update(dt);
            color.A -= (byte)(255*(1 - dt / duration)); 
        }
    }
    public class GatherParticle: Particle
    {
        //how long it's been alive
        public float duration;

        public GatherParticle(Vector2 _pos, float _life, Color _color, Vector2 _gatherTarget): base(_pos,_life,_color)
        {
            gatherTarget = _gatherTarget;
        }
        private Vector2 gatherTarget;
        public override void update( float dt ) {
            life -= dt;
            duration += dt;
            velocity = (gatherTarget - position) * (duration / life);
            position += velocity;
        }
    }
    public class FallingParticle : Particle
    {
        public FallingParticle(Vector2 _pos, float _life, Color _color) : base(_pos, _life, _color) { }
        public override void update(float dt)
        {
            life -= dt;
            position.Y += dt*50;
        }
    }
    public class ScatterParticle: Particle {
        public ScatterParticle(Vector2 _pos, float _life, Color _color): base(_pos,_life,_color) {}
        public override void update(float dt)
        {
            life -= dt;
            switch (ParticleEngine.r.Next(4))
            {
                case 0: position.X += dt*20; break;
                case 1: position.Y += dt*20; break;
                case 2: position.X -= dt*20; break;
                case 3: position.Y -= dt*20; break;
            }
        }
    }

    public class ExplodeParticle: Particle {
        public ExplodeParticle(Vector2 _pos, float _life, Color _color): base(_pos,_life,_color) {}

        public override void update (float dt)
        {
            life -= dt;
            if (velocity == Vector2.Zero)
                {
                    double angle = ParticleEngine.r.NextDouble() * 2 * Math.PI;
                    velocity.X = (float)((ParticleEngine.r.NextDouble() * 100 + 50) * Math.Cos(angle));
                    velocity.Y = (float)((ParticleEngine.r.NextDouble() * 100 + 50) * Math.Sin(angle));
                }
                position += velocity * dt;
        }
    }
}
