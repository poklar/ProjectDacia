using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.WorldEngine;

namespace Engine.Common
{
    public class DebugInfo
    {
        private Game _game;
        private ContentManager _content;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private static string _data;

        private World _world;

        public DebugInfo(Game game, World world)
        {
            _game = game;
            _content = game.Content;
            _world = world;;
        }

        public static string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            _spriteFont = _content.Load<SpriteFont>("Fonts/debug");
        }

        public void Update(GameTime gameTime)
        {
            Draw(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.DrawString(_spriteFont, "Floor: " + _world.CURRENTMAPLEVEL, new Vector2(33, 60), Color.Black);
            _spriteBatch.DrawString(_spriteFont, "Floor: " + _world.CURRENTMAPLEVEL, new Vector2(32, 59), Color.White);

            _spriteBatch.DrawString(_spriteFont, "Chunks: " + _world.RegionsDrawn, new Vector2(33, 75), Color.Black);
            _spriteBatch.DrawString(_spriteFont, "Chunks: " + _world.RegionsDrawn, new Vector2(32, 74), Color.White);

            Vector2 index = new Vector2(33, 90);

            if (_data != null)
            {
                _spriteBatch.DrawString(_spriteFont, _data, index, Color.Black);
                _spriteBatch.DrawString(_spriteFont, _data, new Vector2(index.X - 1, index.Y - 1), Color.White);
                //index.Y += 15;
            }

            _spriteBatch.End();
        }
    }
}
