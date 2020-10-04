using System;

namespace FakerLib
{
    public class FakerConfigException : Exception
    {
        public FakerConfigException() { }
        public FakerConfigException(string message) : base(message) { }
    }
}
