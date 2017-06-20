using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Cameras
{
    public class IsometricCamera : Camera
    {
        private Vector3 _cameraFinalTarget;
        private float _leftRightRotation = 0f;
        private float _upDownRotation = 0f;

        public Quaternion Rotation;
        public float Direction { get; private set; }
        public float Pitch { get; private set; }

        public IsometricCamera(TechCraftGame game)
                : base(game)
        {

        }

        public IsometricCamera(TechCraftGame game, Vector3 position, float direction, float pitch)
                : base(game)
        {
            Position = position;
            Direction = MathHelper.WrapAngle(direction);
            Pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2, MathHelper.PiOver2);
            Rotation = Quaternion.CreateFromYawPitchRoll(Direction, Pitch, 0.0F);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            Direction -= MathHelper.WrapAngle((float)(mouseState.X - Game.GraphicsDevice.DisplayMode.Width / 2) / 200);
            Pitch = MathHelper.Clamp(Pitch - (float)(mouseState.Y - Game.GraphicsDevice.DisplayMode.Height / 2) / 200, -MathHelper.PiOver2, MathHelper.PiOver2);

            Mouse.SetPosition(1920 / 2, 1200 / 2);


            Rotation = Quaternion.CreateFromYawPitchRoll(Direction, Pitch, 0.0F);

            //CalculateView();
            base.Update(gameTime);
        }

        public Matrix GetView()
        {   
            return Matrix.CreateLookAt(
                /*cam pos*/     Position,
                /*Look pos*/    Position + Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Direction)),
                /*Up dir*/      Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Direction)));

        }

        public float LeftRightRotation
        {
            get { return _leftRightRotation; }
            set
            {
                _leftRightRotation = value;
                //CalculateView();
            }
        }

        public float UpDownRotation
        {
            get { return _upDownRotation; }
            set
            {
                _upDownRotation = value;
                //CalculateView();
            }
        }

        protected override void CalculateView()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(_upDownRotation) * Matrix.CreateRotationY(_leftRightRotation);
            Vector3 cameraRotatedTarget = Vector3.Transform(Vector3.Forward, rotationMatrix);
            _cameraFinalTarget = Position + cameraRotatedTarget;
            Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, rotationMatrix);
            View = Matrix.CreateLookAt(Position, _cameraFinalTarget, cameraRotatedUpVector);

            base.CalculateView();
        }

    }
}
