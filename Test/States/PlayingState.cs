﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Engine;
using Engine.Cameras;
using Engine.Common;
using Engine.Components;
using Engine.Controllers;
using Engine.Managers;
using Engine.Particles;
using Engine.WorldEngine;
using Test.ParticleSystems;
using Engine.Network;

namespace Test.States
{
    public class PlayingState : State
    {
        private FirstPersonCamera _camera;
        private FirstPersonCameraController _cameraController;
        private IsometricCamera _isoCamera;
        private ParticleManager _particleManager;
        private WeaponManager _weaponManager;
        private DebugInfo _debugInfo;
        private BlockSelection _blockSelection;

        private Player _player;
        private Texture2D _crosshairTexture;
        private Texture2D _underWaterTexture;

        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private BlockPicker _blockPicker;


        public PlayingState()
        {
        }

        public override void Initialize()
        {

            
            _camera = new FirstPersonCamera(Game);
            _camera.Initialize();
            _camera.Position = Vector3.Zero;
            _camera.LookAt(Vector3.Zero);
            _cameraController = new FirstPersonCameraController(Game);
            _cameraController.Initialize();
            Game.Camera = _camera;

            Game.IsMouseVisible = true;

            _blockSelection = new BlockSelection(Game, _game.GameClient.World);
            _blockSelection.Initialize();

            _player = new Player(Game, this, _game.GameClient.World, _blockSelection, new Vector3(30f, 100f, 30f));
            _player.Initialize();

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            _blockPicker = new BlockPicker(Game, _spriteBatch);
            _blockPicker.Initialize();

            _particleManager = new ParticleManager(Game);
            BubbleParticleSystem pickupParticles = new BubbleParticleSystem(Game,Game.Content);
            pickupParticles.Initialize();
            _particleManager.ParticleSystems.Add(pickupParticles);


            _spriteFont = Game.Content.Load<SpriteFont>("Fonts\\console");

            _debugInfo = new DebugInfo(Game, _game.GameClient.World);


            _particleManager.ParticleEmitters.Add(new ParticleEmitter(_particleManager.ParticleSystems[0], 50, new Vector3(5,3,5)));
            _particleManager.ParticleEmitters.Add(new ParticleEmitter(_particleManager.ParticleSystems[0], 50, new Vector3(15,3, 15)));

            //_weaponManager = new WeaponManager(_game, _game.GameClient.World, _player);
            //_weaponManager.Initialize();

        }

        public BlockPicker BlockPicker
        {
            get { return _blockPicker; }
        }

        public override void LoadContent()
        {
            _blockPicker.LoadContent();
            _player.LoadContent();
            _debugInfo.LoadContent();
            _blockSelection.LoadContent();
            _crosshairTexture = Game.Content.Load<Texture2D>("Textures\\crosshair");
            _underWaterTexture = Game.Content.Load<Texture2D>("Textures\\underwater");
            //_weaponManager.LoadContent();
        }

        public override void ProcessInput(GameTime gameTime)
        {
            /*PlayerIndex controlIndex;
            if (_game.InputState.IsKeyPressed(Keys.X, null, out controlIndex) || _game.InputState.IsButtonPressed(Buttons.X,null, out controlIndex))
            {
                _weaponManager.Fire();
            }*/
        }

       
        public override void Update(GameTime gameTime)
        {
            _cameraController.Update(gameTime);
            Game.Camera.Update(gameTime);
            _player.Update(gameTime);
            _blockSelection.Update(gameTime);
            _debugInfo.Update(gameTime);
            _game.GameClient.World.Update(gameTime);
            _blockPicker.Update(gameTime);
            //_weaponManager.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(Color.SkyBlue);
            _game.GameClient.World.Draw(gameTime,_player.IsUnderWater);
            _player.Draw(gameTime);
            _debugInfo.Draw(gameTime);
            _blockSelection.Draw(gameTime);

            //_weaponManager.Draw(gameTime);

            // _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend); 

            if (_player.IsUnderWater)
            {
                Rectangle screenRect = new Rectangle(0,0,_game.GraphicsDevice.Viewport.Width,_game.GraphicsDevice.Viewport.Height);
                _spriteBatch.Draw(_underWaterTexture, screenRect, Color.White);
            }
            _spriteBatch.Draw(_crosshairTexture, new Vector2(
                (Game.GraphicsDevice.Viewport.Width / 2) - 10,
                (Game.GraphicsDevice.Viewport.Height / 2) - 10), Color.White);
            _blockPicker.Draw(gameTime);
            _spriteBatch.End();            
        }

    }
}
