using System.Drawing;
using Chapter1HelloWorld.Engine;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.DirectWrite;
using FontStyle = SlimDX.DirectWrite.FontStyle;

namespace Chapter1HelloWorld.Components {
    /// <summary>
    /// Simple component to record and display framerate
    /// </summary>
    public class FrameRateCounter :D2DComponent {
        private int _frameCount;
        private float _timeElapsed;
        private readonly GameTimer _timer;
        private Color4 _color;
        private SolidColorBrush _textBrush;
        private TextFormat _defaultTextFormat;

        /// <summary>
        /// Frames per secord for the last frame
        /// </summary>
        public float FPS {get; private set;}
        /// <summary>
        /// Time elapsed for the last frame
        /// </summary>
        public float FrameTime { get; private set; }

        /// <summary>
        /// Color used to render the framerate text
        /// </summary>
        public Color4 Color {
            get { return _color; }
            set {
                Util.ReleaseCom(ref _textBrush);
                try {
                    _textBrush = new SolidColorBrush(App.GApp.D2DRenderTarget, value);
                } catch {
                    // ignored, app is not initialized, brush will be created during AquireResources()
                }
                _color = value;
            }
        }
        /// <summary>
        /// Screen pixel-space area in which the component will be displayed
        /// </summary>
        public Rectangle Bounds { get; set; }


        public FrameRateCounter() {
            _timer = new GameTimer();
            Color = System.Drawing.Color.Lime;
        }
        /// <summary>
        /// Wrap GameTimer Start method
        /// </summary>
        public void Start() {
            _timer.Start();
        }
        /// <summary>
        /// Wrap GameTimer Stop method
        /// </summary>
        public void Stop() {
            _timer.Stop();
        }
        /// <summary>
        /// Wrap GameTimer Reset method
        /// </summary>
        public void Reset() {
            _timer.Reset();
        }
        /// <summary>
        /// Wrap GameTimer Tick method
        /// </summary>
        public void Tick() {
            _timer.Tick();
        }
        /// <summary>
        /// Time elapsed between two most recent frames
        /// </summary>
        public float DeltaTime { get { return _timer.DeltaTime; } }

        

        /// <summary>
        /// Calculate frame rate statistics
        /// </summary>
        public void CalculateFrameRate() {
            _frameCount++;
            if ((_timer.TotalTime - _timeElapsed) >= 1.0f) {
                FPS = _frameCount;
                FrameTime = 1000.0f / FPS;


                _frameCount = 0;
                _timeElapsed += 1.0f;
            }
        }
        /// <summary>
        /// Draw the framerate text at Bounds
        /// </summary>
        /// <param name="renderTarget"></param>
        public override void Draw(RenderTarget renderTarget) {
            var s = string.Format("FPS: {0} Frame Time: {1} (ms)", FPS, FrameTime);
            renderTarget.DrawText(s, _defaultTextFormat, Bounds, _textBrush);
        }

        /// <summary>
        /// Aquire device-dependent resources
        /// </summary>
        /// <param name="renderTarget"></param>
        public override void AquireResources(RenderTarget renderTarget) {
            _textBrush = new SolidColorBrush(renderTarget, Color);
            if (_defaultTextFormat == null) {
                _defaultTextFormat = App.DirectWriteFactory.CreateTextFormat("Consolas", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 14.0f, "en-us");
            }
        }
        /// <summary>
        /// Release device dependent resources
        /// </summary>
        public override void ReleaseResources() {
            Util.ReleaseCom(ref _textBrush);
            Util.ReleaseCom(ref _defaultTextFormat);
        }
    }
}