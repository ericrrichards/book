using System.Drawing;
using System.Windows.Forms;
using Chapter2Pong.Engine;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.DirectWrite;
using FontStyle = SlimDX.DirectWrite.FontStyle;

namespace Chapter2Pong {
    enum ServingPlayer {
        LeftPlayer,
        RightPlayer
    }

    class PongDemo : App {
        private bool _disposed;
        private SolidColorBrush _greenSolidBrush;
        private TextFormat _defaultTextFormat;

        private RectangleF _gameBounds;

        private float _maxSpeed = 100.0f;
        private float _maxBallSpeed = 200.0f;

        private Vector2 _ballPos;
        private Vector2 _ballVelocity;

        
        SolidColorBrush _whiteBrush;
        private StrokeStyle _strokeStyle;
        private Ellipse _ellipse;

        private int _leftScore;
        private int _rightScore;

        private Vector2 _lpCenter;
        private int _lpWidth = 50;
        private Vector2 _rpCenter;
        private int _rpWidth = 50;

        private const int PaddleDepth = 10;


        private PongDemo() {
            MainWindowCaption = "Pong.net";
            Enable4XMsaa = true;
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    Util.ReleaseCom(ref _greenSolidBrush);
                    Util.ReleaseCom(ref _defaultTextFormat);

                    Util.ReleaseCom(ref _whiteBrush);
                    Util.ReleaseCom(ref _strokeStyle);
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        protected override bool Init() {
            if (!base.Init()) {
                return false;
            }
            _greenSolidBrush = new SolidColorBrush(D2DRenderTarget, Color.Lime);
            _defaultTextFormat = DirectWriteFactory.CreateTextFormat(
                "Consolas",
                FontWeight.Bold,
                FontStyle.Normal,
                FontStretch.Normal,
                20.0f,
                "en-us"
            );
            _defaultTextFormat.TextAlignment = TextAlignment.Center;
            _defaultTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

            //FpsCounter.Visible = true;

            var bounds = Window.ClientRectangle;

            var marginSides = bounds.Width / 10;
            var marginTop = bounds.Height / 5;

            _gameBounds = new Rectangle(bounds.Left + marginSides, bounds.Top + marginTop, bounds.Width - (2 * marginSides), bounds.Height - (2 * marginTop));

            ResetBall(Util.RandomBool() ? ServingPlayer.LeftPlayer : ServingPlayer.RightPlayer);

            _whiteBrush = new SolidColorBrush(D2DRenderTarget, Color.White);
            _strokeStyle = new StrokeStyle(
                Direct2DFactory,
                new StrokeStyleProperties {
                    DashStyle = DashStyle.Dash
                }
            );

            _lpCenter = new Vector2(_gameBounds.Left + 10, _gameBounds.Top + _gameBounds.Height *0.5f);
            _rpCenter = new Vector2(_gameBounds.Right - 10, _gameBounds.Top + _gameBounds.Height * 0.5f);

            return true;
        }

        private void ResetBall(ServingPlayer server) {
            _ballPos = new Vector2(_gameBounds.Left + _gameBounds.Width / 2, _gameBounds.Top + _gameBounds.Height / 2);
            _ellipse = new Ellipse() { Center = _ballPos.ToPointF(), RadiusX = 5, RadiusY = 5 };
            _ballVelocity = new Vector2(server == ServingPlayer.LeftPlayer ? 1 : -1, Util.Random(-0.75f, 0.75f));
            _ballVelocity = Vector2.Normalize(_ballVelocity) * _maxBallSpeed;
        }

        protected override void UpdateScene(float dt) {
            base.UpdateScene(dt);

            _ballPos = _ballPos + _ballVelocity * dt;
            _ellipse.Center = _ballPos.ToPointF();

            if (CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(LeftPaddle.Right, LeftPaddle.Top), new Vector2(LeftPaddle.Right, LeftPaddle.Bottom))) {
                _ballVelocity.X = -_ballVelocity.X;
            } else if (CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(RightPaddle.Left, RightPaddle.Top), new Vector2(RightPaddle.Left, RightPaddle.Bottom))) {
                _ballVelocity.X = -_ballVelocity.X;
            }

            if (BallOutOnLeft()) {
                _rightScore++;
                ResetBall(ServingPlayer.LeftPlayer);
            } else if (BallOutOnRight()) {
                _leftScore++;
                ResetBall(ServingPlayer.RightPlayer);
            } else if (BallOutTop() || BallOutBottom()) {
                _ballVelocity.Y = -_ballVelocity.Y;
            }

            var oldLp = _lpCenter;
            if (Util.IsKeyDown(Keys.Q)) {
                _lpCenter.Y -= _maxSpeed * dt;
            } else if (Util.IsKeyDown(Keys.A)) {
                _lpCenter.Y += _maxSpeed * dt;
            }
            if (!_gameBounds.Contains(LeftPaddle)) {
                _lpCenter = oldLp;
            }

            var oldRp = _rpCenter;
            if (Util.IsKeyDown(Keys.Up)) {
                _rpCenter.Y -= _maxSpeed * dt;
            } else if (Util.IsKeyDown(Keys.Down)) {
                _rpCenter.Y += _maxSpeed * dt;
            }
            if (!_gameBounds.Contains(RightPaddle)) {
                _rpCenter = oldRp;
            }

        }

        private RectangleF RightPaddle { get { return new RectangleF(_rpCenter.X - PaddleDepth * 0.5f, _rpCenter.Y - _rpWidth * 0.5f, PaddleDepth, _rpWidth); } }

        private RectangleF LeftPaddle { get { return new RectangleF(_lpCenter.X - PaddleDepth * 0.5f, _lpCenter.Y - _lpWidth * 0.5f, PaddleDepth, _lpWidth); } }

        private bool BallOutTop() {
            return CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Top), new Vector2(_gameBounds.Right, _gameBounds.Top));
        }
        private bool BallOutBottom() {
            return CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Bottom), new Vector2(_gameBounds.Right, _gameBounds.Bottom));
        }
        private bool BallOutOnRight() {
            return CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Right, _gameBounds.Top), new Vector2(_gameBounds.Right, _gameBounds.Bottom));
        }

        private bool BallOutOnLeft() {
            return CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Top), new Vector2(_gameBounds.Left, _gameBounds.Bottom));
        }

        public bool CircleIntersectsLine(Vector2 center, float r, Vector2 p0, Vector2 p1) {
            var ac = center - p0;
            var ab = p1 - p0;

            var ab2 = Vector2.Dot(ab, ab);
            var acab = Vector2.Dot(ac, ab);

            var t = acab / ab2;
            if (t < 0) {
                t = 0;
            } else if (t > 1) {
                t = 1;
            }
            var h = ((ab*t) + p0) - center;
            var h2 = Vector2.Dot(h, h);
            return h2 < (r*r);
        }

        protected override void DrawScene() {


            D2DRenderTarget.BeginDraw();
            D2DRenderTarget.DrawText("Pong!", _defaultTextFormat, new Rectangle(0, 0, ClientWidth, 35), _greenSolidBrush);

            D2DRenderTarget.DrawRectangle(_whiteBrush, _gameBounds, 2);
            var x1 = (_gameBounds.Left + _gameBounds.Width * 0.5f);
            var p0 = new PointF(x1, _gameBounds.Top);
            var p1 = new PointF(x1, _gameBounds.Bottom);
            D2DRenderTarget.DrawLine(_whiteBrush, p0, p1, 2, _strokeStyle);

            
            D2DRenderTarget.FillRectangle(_whiteBrush, LeftPaddle);
            D2DRenderTarget.FillRectangle(_whiteBrush, RightPaddle);

            D2DRenderTarget.FillEllipse(_whiteBrush, _ellipse);

            D2DRenderTarget.DrawText(_leftScore.ToString(), _defaultTextFormat, new Rectangle(10, 10, 50,50), _whiteBrush );
            D2DRenderTarget.DrawText(_rightScore.ToString(), _defaultTextFormat, new Rectangle(ClientWidth-60, 10, 50, 50), _whiteBrush);
            D2DRenderTarget.EndDraw();
        }

        static void Main() {
            Configuration.EnableObjectTracking = true;
            var app = new PongDemo();
            if (!app.Init()) {
                return;
            }
            app.Run();
        }
    }
}
