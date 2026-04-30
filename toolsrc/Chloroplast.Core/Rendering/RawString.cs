namespace Chloroplast.Core.Rendering
{
    /// <summary>
    /// Represents a string that should be written to a template output without HTML encoding.
    /// </summary>
    public class RawString
    {
        private readonly string _value;

        public RawString (string value)
        {
            _value = value ?? string.Empty;
        }

        public override string ToString () => _value;
    }
}
