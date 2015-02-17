using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Chapter1HelloWorld.Engine;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using FontStyle = SlimDX.DirectWrite.FontStyle;

namespace Chapter1HelloWorld {
    class HelloWorldDemo : App {
        private bool _disposed;
        private SolidColorBrush _greenSolidBrush;
        private TextFormat _defaultTextFormat;

        private Buffer _vertexBuffer;
        private const string ShaderCode = "float4 VShader(float4 position:POSITION):SV_POSITION { return position; }\n" +
                                          "float4 PShader(float4 position: SV_POSITION) : SV_Target { return float4(0.3f, 0.3f, 0.3f, 1.0f);}";
        private VertexShader _vShader;
        private PixelShader _pShader;
        private InputLayout _layout;

        private HelloWorldDemo() {
            MainWindowCaption = "Hello World Direct2D & DirectWrite";
            Enable4XMsaa = true;
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    Util.ReleaseCom(ref _greenSolidBrush);
                    Util.ReleaseCom(ref _defaultTextFormat);

                    Util.ReleaseCom(ref _vertexBuffer);
                    Util.ReleaseCom(ref _vShader);
                    Util.ReleaseCom(ref _pShader);
                    Util.ReleaseCom(ref _layout);
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


            var vertices = new DataStream(12 * 3, true, true);
            vertices.Write(new Vector3(0, 0.5f, 0.5f));
            vertices.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vertices.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Position = 0;
            _vertexBuffer = new Buffer(Device, vertices, 12 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            using (var bytecode = ShaderBytecode.Compile(Encoding.UTF8.GetBytes(ShaderCode), "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None)) {
                _vShader = new VertexShader(Device, bytecode);
                _layout = new InputLayout(Device, bytecode, new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) });
            }
            using (var bytecode = ShaderBytecode.Compile(Encoding.UTF8.GetBytes(ShaderCode), "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None)) {
                _pShader = new PixelShader(Device, bytecode);
            }

            return true;
        }

        protected override void OnMouseDown(object sender, MouseEventArgs e) {
            FpsCounter.Visible = !FpsCounter.Visible;

        }

        protected override void DrawScene() {

            ImmediateContext.InputAssembler.InputLayout = _layout;
            ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, 12, 0));
            ImmediateContext.VertexShader.Set(_vShader);
            ImmediateContext.PixelShader.Set(_pShader);

            ImmediateContext.Draw(3,0);

            D2DRenderTarget.BeginDraw();
            D2DRenderTarget.DrawText("Hello World!", _defaultTextFormat, new Rectangle(0, 0, ClientWidth, ClientHeight), _greenSolidBrush);
            D2DRenderTarget.EndDraw();
        }

        static void Main() {
            Configuration.EnableObjectTracking = true;
            var app = new HelloWorldDemo();
            if (!app.Init()) {
                return;
            }
            app.Run();
        }
    }
}
