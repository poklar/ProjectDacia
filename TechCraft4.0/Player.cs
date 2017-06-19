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

using TechCraftEngine;
using TechCraftEngine.WorldEngine;
using TechCraftEngine.Cameras;
using TechCraft.States;
using TechCraftEngine.Particles;
using TechCraft.ParticleSystems;
using System.Diagnostics;

namespace TechCraft
{
    public class Player
    {
        private TechCraftGame _game;
        private World _world;
        private Vector3 _position;
        private Vector3 _playerVelocity;
        private double _headBob;
        private PlayingState _state;

        private BubbleParticleSystem _bubbleParticleSystem;
        private SnowParticleSystem _snowParticleSystem;
        private SoundEffect _bubbleSound;
        private SoundEffectInstance _bubbleSoundInstance;
        private SoundEffect _splashSound;
        private bool _wasUnderwater;
        private bool _isAboveSnowline;
        private MouseState _previousMouseState;

        private Model _aimedBlockModel;
        private BasicEffect _aimedBlockEffect;
        private Texture2D _aimedBlockTexture;

        public PositionedBlock? AimedSolidBlock { get; private set; } // nullable object.        
        public PositionedBlock? AimedEmptyBlock { get; private set; } // nullable object.  

        public Player(TechCraftGame game, PlayingState state, World world, Vector3 startPosition)
        {
            _game = game;
            _world = world;
            _position = startPosition;
            _state = state;
        }

        public Vector3 Position
        {
            get { return _position; }
        }

        public FirstPersonCamera Camera
        {
            get
            {
                return (FirstPersonCamera)_game.Camera;
            }
        }

        /*public IsometricCamera Camera
        {
            get
            {
                return (IsometricCamera)_game.Camera;
            }
        }*/

        public void Initialize()
        {
            _playerVelocity = Vector3.Zero;
            _bubbleParticleSystem = new BubbleParticleSystem(_game, _game.Content);
            _bubbleParticleSystem.Initialize();
            _snowParticleSystem = new SnowParticleSystem(_game, _game.Content);
            _snowParticleSystem.Initialize();

            Camera.LeftRightRotation =  3.946f;
            Camera.UpDownRotation = -1.124f;
        }

        public void LoadContent()
        {
            _splashSound = _game.Content.Load<SoundEffect>("Sounds\\splash");
            _bubbleSound = _game.Content.Load<SoundEffect>("Sounds\\bubbles");
            _bubbleSoundInstance = _bubbleSound.CreateInstance();
            _bubbleSoundInstance.IsLooped = true;
            _bubbleSoundInstance.Play(); _bubbleSoundInstance.Pause();
            _bubbleSoundInstance.Volume = 0.2f;
            _bubbleSoundInstance.Pitch = -1f;

            _aimedBlockModel = _game.Content.Load<Model>("Models\\AimedBlock");
            _aimedBlockEffect = new BasicEffect(_game.GraphicsDevice);
            _aimedBlockTexture = _game.Content.Load<Texture2D>("Textures\\AimedBlock");
        }

        Matrix _rotationMatrix;
        Vector3 _lookVector;
        Random r = new Random();

        public bool IsAboveSnowLine
        {
            get { return _isAboveSnowline; }
        }

        private void UpdateParticles(GameTime gameTime)
        {
            if (IsUnderWater)
            {
                _bubbleParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
                _bubbleParticleSystem.Update(gameTime);
                Vector3 offset = new Vector3((float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5);
                _bubbleParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
            }
            if (IsAboveSnowLine)
            {
                _snowParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
                _snowParticleSystem.Update(gameTime);
                Vector3 offset = new Vector3((float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5);
                _snowParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
                _snowParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
            }
        }

        private void UpdateSounds()
        {
            bool underWater = IsUnderWater;

            if (underWater && !_wasUnderwater)
            {
                _splashSound.Play(0.2f, 0f, 0);
                _bubbleSoundInstance.Resume();
            }
            if (!underWater && _wasUnderwater)
            {
                _splashSound.Play(0.2f, 0f, 0);
                _bubbleSoundInstance.Pause();
            }
            _wasUnderwater = underWater;
        }

        public Vector3 LookVector
        {
            get { return _lookVector; }
        }

        public void Update(GameTime gameTime)
        {
            _rotationMatrix = Matrix.CreateRotationX(Camera.UpDownRotation) * Matrix.CreateRotationY(Camera.LeftRightRotation);
            _lookVector = Vector3.Transform(Vector3.Forward, _rotationMatrix);
            //Debug.WriteLine("_lookVector " + _lookVector);

            _lookVector.Normalize();

            UpdateParticles(gameTime);
            UpdateSounds();
            FindAimedBlock();

            PlayerIndex controlIndex;

            if (_game.InputState.IsButtonDown(Buttons.A, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyDown(Keys.Space, _game.ActivePlayerIndex, out controlIndex))
            {
                Vector3 footPosition = _position + new Vector3(0f, -1.5f, 0f);
                //XXX fly mode
                //if (_world.SolidAtPoint(footPosition) &&  _playerVelocity.Y == 0)
                //{
                _playerVelocity.Y = WorldSettings.PLAYERJUMPVELOCITY;
                float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
                _position.Y += amountBelowSurface + 0.01f;
                //}
            }

            if (_game.InputState.IsKeyPressed(Keys.Up, _game.ActivePlayerIndex, out controlIndex))
            {
                _world.CURRENTMAPLEVEL++;
                _world.AddFloor();
            }
            if (_game.InputState.IsKeyPressed(Keys.Down, _game.ActivePlayerIndex, out controlIndex))
            {
                _world.CURRENTMAPLEVEL--;
                _world.RemoveFloor();
            }

            UpdatePosition(gameTime);

            float headbobOffset = (float)Math.Sin(_headBob) * 0.06f;
            Camera.Position = _position; //+ new Vector3(0, 0.15f + headbobOffset, 0);

            CheckBuild(gameTime);

            _previousMouseState = Mouse.GetState();

        }

        public void CheckBuild(GameTime gameTime)
        {
            PlayerIndex controlIndex;

            MouseState mouseState = Mouse.GetState();

            float distance = 0.0f;

            Ray ray = GetMouseRay();
            BlockIndex index = new BlockIndex(ray.Direction * distance + ray.Position);

            if (_game.InputState.IsButtonPressed(Buttons.RightTrigger, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyPressed(Keys.Q, _game.ActivePlayerIndex, out controlIndex)
                || (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed))
            {
                //for (float x = 0.5f; x < 5f; x += 0.2f)
                for (float x = 0.5f; x < 100f; x += 0.2f)
                {
                    //Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    index = new BlockIndex(ray.Direction * distance + ray.Position);

                    BlockType blockType = _world.BlockTypeAtPoint(index.Position);
                    if (blockType != BlockType.None && blockType != BlockType.Water)
                    {
                        if (index.Position.Y > 2)
                        {
                            // Can't dig water or lava
                            BlockType targetType = _world.BlockTypeAtPoint(index.Position);
                            if (BlockInformation.IsDiggable(targetType))
                            {
                                _world.RemoveBlock((ushort)index.Position.X, (ushort)index.Position.Y, (ushort)index.Position.Z);
                            }
                        }
                        break;
                    }

                    distance += 0.2f;
                }
                distance = 0.0f;
            }            
            if (_game.InputState.IsKeyPressed(Keys.V, _game.ActivePlayerIndex, out controlIndex))
            {
                for (float x = 0.5f; x < 5f; x += 0.2f)
                {
                    Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    if (_world.BlockTypeAtPoint(targetPoint) != BlockType.None)
                    {
                        Random r = new Random();
                        for (ushort dy = (ushort)(targetPoint.Y - 3); dy < (ushort)(targetPoint.Y + 3); dy++)
                        {
                            for (ushort dx = (ushort)(targetPoint.X - 3); dx < (ushort)(targetPoint.X + 3); dx++)
                            {
                                for (ushort dz = (ushort)(targetPoint.Z - 3); dz < (ushort)(targetPoint.Z + 3); dz++)
                                {
                                    if (r.Next(3) == 0) _world.RemoveBlock(dx, dy, dz);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            if (_game.InputState.IsButtonPressed(Buttons.LeftTrigger, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyPressed(Keys.E, _game.ActivePlayerIndex, out controlIndex)
                || (mouseState.RightButton == ButtonState.Pressed
                && _previousMouseState.RightButton != ButtonState.Pressed))
            {
                float hit = 0;
                //for (float x = 0.8f; x < 5f; x += 0.1f)
                for (float x = 0.5f; x < 100f; x += 0.2f)
                {
                    //Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    index = new BlockIndex(ray.Direction * distance + ray.Position);
                    if (_world.BlockTypeAtPoint(index.Position) != BlockType.None)
                    {
                        hit = x;
                        break;
                    }
                    distance += 0.2f;
                }
                if (hit != 0)
                {
                    for (float x = hit; x > 0.7f; x -= 0.1f)
                    {
                        //Vector3 targetPoint = Camera.Position + (_lookVector * x);
                        index = new BlockIndex(ray.Direction * distance + ray.Position);
                        if (_world.BlockTypeAtPoint(index.Position) == BlockType.None)
                        {
                            _world.AddBlock((ushort)index.Position.X, (ushort)index.Position.Y, (ushort)index.Position.Z, _state.BlockPicker.SelectedBlockType, true, true);
                            break;
                        }
                        distance -= 0.1f;
                    }
                }
                distance = 0.0f;
            }
        }

        public bool IsUnderWater
        {
            get
            {
                return (_world.BlockTypeAtPoint(_game.Camera.Position) == BlockType.Water);
            }
        }

        private void UpdatePosition(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            int scrollWheelDelta = _previousMouseState.ScrollWheelValue - mouseState.ScrollWheelValue;

            Vector3 footPosition;

            if (scrollWheelDelta >= 120)
            {
                footPosition = _position + new Vector3(0f, -1.5f, 0f);


                float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
                _position.Y += amountBelowSurface + 0.01f;
            }
            else if (scrollWheelDelta <= -120)
            {
                footPosition = _position + new Vector3(0f, 1.5f, 0f);


                float amountBelowSurface = ((ushort)footPosition.Y) - 1 - footPosition.Y;
                _position.Y += amountBelowSurface + 0.01f;
            }
            //_previousMouseState = mouseState;


            /*_playerVelocity.Y += WorldSettings.PLAYERGRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 footPosition = _position + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = _position + new Vector3(0f, 0.1f, 0f);
            _isAboveSnowline = headPosition.Y > WorldSettings.SNOWLINE;
            if (_world.SolidAtPoint(footPosition) || _world.SolidAtPoint(headPosition))
            {
                BlockType standingOnBlock = _world.BlockAtPoint(footPosition);
                BlockType hittingHeadOnBlock = _world.BlockAtPoint(headPosition);

                // If we"re hitting the ground with a high velocity, die!
                //if (standingOnBlock != BlockType.None && _P.playerVelocity.Y < 0)
                //{
                //    float fallDamage = Math.Abs(_P.playerVelocity.Y) / DIEVELOCITY;
                //    if (fallDamage >= 1)
                //    {
                //        _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        _P.KillPlayer(Defines.deathByFall);//"WAS KILLED BY GRAVITY!");
                //        return;
                //    }
                //    else if (fallDamage > 0.5)
                //    {
                //        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn't appear.
                //        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                //        if (standingOnBlock != BlockType.Jump)
                //        {
                //            _P.screenEffect = ScreenEffect.Fall;
                //            _P.screenEffectCounter = 2 - (fallDamage - 0.5) * 4;
                //            _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        }
                //    }
                //}

                // If the player has their head stuck in a block, push them down.
                if (_world.SolidAtPoint(headPosition))
                {
                    int blockIn = (int)(headPosition.Y);
                    _position.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (_world.SolidAtPoint(footPosition))
                {
                    int blockOn = (int)(footPosition.Y);
                    _position.Y = (float)(blockOn + 1 + 1.45);
                }

                _playerVelocity.Y = 0;

                // Logic for standing on a block.
                // switch (standingOnBlock)
                //  {
                //case BlockType.Jump:
                //    _P.playerVelocity.Y = 2.5f * JUMPVELOCITY;
                //    _P.PlaySoundForEveryone(InfiniminerSound.Jumpblock, _P.playerPosition);
                //    break;

                //case BlockType.Road:
                //    movingOnRoad = true;
                //    break;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //  }

                // Logic for bumping your head on a block.
                // switch (hittingHeadOnBlock)
                // {
                //case BlockType.Shock:
                //    _P.KillPlayer(Defines.deathByElec);
                //    return;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //}
            }

            // Death by falling off the map.
            //if (_P.playerPosition.Y < -30)
            //{
            //    _P.KillPlayer(Defines.deathByMiss);
            //    return;
            //}*/

            _position += _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 moveVector = new Vector3(
                GamePad.GetState(_game.ActivePlayerIndex).ThumbSticks.Left.X,
                0,
                -GamePad.GetState(_game.ActivePlayerIndex).ThumbSticks.Left.Y);

            PlayerIndex controlIndex;
            if (_game.InputState.IsKeyDown(Keys.W, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Forward * 10;
            }
            if (_game.InputState.IsKeyDown(Keys.S, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Backward * 10;
            }
            if (_game.InputState.IsKeyDown(Keys.A, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Left * 10;
            }
            if (_game.InputState.IsKeyDown(Keys.D, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Right * 10;
            }

            //moveVector.Normalize();
            moveVector *= WorldSettings.PLAYERMOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 rotatedMoveVector = Vector3.Transform(moveVector, Matrix.CreateRotationY(Camera.LeftRightRotation));

            // Attempt to move, doing collision stuff.
            if (TryToMoveTo(rotatedMoveVector, gameTime)) { }
            else if (!TryToMoveTo(new Vector3(0, 0, rotatedMoveVector.Z), gameTime)) { }
            else if (!TryToMoveTo(new Vector3(rotatedMoveVector.X, 0, 0), gameTime)) { }
        }

        private void FindAimedBlock()
        {
            float distance = 0.0f;
            float? intersect;

            Ray ray = GetMouseRay();
            BlockIndex index = new BlockIndex(ray.Direction * distance + ray.Position);            

            while (distance <= 100f)
            {
                index = new BlockIndex(ray.Direction * distance + ray.Position);

                intersect = index.GetBoundingBox().Intersects(ray);

                Vector3 target = Camera.Position + (_lookVector * distance);

                Block block = _world.BlockAt((int) index.Position.X, (int) index.Position.Y, (int) index.Position.Z);

                if (block != null)
                {
                    if (block.BlockType == BlockType.None)
                        AimedEmptyBlock = new PositionedBlock(new Vector3i(index.Position), block.BlockType);
                    else if (block.IsActive)
                    {
                        AimedSolidBlock = new PositionedBlock(new Vector3i(index.Position), block.BlockType);
                        return;
                    }
                    
                }
                distance += 0.2f;
            }

            AimedSolidBlock = null;
        }

        private Ray GetMouseRay()
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            return Camera.GetMouseRay(mousePos, _game.GraphicsDevice.Viewport);
        }

        private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength + 0.3f);

            // Apply this test vector.
            Vector3 movePosition = _position + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!_world.SolidAtPoint(movePosition) && !_world.SolidAtPoint(lowerBodyPoint) && !_world.SolidAtPoint(midBodyPoint))
            {
                _position = _position + moveVector;
                if (moveVector != Vector3.Zero)
                {
                    _headBob += 0.2;
                }
                return true;
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = _world.BlockTypeAtPoint(lowerBodyPoint);
            BlockType midBlock = _world.BlockTypeAtPoint(midBodyPoint);
            BlockType upperBlock = _world.BlockTypeAtPoint(movePosition);

            // It's solid there, so see if it's a lava block. If so, touching it will kill us!
            //if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            //{
            //    _P.KillPlayer(Defines.deathByLava);
            //    return true;
            //}

            // If it's a ladder, move up.
            //if (upperBlock == BlockType.Ladder || lowerBlock == BlockType.Ladder || midBlock == BlockType.Ladder)
            //{
            //    _P.playerVelocity.Y = CLIMBVELOCITY;
            //    Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
            //    if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
            //        _P.playerPosition.Y += 0.1f;
            //    return true;
            //}

            return true;
        }

        public void Draw(GameTime gameTime)
        {
            if (IsUnderWater)
            {
                _bubbleParticleSystem.Draw(gameTime);
            }
            _snowParticleSystem.Draw(gameTime);

            if (AimedSolidBlock.HasValue) RenderAimedBlock();
        }

        private void RenderAimedBlock()
        {
            _game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            // allows any transparent pixels in original PNG to draw transparent
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var position = AimedSolidBlock.Value.Position.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);
            Matrix matrix_a, matrix_b;
            Matrix identity = Matrix.Identity; // setup the matrix prior to translation and scaling  
            Matrix.CreateTranslation(ref position, out matrix_a);
            // translate the position a half block in each direction
            Matrix.CreateScale(0.505f, out matrix_b);
            // scales the selection box slightly larger than the targetted block
            identity = Matrix.Multiply(matrix_b, matrix_a); // the final position of the block

            _aimedBlockEffect.World = identity;
            _aimedBlockEffect.View = Camera.View;
            _aimedBlockEffect.Projection = Camera.Projection;
            _aimedBlockEffect.Texture = _aimedBlockTexture;
            _aimedBlockEffect.TextureEnabled = true;

            foreach (EffectPass pass in _aimedBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int i = 0; i < _aimedBlockModel.Meshes[0].MeshParts.Count; i++)
                {
                    ModelMeshPart parts = _aimedBlockModel.Meshes[0].MeshParts[i];
                    if (parts.NumVertices == 0) continue;

                    _game.GraphicsDevice.Indices = parts.IndexBuffer;
                    _game.GraphicsDevice.SetVertexBuffer(parts.VertexBuffer);
                    _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, parts.PrimitiveCount);
                }
            }
        }
    }
}
