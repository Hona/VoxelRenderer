using Console3DEngineLibrary.Map;
using Console3DEngineLibrary.Rendering;
using csPixelGameEngine;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using VoxelRendererUI;

namespace Console3DEngineLibrary
{
    public class Game
    {
        private readonly ASCIIMap _map;
        private readonly Camera _camera;
        private double _currentFrameTime;
        private PixelGameEngine _pixelGameEngine;

        public Game(ASCIIMap map, double fov, PixelGameEngine pixelGameEngine)
        {
            _map = map;
            _pixelGameEngine = pixelGameEngine;

            _camera = new Camera(fov)
            {
                PositionVector = new Vector
                {
                    X = (double)map.MapNodes.GetLength(0) / 2,
                    Y = (double)map.MapNodes.GetLength(1) / 2
                }
            };
        }

        //speed modifiers
        private double MoveSpeed => 4 * _currentFrameTime; // The constant value is in squares/second
        private double RotationSpeed => 1.2 * _currentFrameTime; // The constant value is in radians/second
        public void Update(double frameTime)
        {
            _currentFrameTime = frameTime;
            // Cast a ray for every column on the screen
            for (var x = 0; x < _pixelGameEngine.ScreenWidth; x++) RayCast(x);

            ProcessInput();
        }
        private void RayCast(int screenX)
        {
            // Ray position and direction

            // X coordinate in the camera space
            var cameraX = 2 * screenX / (double)_pixelGameEngine.ScreenWidth - 1;
            var rayDirectionVector = new Vector
            {
                X = _camera.DirectionVector.X + _camera.CameraPlaneVector.X * cameraX,
                Y = _camera.DirectionVector.Y + _camera.CameraPlaneVector.Y * cameraX
            };

            var mapX = (int)_camera.PositionVector.X;
            var mapY = (int)_camera.PositionVector.Y;

            var distanceFromSideVector = new Vector();

            var deltaDistanceFromSideVector = new Vector
            {
                X = Math.Abs(1 / rayDirectionVector.X),
                Y = Math.Abs(1 / rayDirectionVector.Y)
            };

            double perpendicularWallDistance;

            int stepX, stepY;
            var hit = false;
            var side = 0;

            if (rayDirectionVector.X < 0)
            {
                stepX = -1;
                distanceFromSideVector.X = (_camera.PositionVector.X - mapX) * deltaDistanceFromSideVector.X;
            }
            else
            {
                stepX = 1;
                distanceFromSideVector.X =
                    (mapX + 1.0 - _camera.PositionVector.X) * deltaDistanceFromSideVector.X;
            }

            if (rayDirectionVector.Y < 0)
            {
                stepY = -1;
                distanceFromSideVector.Y = (_camera.PositionVector.Y - mapY) * deltaDistanceFromSideVector.Y;
            }
            else
            {
                stepY = 1;
                distanceFromSideVector.Y =
                    (mapY + 1.0 - _camera.PositionVector.Y) * deltaDistanceFromSideVector.Y;
            }

            while (!hit)
            {
                if (distanceFromSideVector.X < distanceFromSideVector.Y)
                {
                    distanceFromSideVector.X += deltaDistanceFromSideVector.X;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    distanceFromSideVector.Y += deltaDistanceFromSideVector.Y;
                    mapY += stepY;
                    side = 1;
                }

                // Check if the ray has hit a wall
                if (_map.MapNodes[mapX, mapY] != _map.Config.EmptyChar) hit = true;
            }

            if (side == 0)
                perpendicularWallDistance =
                    (mapX - _camera.PositionVector.X + (double)(1 - stepX) / 2) / rayDirectionVector.X;
            else
                perpendicularWallDistance =
                    (mapY - _camera.PositionVector.Y + (double)(1 - stepY) / 2) / rayDirectionVector.Y;

            var lineHeight = (int)(_pixelGameEngine.ScreenHeight / perpendicularWallDistance);

            var drawStart = -lineHeight / 2 + _pixelGameEngine.ScreenHeight / 2;
            if (drawStart < 0) drawStart = 0;
            var drawEnd = lineHeight / 2 + _pixelGameEngine.ScreenHeight / 2;
            if (drawEnd >= _pixelGameEngine.ScreenHeight) drawEnd = _pixelGameEngine.ScreenHeight - 1;

            var color = Color.Green;

            switch (_map.MapNodes[mapX, mapY])
            {
                case 'x':
                        color = Color.DarkGreen;
                    break;
                case 'y':
                    color = Color.Blue;
                    if (side == 1)
                        color = Color.DarkBlue;
                    break;
                case 'z':
                    color = Color.Gray;
                    if (side == 1)
                        color = Color.DarkGray;
                    break;
            }

            DrawColumn(screenX, drawStart, drawEnd, color);
        }
        private void RotateCamera(bool rightDirection)
        {
            var rotationSpeed = RotationSpeed * (rightDirection ? -1 : 1);
            //both camera direction and camera plane must be rotated
            var oldDirX = _camera.DirectionVector.X;
            _camera.DirectionVector.X = _camera.DirectionVector.X * Math.Cos(rotationSpeed) -
                                        _camera.DirectionVector.Y * Math.Sin(rotationSpeed);
            _camera.DirectionVector.Y = oldDirX * Math.Sin(rotationSpeed) +
                                        _camera.DirectionVector.Y * Math.Cos(rotationSpeed);
            var oldPlaneX = _camera.CameraPlaneVector.X;
            _camera.CameraPlaneVector.X = _camera.CameraPlaneVector.X * Math.Cos(rotationSpeed) -
                                          _camera.CameraPlaneVector.Y * Math.Sin(rotationSpeed);
            _camera.CameraPlaneVector.Y = oldPlaneX * Math.Sin(rotationSpeed) +
                                          _camera.CameraPlaneVector.Y * Math.Cos(rotationSpeed);
        }

        private void MovePlayer(bool forward)
        {
            var directionVectorDirectionMultiplier = forward ? 1 : -1;
            if (_map.MapNodes[
                    (int)(_camera.PositionVector.X +
                           _camera.DirectionVector.X * directionVectorDirectionMultiplier * MoveSpeed),
                    (int)_camera.PositionVector.Y] ==
                _map.Config.EmptyChar)
                _camera.PositionVector.X += _camera.DirectionVector.X * directionVectorDirectionMultiplier * MoveSpeed;
            if (_map.MapNodes[(int)_camera.PositionVector.X,
                    (int)(_camera.PositionVector.Y +
                           _camera.DirectionVector.Y * directionVectorDirectionMultiplier * MoveSpeed)] ==
                _map.Config.EmptyChar)
                _camera.PositionVector.Y += _camera.DirectionVector.Y * directionVectorDirectionMultiplier * MoveSpeed;
        }
        private void ProcessInput()
        {
            var keyboard = Keyboard.GetState();

            if(keyboard.IsKeyDown(Key.W))
            {
                MovePlayer(true);
            }

            if (keyboard.IsKeyDown(Key.S))
            {
                MovePlayer(false);
            }

            if (keyboard.IsKeyDown(Key.D))
            {
                RotateCamera(true);
            }
            if (keyboard.IsKeyDown(Key.A))
            {
                RotateCamera(false);
            }
            
        }
        internal void DrawColumn(int x, long drawStart, long drawEnd, Color color)
        {

            if (drawStart < 0 || drawEnd < 0 || drawEnd > _pixelGameEngine.ScreenHeight || drawStart > drawEnd)
            {
                drawStart = 0;
                drawEnd = _pixelGameEngine.ScreenHeight;
            }

            // Draw above box
            if (0 < drawStart)
            {
                var color2 = new Pixel { a = Color.DarkSlateGray.A, b = Color.DarkSlateGray.B, r = Color.DarkSlateGray.R, g = Color.DarkSlateGray.G };
                _pixelGameEngine.DrawLine(x, 0, x, (int)drawStart, color2);
            }

            // Draw below box
            if (drawEnd < _pixelGameEngine.ScreenHeight)
            {
                var darkestColor = new Pixel { a = Color.Black.A, b = Color.Black.B, r = Color.Black.R, g = Color.Black.G };
                var lighterColor = new Pixel { a = Color.DimGray.A, b = Color.DimGray.B, r = Color.DimGray.R, g = Color.DimGray.G };

                var middleY = (int)(((float) 0.3 / (float) _pixelGameEngine.ScreenWidth) * MathF.Pow(x - (float)_pixelGameEngine.ScreenWidth / 2, 2) + (float)_pixelGameEngine.ScreenHeight / 1.5f);
                
                _pixelGameEngine.DrawLine(x, middleY + 1, x, (int)_pixelGameEngine.ScreenHeight, darkestColor);
                _pixelGameEngine.DrawLine(x, (int)drawEnd,x, middleY, lighterColor);
            }

            // Draw box
            var color3 = new Pixel {a = color.A, b = color.B, r = color.R, g = color.G};
            _pixelGameEngine.DrawLine(x, (int)drawStart, x, (int)drawEnd, color3);
        }
    }
}