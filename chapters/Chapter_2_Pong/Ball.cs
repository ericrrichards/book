using System.Drawing;
using Chapter2Pong.Engine;
using SlimDX;
using SlimDX.Direct2D;

namespace Chapter2Pong {
    class Ball {
        private float _maxBallSpeed = 200.0f;
        private Vector2 _ballPos;
        private Vector2 _lastPallPos;
        private Vector2 _ballVelocity;
        private RectangleF _gameBounds;

        private Ellipse Ellipse { get { return new Ellipse() { Center = _ballPos.ToPointF(), RadiusX = 5, RadiusY = 5 }; } }

        public Ball(RectangleF gameBounds) {
            _gameBounds = gameBounds;
        }

        public void Reset(ServingPlayer server) {
            _ballPos = new Vector2(_gameBounds.Left + _gameBounds.Width / 2, _gameBounds.Top + _gameBounds.Height / 2);
            _ballVelocity = new Vector2(server == ServingPlayer.LeftPlayer ? 1 : -1, Util.Random(-0.75f, 0.75f));
            _ballVelocity = Vector2.Normalize(_ballVelocity) * _maxBallSpeed;
        }

        public void Update(float dt) {
            _lastPallPos = _ballPos;
            _ballPos = _ballPos + _ballVelocity * dt;
        }

        public void Draw(RenderTarget renderTarget) {
            using (var whiteBrush = new SolidColorBrush(renderTarget, Color.White)) {
                renderTarget.FillEllipse(whiteBrush, Ellipse);
            }

        }
        public bool BallOutTop() {
            return Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Top), new Vector2(_gameBounds.Right, _gameBounds.Top));
        }

        public bool BallOutBottom() {
            return Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Bottom), new Vector2(_gameBounds.Right, _gameBounds.Bottom));
        }

        public bool BallOutOnRight() {
            return Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(_gameBounds.Right, _gameBounds.Top), new Vector2(_gameBounds.Right, _gameBounds.Bottom));
        }

        public bool BallOutOnLeft() {
            return Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Top), new Vector2(_gameBounds.Left, _gameBounds.Bottom));
        }

        public void BounceX() {
            _ballVelocity.Y = -_ballVelocity.Y;
            _ballPos = _lastPallPos;
        }

        public void BounceY() {
            _ballVelocity.X = -_ballVelocity.X;
            _ballPos = _lastPallPos;
        }

        public bool HitPaddle(RectangleF paddle) {
            return
                Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(paddle.Right, paddle.Top), new Vector2(paddle.Right, paddle.Bottom))
                || Util.CircleIntersectsLine(_ballPos, Ellipse.RadiusX, new Vector2(paddle.Left, paddle.Top), new Vector2(paddle.Left, paddle.Bottom));
        }
    }
}