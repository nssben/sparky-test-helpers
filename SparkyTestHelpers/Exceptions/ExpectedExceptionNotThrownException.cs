using System;

namespace SparkyTestHelpers.Exceptions
{
    public class ExpectedExceptionNotThrownException : Exception
    {
        public ExpectedExceptionNotThrownException(string msg) : base(msg)
        {
        }
    }
}