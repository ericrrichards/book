using SlimDX.Direct2D;

namespace Chapter2Pong.Engine {
    /// <summary>
    /// Base class for custom Direct2D objects that need to re-init themselves when the graphics device is lost
    /// </summary>
    public abstract class D2DComponent : DisposableClass {
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
        /// <param name="renderTarget"></param>
        public abstract void AquireResources(RenderTarget renderTarget);
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