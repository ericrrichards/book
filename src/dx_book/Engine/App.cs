using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Factory = SlimDX.Direct2D.Factory;
using FeatureLevel = SlimDX.Direct3D11.FeatureLevel;
using Resource = SlimDX.Direct3D11.Resource;

namespace dx_book {
    public class App : DisposableClass {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static App GApp;

        private IntPtr AppInst { get; set; }
        private bool _disposed;
        private bool _running;
        private bool _appPaused;
        private bool _minimized;
        private bool _maximized;
        private bool _resizing;
        private bool _fullscreen;


        protected D3DForm Window { get; set; }
        protected string MainWindowCaption { get; set; }
        protected internal RectangleF ClientBounds { get { return Window.ClientRectangle; } }

        public Color4 ClearColor { get; set; }
        public float DepthClearValue { get; private set; }
        public byte StencilClearValue { get; private set; }

        protected internal int ClientWidth;
        protected internal int ClientHeight;



        protected int Msaa4XQuality;
        protected bool Enable4XMsaa;
        protected Format _backBufferFormat;
        protected DriverType DriverType;


        protected Device Device;
        protected DeviceContext ImmediateContext;
        protected SwapChain SwapChain;
        private Texture2D _depthStencilBuffer;
        protected RenderTargetView RenderTargetView;
        protected DepthStencilView DepthStencilView;
        protected Viewport Viewport;
        protected internal RenderTarget D2DRenderTarget;

        

        public static Factory Direct2DFactory;
        public static SlimDX.DirectWrite.Factory DirectWriteFactory;

        


        protected readonly FrameRateCounter FpsCounter;
        protected readonly List<DxComponent> Components;
        


        protected App(IntPtr handle) {
            XmlConfigurator.Configure();
            AppInst = handle;
            MainWindowCaption = "D3D11 Application";
            DriverType = DriverType.Hardware;
            _backBufferFormat = Format.R8G8B8A8_UNorm_SRGB;
            ClientWidth = 800;
            ClientHeight = 600;
            Viewport = new Viewport();
            FpsCounter = new FrameRateCounter {
                Bounds = new Rectangle(0, 0, ClientWidth, 0)
            };
            GApp = this;

            Components = new List<DxComponent>();
            FindDxComponents();

            ClearColor = Color.Black;
            DepthClearValue = 1.0f;
            StencilClearValue = 0;
        }

        private void FindDxComponents() {
            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Log.Debug("DxComponents:");
            foreach (var fieldInfo in fields.Where(fieldInfo => fieldInfo.FieldType.IsSubclassOf(typeof(DxComponent)))) {
                Log.DebugFormat("{0} - {1}", fieldInfo.FieldType, fieldInfo.Name);
                Components.Add(fieldInfo.GetValue(this) as DxComponent);
            }
        }

        protected override void Dispose(bool disposing) {
            Log.Info("Disposing of App");
            if (!_disposed) {
                if (disposing) {
                    Util.ReleaseCom(ref RenderTargetView);
                    Util.ReleaseCom(ref DepthStencilView);

                    Util.ReleaseCom(ref _depthStencilBuffer);
                    if (ImmediateContext != null) {
                        ImmediateContext.ClearState();
                    }

                    if (SwapChain.IsFullScreen) {
                        SwapChain.SetFullScreenState(false, null);
                    }
                    Util.ReleaseCom(ref SwapChain);
                    Util.ReleaseCom(ref ImmediateContext);
                    Util.ReleaseCom(ref Device);

                    Util.ReleaseCom(ref Direct2DFactory);
                    Util.ReleaseCom(ref DirectWriteFactory);

                    Util.ReleaseCom(ref D2DRenderTarget);


                    foreach (var component in Components) {
                        var dxComponent = component;
                        Util.ReleaseCom(ref dxComponent);
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        protected virtual bool Init() {
            if (!InitMainWindow()) {
                return false;
            }
            if (!InitDirect3D()) {
                return false;
            }

            Window.KeyDown += HandleAltEnter();


            _running = true;

            Log.Info("App initialized");
            return true;
        }

        private KeyEventHandler HandleAltEnter() {
            return (o, e) => {
                if (e.Alt && e.KeyCode == Keys.Enter) {
                    _resizing = true;
                    _fullscreen = !_fullscreen;
                    if (_fullscreen) {
                        Log.Info("Entering full-screen mode");
                        Window.FormBorderStyle = FormBorderStyle.None;
                        Window.WindowState = FormWindowState.Maximized;
                    } else {
                        Log.Info("Leaving full-screen mode");
                        Window.WindowState = FormWindowState.Normal;
                        Window.FormBorderStyle = FormBorderStyle.Sizable;
                        
                    }
                    _resizing = false;
                }
            };
        }

        private bool InitMainWindow() {
            try {
                Window = new D3DForm {
                    Text = MainWindowCaption,
                    Name = "D3DWndClassName",
                    FormBorderStyle = FormBorderStyle.Sizable,
                    ClientSize = new Size(ClientWidth, ClientHeight),
                    StartPosition = FormStartPosition.CenterScreen,
                    MyWndProc = WndProc,
                    MinimumSize = new Size(200, 200),
                };
                Window.MouseDown += OnMouseDown;
                Window.MouseUp += OnMouseUp;
                Window.MouseMove += OnMouseMove;
                Window.MouseWheel += OnMouseWheel;
                Window.ResizeBegin += (sender, args) => {
                    _appPaused = true;
                    _resizing = true;
                    FpsCounter.Stop();
                };
                Window.ResizeEnd += (sender, args) => {
                    _appPaused = false;
                    _resizing = false;
                    FpsCounter.Start();
                    OnResize();
                };


                Window.Show();
                Window.Update();
                return true;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error");
                return false;
            }
        }

        private bool InitDirect3D() {
            var creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif
            try {
                Device = new Device(DriverType, creationFlags);
                

            } catch (Exception ex) {
                MessageBox.Show("D3D11Device creation failed\n" + ex.Message + "\n" + ex.StackTrace, "Error");
                return false;
            }
            ImmediateContext = Device.ImmediateContext;
            if (Device.FeatureLevel != FeatureLevel.Level_11_0) {
                Console.WriteLine("Direct3D Feature Level 11 unsupported\nSupported feature level: " + Enum.GetName(Device.FeatureLevel.GetType(), Device.FeatureLevel));

            }
            try {
                var sd = new SwapChainDescription {
                    ModeDescription = new ModeDescription(ClientWidth, ClientHeight, new Rational(60, 1), _backBufferFormat) {
                        ScanlineOrdering = DisplayModeScanlineOrdering.Unspecified,
                        Scaling = DisplayModeScaling.Unspecified
                    },
                    SampleDescription = Enable4XMsaa && Device.FeatureLevel >= FeatureLevel.Level_10_1 ? new SampleDescription(4, Msaa4XQuality - 1) : new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                    BufferCount = 1,
                    OutputHandle = Window.Handle,
                    IsWindowed = true,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.AllowModeSwitch
                };
                SwapChain = new SwapChain(Device.Factory, Device, sd);

                using (var factory = SwapChain.GetParent<SlimDX.DXGI.Factory>()) {
                    factory.SetWindowAssociation(Window.Handle, WindowAssociationFlags.IgnoreAltEnter);
                }
            } catch (Exception ex) {
                MessageBox.Show("SwapChain creation failed\n" + ex.Message + "\n" + ex.StackTrace, "Error");
                return false;
            }


            Direct2DFactory = new Factory(FactoryType.Multithreaded, DebugLevel.Information);
            DirectWriteFactory = new SlimDX.DirectWrite.Factory(SlimDX.DirectWrite.FactoryType.Shared);

            OnResize();

            Log.Info("DirectX initialized");

            return true;
        }

        // ReSharper disable InconsistentNaming
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_SIZE = 0x0005;
        private const int WM_DESTROY = 0x0002;
        private const int WM_MENUCHAR = 0x0120;
        private const int MNC_CLOSE = 1;
        // ReSharper restore InconsistentNaming

        private bool WndProc(ref Message m) {
            switch (m.Msg) {
                case WM_ACTIVATE:
                    if (m.WParam.ToInt32().LowWord() == 0) {
                        _appPaused = true;
                        FpsCounter.Stop();
                    } else {
                        _appPaused = false;
                        FpsCounter.Start();
                    }
                    return true;
                case WM_SIZE:
                    ClientWidth = m.LParam.ToInt32().LowWord();
                    ClientHeight = m.LParam.ToInt32().HighWord();
                    if (Device != null) {
                        if (m.WParam.ToInt32() == 1) { // SIZE_MINIMIZED
                            _appPaused = true;
                            _minimized = true;
                            _maximized = false;
                        } else if (m.WParam.ToInt32() == 2) { // SIZE_MAXIMIZED
                            _appPaused = false;
                            _minimized = false;
                            _maximized = true;
                            OnResize();
                        } else if (m.WParam.ToInt32() == 0) { // SIZE_RESTORED
                            if (_minimized) {
                                _appPaused = false;
                                _minimized = false;
                                OnResize();
                            } else if (_maximized) {
                                _appPaused = false;
                                _maximized = false;
                                OnResize();
                            } else if (_resizing) {

                            } else {
                                OnResize();
                            }
                        }
                    }
                    return true;
                case WM_DESTROY:
                    _running = false;
                    return true;
                case WM_MENUCHAR:
                    // disable Alt-Enter beep
                    m.Result = new IntPtr(MNC_CLOSE << 16); 
                    return true;
            }
            return false;
        }
        protected virtual void OnMouseWheel(object sender, MouseEventArgs e) { }
        protected virtual void OnMouseMove(object sender, MouseEventArgs e) { }
        protected virtual void OnMouseUp(object sender, MouseEventArgs e) { }
        protected virtual void OnMouseDown(object sender, MouseEventArgs e) { }

        protected virtual void OnResize() {
            Log.Debug("Resizing");

            // release all the objects that reference the backbuffer surface
            Util.ReleaseCom(ref RenderTargetView);
            Util.ReleaseCom(ref DepthStencilView);
            Util.ReleaseCom(ref _depthStencilBuffer);
            Util.ReleaseCom(ref D2DRenderTarget);

            Log.Debug("Releasing DxComponents");
            foreach (var dxComponent in Components) {
                dxComponent.ReleaseResources();
            }

            ImmediateContext.ClearState();
            SwapChain.ResizeBuffers(0, ClientWidth, ClientHeight, Format.Unknown, SwapChainFlags.AllowModeSwitch);
            Log.Debug("SwapChain buffers resized");
            // NOTE usings here are very important - DXGIException in the block above when calling SwapChain.ResizeBuffers()
            // if all references to the swap chain backbuffer are not released
            // Getting the Texture2D and DXGI.Surface here are tricky, because it's not immediately obvious that a disposable reference is created when they are obtained
            using (var renderTarget = Resource.FromSwapChain<Texture2D>(SwapChain, 0)) {
                RenderTargetView = new RenderTargetView(Device, renderTarget);
                RenderTargetView.Resource.DebugName = "main render target";

                using (var renderSurface = renderTarget.AsSurface()) {
                    D2DRenderTarget = RenderTarget.FromDXGI(
                        Direct2DFactory,
                        renderSurface,
                        new RenderTargetProperties {
                            Type = RenderTargetType.Default,
                            PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                            HorizontalDpi = Direct2DFactory.DesktopDpi.Width,
                            VerticalDpi = Direct2DFactory.DesktopDpi.Height
                        }
                    );
                    Log.Debug("RenderTarget resized");
                }
            }

            var depthStencilDesc = new Texture2DDescription {
                Width = ClientWidth,
                Height = ClientHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = (Enable4XMsaa && Device.FeatureLevel >= FeatureLevel.Level_10_1) ? new SampleDescription(4, Msaa4XQuality - 1) : new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            _depthStencilBuffer = new Texture2D(Device, depthStencilDesc) { DebugName = "DepthStencilBuffer" };
            DepthStencilView = new DepthStencilView(Device, _depthStencilBuffer);
            Log.Debug("DepthStencil resized");

            ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);

            Viewport = new Viewport(0, 0, ClientWidth, ClientHeight, 0.0f, 1.0f);

            ImmediateContext.Rasterizer.SetViewports(Viewport);

            Log.Debug("Re-aquiring DxComponents");
            foreach (var dxComponent in Components) {
                dxComponent.AquireResources();
            }

        }

        protected virtual void UpdateScene(float dt) { }
        protected virtual void DrawScene() { }
        private void DrawSceneInternal() {
            ImmediateContext.ClearRenderTargetView(RenderTargetView, ClearColor);
            
            ImmediateContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, DepthClearValue, StencilClearValue);

            DrawScene();

            D2DRenderTarget.BeginDraw();

            foreach (var dxComponent in Components.Where(c=>c.Visible)) {
                dxComponent.Draw(D2DRenderTarget);
            }
            D2DRenderTarget.EndDraw();

            SwapChain.Present(0, PresentFlags.None);
        }
        public void Run() {
            FpsCounter.Reset();
            while (_running) {
                Application.DoEvents();
                FpsCounter.Tick();

                if (!_appPaused) {
                    FpsCounter.CalculateFrameRate();
                    UpdateScene(FpsCounter.DeltaTime);
                    DrawSceneInternal();
                } else {
                    Thread.Sleep(100);
                }
            }
            Dispose();
        }

        
    }


}