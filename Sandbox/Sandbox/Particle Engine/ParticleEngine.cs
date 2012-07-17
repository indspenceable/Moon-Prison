using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MoonPrison.Particle_Engine
{
    class ParticleEngine
    {
        private static LinkedList<Particle> particles = new LinkedList<Particle>();
        private static LinkedList<Emitter> emitters = new LinkedList<Emitter>();
        public static Random r = new Random();
        //public static Rectangle sourceRect = new Rectangle(0, 0, 3, 3);
        //public static Vector2 textureOrigin = new Vector2(1, 1);

        public static Texture2D particleTexture;// = new Texture2D();

        public static void reset() {
            particles = new LinkedList<Particle>();
            emitters = new LinkedList<Emitter>();
        }

        public static void addEmitter(Emitter e)
        {
            emitters.AddLast(e);
        }

        public static void loadContent(ContentManager content)
        {
            //particleSprite.LoadContent(content,@"sprites/particle");// = content.Load<Texture2D>("@sprites/particle");
            particleTexture = content.Load<Texture2D>(@"sprites/particle");
        }

        public static void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            foreach (Particle p in particles)
            {
                p.draw(spriteBatch, origin);
            }
//            spriteBatch.Draw(particleSprite,position - Origin,
//                            Source, tint, rotation, origin, Scale,
//                            SpriteEffects.None, 0);
        }

        public static void removeEmmiter(Emitter e)
        {
            emitters.Remove(e);
        }

        public static void clearEmitters()
        {
            emitters = new LinkedList<Emitter>();
        }

        public static void update(float dt)
        {
            LinkedList<Emitter> deadEmitters = new LinkedList<Emitter>();
            foreach (Emitter e in emitters)
            {
                e.emit(dt, particles);
                if (e.isDone())
                {
                    deadEmitters.AddLast(e);
                }
            }
            foreach (Emitter e in deadEmitters)
            {
                emitters.Remove(e);
            }


            LinkedList<Particle> deadParticles = new LinkedList<Particle>();
            foreach (Particle p in particles)
            {
                p.update(dt);
                if (p.isDone())
                {
                    deadParticles.AddFirst(p);
                }
            }
            foreach (Particle p in deadParticles)
            {
                particles.Remove(p);
            }
        }
    }
}
