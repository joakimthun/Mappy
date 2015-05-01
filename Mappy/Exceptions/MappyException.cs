using System;

namespace Mappy.Exceptions
{
    internal class MappyException : Exception
    {
        public MappyException(string format, params object[] args) : base(string.Format(format, args))
        {   
        }
    }
}
