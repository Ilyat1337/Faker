using System;

namespace FakerLib.Generators
{
    class ByteGenerator : RandomGenerator
    {
        public override object Generate()
        {
            return Convert.ToByte(GetRandom().Next());
        }
    }
}
