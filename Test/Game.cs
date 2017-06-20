using System;
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
using Engine.Common;
using Test.States;
//using fbDeprofiler;

namespace Test
{
    public class Game : TechCraftGame
    {
        public Game() {
            //DeProfiler.Run(); 
            FrameRateCounter frameRate = new FrameRateCounter(this);
            frameRate.DrawOrder = 1;
            Components.Add(frameRate);

        }

        protected override void Initialize()
        {
            TitleState _state = new TitleState();
            
            _state.Game = this;
            _state.Initialize();
            _state.LoadContent();
            StateManager.ActiveState = _state;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }
    }
}
