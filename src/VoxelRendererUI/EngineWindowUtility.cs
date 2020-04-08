using csPixelGameEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelRendererUI
{
    internal static class EngineWindowUtility
    {
        public static PixelGameEngine CreateEngine(EngineWindowSettings settings)
        {
            var window = new GLWindow(settings.ScreenWidth, settings.ScreenHeight, settings.PixelWidth, settings.PixelHeight, settings.AppName);
            var pixelGameEngine = new PixelGameEngine(settings.AppName);
            pixelGameEngine.Construct(settings.ScreenWidth, settings.ScreenHeight, window);
            return pixelGameEngine;
        }
    }
}
