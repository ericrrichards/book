using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Chapter2Pong.Engine;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using FontStyle = SlimDX.DirectWrite.FontStyle;

namespace Chapter2Pong {
    class PongDemo : App {
        private bool _disposed;
        private SolidColorBrush _greenSolidBrush;
        private TextFormat _defaultTextFormat;

        private Rectangle _gameBounds;

        private float _maxSpeed = 50.0f;

        private Vector2 _ballPos;
        private Vector2 _ballVelocity;


        private Rectangle _paddle1;
        private Rectangle _paddle2;

        SolidColorBrush _whiteBrush;
        private StrokeStyle _strokeStyle;
        private Ellipse _ellipse;

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

            ResetBall();

            _whiteBrush = new SolidColorBrush(D2DRenderTarget, Color.White);
            _strokeStyle = new StrokeStyle(
                Direct2DFactory,
                new StrokeStyleProperties {
                    DashStyle = DashStyle.Dash
                }
            );


            return true;
        }

        private void ResetBall() {
            _ballPos = new Vector2(_gameBounds.Left + _gameBounds.Width / 2, _gameBounds.Top + _gameBounds.Height / 2);
            _ellipse = new Ellipse() { Center = _ballPos.ToPointF(), RadiusX = 5, RadiusY = 5 };
            _ballVelocity = new Vector2(Util.RandomBool() ? 1 : -1, Util.Random(-0.5f, 0.5f));
            _ballVelocity = Vector2.Normalize(_ballVelocity) * _maxSpeed;
        }

        protected override void UpdateScene(float dt) {
            base.UpdateScene(dt);

            _ballPos = _ballPos + _ballVelocity * dt;
            _ellipse.Center = _ballPos.ToPointF();

            if (Util.IsKeyDown(Keys.Space)) {
                ResetBall();
            }

            if (CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Left, _gameBounds.Top), new Vector2(_gameBounds.Left, _gameBounds.Bottom))) {
                ResetBall();
            } else if (CircleIntersectsLine(_ballPos, _ellipse.RadiusX, new Vector2(_gameBounds.Right, _gameBounds.Top), new Vector2(_gameBounds.Right, _gameBounds.Bottom))) {
                ResetBall();
            }
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

            D2DRenderTarget.FillEllipse(_whiteBrush, _ellipse);

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
