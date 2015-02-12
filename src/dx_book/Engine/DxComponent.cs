using System.Drawing;
using SlimDX.Direct2D;

namespace dx_book {
    /// <summary>
    /// Base class for custom DirectX objects that need to re-init themselves when the graphics device is lost
    /// </summary>
    public abstract class DxComponent : DisposableClass {
        private bool _disposed;

        /// <summary>
        /// Should the component be displayed?
        /// </summary>
        public bool Visible { get; set; }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    ReleaseResources();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Load device-dependent resources
        /// </summary>
        public abstract void AquireResources();
        /// <summary>
        /// Release device-dependent resources
        /// </summary>
        public abstract void ReleaseResources();
        /// <summary>
        /// Draw the component to this render target
        /// </summary>
        /// <param name="renderTarget">RenderTarget to draw component on</param>
        public abstract void Draw(RenderTarget renderTarget);
    }
}