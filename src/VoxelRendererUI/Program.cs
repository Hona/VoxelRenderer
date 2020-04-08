using csPixelGameEngine;
using System;

namespace VoxelRendererUI
{
    internal static class Program
    {
        private static Random _random = new Random();
        private static PixelGameEngine _pixelGameEngine;
        internal static void Main()
        {
            var appSettings = new EngineWindowSettings
            {
                AppName = "VoxelRenderer",
                ScreenWidth = 150,
                ScreenHeight = 150,
                PixelHeight = 4,
                PixelWidth = 4
            };

            _pixelGameEngine = EngineWindowUtility.CreateEngine(appSettings);

            _pixelGameEngine.OnFrameUpdate += PixelGameEngine_OnFrameUpdate;

            _pixelGameEngine.Start();
        }

        private static void PixelGameEngine_OnFrameUpdate(object sender, FrameUpdateEventArgs frameUpdateArgs)
        {
            for (uint x = 0; x < _pixelGameEngine.ScreenWidth; x++)
                for (uint y = 0; y < _pixelGameEngine.ScreenHeight; y++)
                    _pixelGameEngine.Draw(x, y, new Pixel((byte)_random.Next(255), (byte)_random.Next(255), (byte)_random.Next(255)));

            _pixelGameEngine.DrawString(0, 0, $"{(int)(1 / frameUpdateArgs.ElapsedTime)} FPS", Pixel.WHITE);
        }
    }
}
