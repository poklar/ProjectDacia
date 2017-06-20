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

using System.Net;
using System.Threading;
using Engine.WorldEngine;
using System.IO;
using System.IO.Compression;
using Engine.WorldEngine.Generators;

namespace Engine.Network
{
    public enum GameState
    {
        Ready,
        Connecting,
        Connected,
        Disconnected,
        Rejected,
        Loading,
        Loaded,
        Playing
    }

    public enum NetworkMessageType
    {
        PlayerMove = 1
    }

    public class GameClient
    {
        private TechCraftGame _game;
        private World _world;
        private GameState _gameState = GameState.Ready;

        public GameClient(TechCraftGame game)
        {
            _game = game;
            _world = new World(_game);
            _world.Initialize();
        }

        public World World
        {
            get { return _world; }
        }

        public GameState GameState
        {
            get { return _gameState; }
        }

        private string _statusText = "INITIALIZING";
        public string StatusText
        {
            get { return _statusText; }
        }

        public void LoadMap()
        {
             _statusText = "LOADING";
             /*LandscapeMapGenerator mapGenerator = new LandscapeMapGenerator();
             //DualLayerTerrainWithMediumValleys mapGenerator = new DualLayerTerrainWithMediumValleys(); 
             _statusText = "GENERATING MAP";
             BlockType[, ,] mapData = mapGenerator.GenerateMap();
             _statusText = "BUILDING WORLD";
             for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
             {
                 for (int y = 0; y < WorldSettings.MAPHEIGHT; y++)
                 {
                     for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                     {
                         BlockType blockType = mapData[x, y, z];
                         if (blockType != BlockType.None)
                         {
                             _world.AddBlock(x, y, z, blockType);
                         }
                     }
                 }
             }*/
             


            _statusText = "BUILDING WORLD";
            //IRegionBuilder builder = new SimpleTerrain();
            IRegionBuilder builder = new TerrainWithCaves();
            //IRegionBuilder builder = new FlatReferenceTerrain();

            
            
            _world.BuildRegions(builder);

            _statusText = "INITIALIZING LIGHTING";
            _world.Lighting.Initialize();

            //_statusText = "BUILDING REGIONS";
            //_world.BuildRegions();
            _statusText = "LOADED";
            _gameState = GameState.Loaded;
        }
    }
}
