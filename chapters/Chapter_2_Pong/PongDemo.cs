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


        SolidColorBrush _whiteBrush;
        private StrokeStyle _strokeStyle;

        private int _leftScore;
        private int _rightScore;

        private Vector2 _lpCenter;
        private int _lpWidth = 50;
        private Vector2 _rpCenter;
        private int _rpWidth = 50;
        private Ball _ball;

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

            FpsCounter.Visible = true;

            _whiteBrush = new SolidColorBrush(D2DRenderTarget, Color.White);
            _strokeStyle = new StrokeStyle(
                Direct2DFactory,
                new StrokeStyleProperties {
                    DashStyle = DashStyle.Dash
                }
            );

            var bounds = Window.ClientRectangle;

            var marginSides = bounds.Width / 10;
            var marginTop = bounds.Height / 5;

            _gameBounds = new Rectangle(bounds.Left + marginSides, bounds.Top + marginTop, bounds.Width - (2 * marginSides), bounds.Height - (2 * marginTop));
            _ball = new Ball(_gameBounds);
            _ball.Reset(Util.RandomBool() ? ServingPlayer.LeftPlayer : ServingPlayer.RightPlayer);


            _lpCenter = new Vector2(_gameBounds.Left + 10, _gameBounds.Top + _gameBounds.Height *0.5f);
            _rpCenter = new Vector2(_gameBounds.Right - 10, _gameBounds.Top + _gameBounds.Height * 0.5f);

            return true;
        }

        protected override void UpdateScene(float dt) {
            base.UpdateScene(dt);

            _ball.Update(dt);

            if (_ball.HitPaddle(LeftPaddle)){
                _ball.BounceY();
            } else if (_ball.HitPaddle(RightPaddle)) {
                _ball.BounceY();
            }

            if (_ball.BallOutOnLeft()) {
                _rightScore++;
                _ball.Reset(ServingPlayer.LeftPlayer);
            } else if (_ball.BallOutOnRight()) {
                _leftScore++;
                _ball.Reset(ServingPlayer.RightPlayer);
            } else if (_ball.BallOutTop() || _ball.BallOutBottom()) {
                _ball.BounceX();
                
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

            _ball.Draw(D2DRenderTarget);

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
