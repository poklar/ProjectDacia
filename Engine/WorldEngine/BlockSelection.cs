using Engine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.WorldEngine
{
    /// <summary>
    /// The block selector
    /// </summary>
    public class BlockSelection
    {
        private TechCraftGame _game;
        private World _world;

        private List<PositionedBlock> _selectedBlocks;
        private PositionedBlock? _startBlock;
        private bool _startingBlockSelected;
        

        private Model _aimedBlockModel;
        private BasicEffect _aimedBlockEffect;
        private Texture2D _aimedBlockTexture;

        
        /// <summary>
        /// Gets or sets the aimed solid block.
        /// </summary>
        /// <value>
        /// The aimed solid block.
        /// </value>
        public PositionedBlock? AimedSolidBlock { get; set; } // nullable object.        
        /// <summary>
        /// Gets the aimed empty block.
        /// </summary>
        /// <value>
        /// The aimed empty block.
        /// </value>
        public PositionedBlock? AimedEmptyBlock { get; private set; } // nullable object.  

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockSelection"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="world">The world.</param>
        public BlockSelection(TechCraftGame game, World world)
        {
            _game = game;
            _world = world;
        }

        public FirstPersonCamera Camera
        {
            get
            {
                return (FirstPersonCamera)_game.Camera;
            }
        }

        public void Initialize()
        {
            _selectedBlocks = new List<PositionedBlock>();
            _startingBlockSelected = false;
        }


        public void LoadContent()
        {
            //_aimedBlockModel = _game.Content.Load<Model>("Models\\AimedBlock");
            _aimedBlockModel = _game.Content.Load<Model>("Models\\selectionModel");
            _aimedBlockEffect = new BasicEffect(_game.GraphicsDevice);
            //_aimedBlockTexture = _game.Content.Load<Texture2D>("Textures\\AimedBlock");
            _aimedBlockTexture = _game.Content.Load<Texture2D>("Textures\\whiteSelectionTex");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            if (_startingBlockSelected)
                RenderGrid();
            else if (AimedSolidBlock.HasValue)
                RenderAimedBlock();
        }

        /// <summary>
        /// Sets the starting point of the grid selection.
        /// </summary>
        /// <param name="startPosition">The start position of the grid.</param>
        public void SetStartingPoint(Vector3i startPosition)
        {
            // Get blocktype from the block
            BlockType blockType = _world.BlockAt(startPosition.x, startPosition.y, startPosition.z).BlockType;

            // Set the starting block
            _startBlock = new PositionedBlock(startPosition, blockType);

            // Set the bool true so it knows the starting block has been selected
            _startingBlockSelected = true;
        }

        /// <summary>
        /// Cancels the selection.
        /// </summary>
        public void CancelSelection()
        {
            // Clear the list of selected blocks
            _selectedBlocks.Clear();

            // Reset the starting block
            _startBlock = null;

            if (AimedSolidBlock.HasValue)
                AimedSolidBlock = null;

            // Selection is canceled, so selected block is false
            _startingBlockSelected = false;
        }

        public void SetAimedBlock(Vector3i position, BlockType blockType, bool isSolid)
        {
            if (isSolid)
                AimedSolidBlock = new PositionedBlock(position, blockType);
            else
                AimedEmptyBlock = new PositionedBlock(position, blockType);
        }

        public void FindAimedBlock(Vector3 lookVector)
        {
            float distance = 0.0f;
            float? intersect;

            MouseState mouseState = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            Ray ray = Camera.GetMouseRay(mousePos, _game.GraphicsDevice.Viewport);
            BlockIndex index = new BlockIndex(ray.Direction * distance + ray.Position);

            while (distance <= 100f)
            {
                index = new BlockIndex(ray.Direction * distance + ray.Position);

                intersect = index.GetBoundingBox().Intersects(ray);

                Vector3 target = Camera.Position + (lookVector * distance);

                Block block = _world.BlockAt((int)index.Position.X, (int)index.Position.Y, (int)index.Position.Z);

                if (block != null)
                {
                    if (block.BlockType == BlockType.None)
                        SetAimedBlock(new Vector3i(index.Position), block.BlockType, false);
                    else if (block.IsActive)
                    {
                        if (_startingBlockSelected)
                        {
                            PositionedBlock currentSelectedBlock = new PositionedBlock(new Vector3i(index.Position), block.BlockType);
                            
                            if (_startBlock != null)
                                CalculateGrid(currentSelectedBlock);                        
                        }
                        else
                            SetAimedBlock(new Vector3i(index.Position), block.BlockType, true);
                        return;
                    }

                }
                distance += 0.2f;
            }

            AimedSolidBlock = null;
        }

        /// <summary>
        /// Calculates the grid.
        /// </summary>
        /// <param name="currentBlock">The current block.</param>
        private void CalculateGrid(PositionedBlock currentBlock)
        {
            // Clear the array so it will start all over
            _selectedBlocks.Clear();

            // Get the starting position of the first block
            Vector3 startPos = _startBlock.Value.Position.AsVector3();

            // Get the position of the block that is currently selected
            Vector3 endPos = currentBlock.Position.AsVector3();

            // Fills up a rectangle with blocks depending the position of the current block (endPos)
            if (startPos.X <= endPos.X && startPos.Z <= endPos.Z)
            {
                for (float x = startPos.X; x <= endPos.X; x++)
                {
                    for (float z = startPos.Z; z <= endPos.Z; z++)
                        AddSelectedBlock(x, startPos.Y, z);
                }
            }
            else if (startPos.X <= endPos.X && startPos.Z >= endPos.Z)
            {
                for (float x = startPos.X; x <= endPos.X; x++)
                {
                    for (float z = startPos.Z; z >= endPos.Z; z--)
                        AddSelectedBlock(x, startPos.Y, z);
                }
            }
            else if (startPos.X >= endPos.X && startPos.Z >= endPos.Z)
            {
                for (float x = startPos.X; x >= endPos.X; x--)
                {
                    for (float z = startPos.Z; z >= endPos.Z; z--)
                        AddSelectedBlock(x, startPos.Y, z);
                }
            }
            else if (startPos.X >= endPos.X && startPos.Z <= endPos.Z)
            {
                for (float x = startPos.X; x >= endPos.X; x--)
                {
                    for (float z = startPos.Z; z <= endPos.Z; z++)
                        AddSelectedBlock(x, startPos.Y, z);
                }
            }
        }

        /// <summary>
        /// Adds the selected block to the list.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        private void AddSelectedBlock(float x, float y, float z)
        {
            Vector3 position = new Vector3(x, y, z);
            // Find the blocktype
            BlockType blockType = _world.BlockTypeAtPoint(position);

            // If the blocktype isn't none
            if (blockType != BlockType.None)
            {
                // Create positioned block
                PositionedBlock posBlock = new PositionedBlock(new Vector3i(position), blockType);

                // Add it to the list
                _selectedBlocks.Add(posBlock);
            }
                
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


        /// <summary>
        /// Renders the selected grid.
        /// </summary>
        // TODO: I'm sure this can be done differently/better
        private void RenderGrid()
        {
            _game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            // allows any transparent pixels in original PNG to draw transparent
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (_selectedBlocks.Count > 0)
            {
                foreach (var item in _selectedBlocks)
                {
                    var position = item.Position.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);
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
    }
}
