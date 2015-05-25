using System;

namespace Mappy.Exceptions
{
    internal class MappyException : Exception
    {
        public MappyException(string message) : base(message)
        {   
        }
    }
}
