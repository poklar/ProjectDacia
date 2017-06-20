﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Engine;
using Engine.Cameras;

namespace Engine.Components
{
    public abstract class TechEngineComponent
    {
        private TechCraftGame _game;

        public TechEngineComponent(TechCraftGame game)
        {
            _game = game;
        }

        public TechCraftGame Game
        {
            get { return _game; }
        }

        public abstract void Initialize();

        public abstract void LoadContent();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);
    }
}
