using System.Drawing;
using Chapter2Pong.Engine;
using SlimDX;
using SlimDX.Direct2D;

namespace Chapter2Pong {
    class Ball {
        private float _maxBallSpeed = 200.0f;
        private Vector2 _ballPos;
        private Vector2 _ballVelocity;
        private Ellipse _ellipse;
        private RectangleF _gameBounds;

        public Ball(RectangleF gameBounds) {
            _gameBounds = gameBounds;
        }

        public void Reset(ServingPlayer server) {
            _ballPos = new Vector2(_gameBounds.Left + _gameBounds.Width / 2, _gameBounds.Top + _gameBounds.Height / 2);
            _ellipse = new Ellipse() { Center = _ballPos.ToPointF(), RadiusX = 5, RadiusY = 5 };
            _ballVelocity = new Vector2(server == ServingPlayer.LeftPlayer ? 1 : -1, Util.Random(-0.75f, 0.75f));
            _ballVelocity = Vector2.Normalize(_ballVelocity) * _maxBallSpeed;
        }

        public void Update(float dt) {
            _ballPos = _ballPos + _ballVelocity * dt;
            _ellipse.Center = _ballPos.ToPointF();
        }
    }
}