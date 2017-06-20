using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using TechCraftEngine;

namespace TechCraftEngine.Controllers
{
    public abstract class Controller
    {
        private TechCraftGame _game;

        public Controller(TechCraftGame game)
        {
            _game = game;
        }

        public TechCraftGame Game
        {
            get { return _game; }
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update(GameTime gameTime)
        {

        }
    }
}
