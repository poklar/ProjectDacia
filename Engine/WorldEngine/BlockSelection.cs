using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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


        public void Initialize()
        {
            _selectedBlocks = new List<PositionedBlock>();
        }


        public void LoadContent()
        {
            _aimedBlockModel = _game.Content.Load<Model>("Models\\AimedBlock");
            _aimedBlockEffect = new BasicEffect(_game.GraphicsDevice);
            _aimedBlockTexture = _game.Content.Load<Texture2D>("Textures\\AimedBlock");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            if (AimedSolidBlock.HasValue) RenderAimedBlock();
        }

        public void SetAimedBlock(Vector3i position, BlockType blockType, bool isSolid)
        {
            if (isSolid)
                AimedSolidBlock = new PositionedBlock(position, blockType);
            else
                AimedEmptyBlock = new PositionedBlock(position, blockType);
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
            _aimedBlockEffect.View = _game.Camera.View;
            _aimedBlockEffect.Projection = _game.Camera.Projection;
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
