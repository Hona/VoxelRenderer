using Console3DEngineLibrary;
using Console3DEngineLibrary.Map;
using csPixelGameEngine;
using System;

namespace VoxelRendererUI
{
    internal static class Program
    {
        private static Random _random = new Random();
        private static PixelGameEngine _pixelGameEngine;
        private static Game _game;
        internal static void Main()
        {
            var mapLines = new[]
            {
                "xxxxxxxxxxxxxxxxxxxx",
                "x   x  x  y        x",
                "x   x  x  y        x",
                "xxxx     y   xxxxxxx",
                "x        z         x",
                "x       z z        x",
                "x      z   z       x",
                "x     z     z      x",
                "x    z       z     x",
                "x                  x",
                "x                  x",
                "xxxxxxxxxxxxxxxxxxxx"
            };

            var mapConfig = new MapConfig
            {
                EmptyChar = ' ',
                PlayerChar = 'p'
            };
            var fov = 90;

            var appSettings = new EngineWindowSettings
            {
                AppName = "VoxelRenderer",
                ScreenWidth = 1280,
                ScreenHeight = 720,
                PixelHeight = 1,
                PixelWidth = 1
            };

            _pixelGameEngine = EngineWindowUtility.CreateEngine(appSettings);
            _game = new Game(ASCIIMap.FromLines(mapConfig, mapLines), fov, _pixelGameEngine);

            _pixelGameEngine.OnFrameUpdate += PixelGameEngine_OnFrameUpdate;
            _pixelGameEngine.Start();
        }

        private static void PixelGameEngine_OnFrameUpdate(object sender, FrameUpdateEventArgs frameUpdateArgs)
        {
            _game.Update(frameUpdateArgs.ElapsedTime);
            _pixelGameEngine.DrawString(0, 0, $"{(int)(1 / frameUpdateArgs.ElapsedTime)} FPS", Pixel.WHITE);
        }
    }
}
