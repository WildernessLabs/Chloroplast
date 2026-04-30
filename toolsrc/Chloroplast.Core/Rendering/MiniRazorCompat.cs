namespace MiniRazor
{
    /// <summary>
    /// Backward-compatible shim so legacy templates that import MiniRazor can
    /// continue to use RawString with the RazorLight-based renderer.
    /// </summary>
    public class RawString : Chloroplast.Core.Rendering.RawString
    {
        public RawString (string value) : base (value)
        {
        }
    }
}
