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
using MoonPrison.Particle_Engine;
namespace MoonPrison
{
    public class Bullet : Sprite
    {
        public Boolean isVisible;
        private float age;

        //hooray for constants!
        public const int BULLET_MAX_SPEED = 450;
        private const float BULLET_SCALE = .2f;
        private const float BULLET_MAX_AGE = 1.2f;

        public Bullet(Vector2 position) : base(position)
        {
            this.maxSpeed = BULLET_MAX_SPEED;
            this.isVisible = false;
            this.Scale = BULLET_SCALE;
            age = 0;
        }

        public new void Update(float dt)
        {
            if (this.isVisible)
            {
                age += dt;
                if (age > BULLET_MAX_AGE)
                {
                    this.isVisible = false;
                    this.age = 0;
                }
                for (float a = 0; a < 1; a+=0.1f)
                {
                    Particle_Engine.ParticleEngine.addEmitter(new BulletSmokeEmitter(position + a*dt*velocity));
                }

                base.Update(dt);
            }
        }

    }
}
