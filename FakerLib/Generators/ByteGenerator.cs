using System;

namespace FakerLib.Generators
{
    class ByteGenerator : RandomGenerator
    {
        private static readonly int MAX_BYTE_VALUE = 255;

        public override object Generate()
        {
            return Convert.ToByte(GetRandom().Next(MAX_BYTE_VALUE + 1));
        }
    }
}
