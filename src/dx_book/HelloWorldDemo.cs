using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.DirectWrite;
using FontStyle = SlimDX.DirectWrite.FontStyle;

namespace dx_book {
    class HelloWorldDemo : App {
        private bool _disposed;
        private SolidColorBrush _greenSolidBrush;
        private TextFormat _defaultTextFormat;

        public HelloWorldDemo(IntPtr handle)
            : base(handle) {
            MainWindowCaption = "Hello World Direct2D & DirectWrite";
            Enable4XMsaa = true;
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    Util.ReleaseCom(ref _greenSolidBrush);
                    Util.ReleaseCom(ref _defaultTextFormat);
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
                FontWeight.Normal, 
                FontStyle.Normal, 
                FontStretch.Normal, 
                14.0f, 
                "en-us"
            );
            _defaultTextFormat.TextAlignment = TextAlignment.Center;
            _defaultTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

            FpsCounter.Visible = true;

            return true;
        }

        protected override void OnMouseDown(object sender, MouseEventArgs e) {
            FpsCounter.Visible = !FpsCounter.Visible;

        }

        protected override void DrawScene() {
            D2DRenderTarget.BeginDraw();
            D2DRenderTarget.DrawText("Hello World!", _defaultTextFormat, new Rectangle(0, 0, ClientWidth, ClientHeight), _greenSolidBrush);
            D2DRenderTarget.EndDraw();
        }

        static void Main(string[] args) {
            Configuration.EnableObjectTracking = true;
            var app = new HelloWorldDemo(Process.GetCurrentProcess().Handle);
            if (!app.Init()) {
                return;
            }
            app.Run();
        }
    }
}
