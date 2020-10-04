using System;

namespace FakerLib
{
    class FakerConfigException : Exception
    {
        public FakerConfigException() { }
        public FakerConfigException(string message) : base(message) { }
    }
}
