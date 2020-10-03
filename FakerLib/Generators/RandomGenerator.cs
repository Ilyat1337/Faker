using GeneratorInterface;
using System;

namespace FakerLib.Generators
{
    abstract class RandomGenerator : IGenerator
    {
        private static Random random;

        public abstract object Generate();

        protected Random GetRandom()
        {
            if (random != null)
                return random;
            return random = new Random();
        }
    }
}
