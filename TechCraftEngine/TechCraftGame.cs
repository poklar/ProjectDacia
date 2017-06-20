using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TechCraftEngine.Cameras;
using TechCraftEngine.Common;
using TechCraftEngine.Managers;
using TechCraftEngine.Network;

using System.Threading;

namespace TechCraftEngine
{
    public class TechCraftGame : Game
    {
        private StateManager _stateManager;
        private InputState _inputState;
        private Camera _camera;
        private GraphicsDeviceManager _graphics;
        private PlayerIndex _activePlayerIndex;
        private GameClient _gameClient;

        private List<Thread> _threads;

        public TechCraftGame()
            : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.IsFullScreen = false;
            
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.PreparingDeviceSettings += PrepareDeviceSettings;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferMultiSampling = false;

            //_gamerServices = new GamerServicesComponent(this);
            //this.Components.Add(_gamerServices);

            Content.RootDirectory = "Content";

            _threads = new List<Thread>();
            _stateManager = new StateManager(this);
            _inputState = new InputState();
        }

        public GameClient GameClient
        {
            get { return _gameClient; }
            set { _gameClient = value; }
        }

        private void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PlatformContents;
        }

        public PlayerIndex ActivePlayerIndex
        {
            get { return _activePlayerIndex; }
            set { _activePlayerIndex = value; }
        }

        public GraphicsDeviceManager Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        public StateManager StateManager
        {
            get { return _stateManager; }
        }

        public InputState InputState
        {
            get { return _inputState; }
        }

        public Camera Camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        public List<Thread> Threads
        {
            get { return _threads; }
        }

        protected override void Initialize()
        {
            _stateManager.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _stateManager.LoadContent();
            base.LoadContent();
        }

        
        protected override void Update(GameTime gameTime)
        {
            PlayerIndex controlIndex;
            if (_inputState.IsKeyPressed(Keys.Escape, null, out controlIndex) ||
                _inputState.IsButtonPressed(Buttons.Back, null, out controlIndex))
            {
                foreach (Thread thread in _threads)
                {
                    thread.Abort();
                }
                Exit();
            }
            _inputState.Update(gameTime);
            _stateManager.ProcessInput(gameTime);
            _stateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _stateManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
