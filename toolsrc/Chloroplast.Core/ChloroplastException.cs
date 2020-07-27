using System;
namespace Chloroplast.Core
{
    public class ChloroplastException : ApplicationException
    {
        public ChloroplastException (string message) : base (message) { }
        public ChloroplastException (string message, Exception inner) : base (message, inner) { }
    }
}
