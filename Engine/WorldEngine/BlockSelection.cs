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
    // TODO: Using models for now, should switch to vertices
    public class BlockSelection
    {
        private TechCraftGame _game;
        private World _world;

        private Dictionary<PositionedBlock, Texture2D> _selectedBlocks;
        private PositionedBlock? _startBlock;
        private bool _startingBlockSelected;
        

        private Model _aimedBlockModel;
        private BasicEffect _aimedBlockEffect;
        private Texture2D _aimedBlockWhiteTexture;
        private Texture2D _aimedBlockRedTexture;


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

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            _selectedBlocks = new Dictionary<PositionedBlock, Texture2D>();
            _startingBlockSelected = false;
        }


        /// <summary>
        /// Loads the content.
        /// </summary>
        public void LoadContent()
        {
            //_aimedBlockModel = _game.Content.Load<Model>("Models\\AimedBlock");
            _aimedBlockModel = _game.Content.Load<Model>("Models\\selectionModel");
            _aimedBlockEffect = new BasicEffect(_game.GraphicsDevice);
            //_aimedBlockTexture = _game.Content.Load<Texture2D>("Textures\\AimedBlock");
            _aimedBlockWhiteTexture = _game.Content.Load<Texture2D>("Textures\\whiteSelectionTex");
            _aimedBlockRedTexture = _game.Content.Load<Texture2D>("Textures\\redSelectionTex");
        }

        /// <summary>
        /// Updates the specified game time.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// Draws the specified game time.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
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
            // Clear the dictionary of selected blocks
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

        /// <summary>
        /// Finds the aimed (targeted) block.
        /// </summary>
        public void FindAimedBlock()
        {
            BlockIndex index;
            float distance = 0.0f;
            //float? intersect;            

            // Get the current mouse state
            MouseState mouseState = Mouse.GetState();

            // Get mouse positions
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            // Calculate where the mouse points to
            Ray ray = Camera.GetMouseRay(mousePos, _game.GraphicsDevice.Viewport);
           
            // While within reach
            while (distance <= WorldSettings.BlockEditing.PLAYERREACH)
            {
                // Create a box at calculated position
                index = new BlockIndex(ray.Direction * distance + ray.Position);

                //intersect = index.GetBoundingBox().Intersects(ray);

                // Get the block from the previous calculated index
                Block block = _world.BlockAt((int)index.Position.X, (int)index.Position.Y, (int)index.Position.Z);

                // If there isn't a block
                if (block.BlockType == BlockType.None)
                    // Set empty aimed block
                    // TODO: does this even have a point? REMOVE?
                    SetAimedBlock(new Vector3i(index.Position), block.BlockType, false);
                // If there's a block and it is active
                else if (block.BlockType != BlockType.None && block.IsActive)
                {
                    // If there's a starting block selected for editing
                    if (_startingBlockSelected)
                    {
                        // Create a positioned block from the current selected block
                        PositionedBlock currentSelectedBlock = new PositionedBlock(new Vector3i(index.Position), block.BlockType);
                        
                        // Check if the starting block is set    
                        if (_startBlock != null)
                            // Calculate the grid
                            CalculateGrid(currentSelectedBlock);                        
                    }
                    // If not in editing mode
                    else
                        // Set aimed block
                        SetAimedBlock(new Vector3i(index.Position), block.BlockType, true);
                    return;
                }

                // Increaase the distance
                distance += 0.2f;
            }

            // Reset aimed block
            AimedSolidBlock = null;
        }

        /// <summary>
        /// Calculates the grid.
        /// </summary>
        /// <param name="currentBlock">The current block.</param>
        private void CalculateGrid(PositionedBlock currentBlock)
        {
            // Clear the dictionary so it will start all over
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
        /// Adds the selected block to the dictionary.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        private void AddSelectedBlock(float x, float y, float z)
        {
            // Convert postion to a Vector3
            Vector3 position = new Vector3(x, y, z);
            // Find the blocktype
            BlockType blockType = _world.BlockTypeAtPoint(position);

            // If the blocktype isn't none
            if (blockType != BlockType.None)
            {
                // Create positioned block
                PositionedBlock posBlock = new PositionedBlock(new Vector3i(position), blockType);

                // TODO: JUST FOR TESTING, later on more block types needs to be checked if it's possible to build there
                // TODO: Check if building is possible
                if (blockType == BlockType.Grass)
                    // Possible to build here so add block and the white texture to dictionary
                    _selectedBlocks.Add(posBlock, _aimedBlockWhiteTexture);
                else
                    // Not possible to build here, add block and the red texture to the dictionary
                    _selectedBlocks.Add(posBlock, _aimedBlockRedTexture);
            }

        }

        /// <summary>
        /// Renders the aimed block.
        /// </summary>
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
            _aimedBlockEffect.Texture = _aimedBlockWhiteTexture;
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
                    var position = item.Key.Position.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);
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
                    _aimedBlockEffect.Texture = item.Value;
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
