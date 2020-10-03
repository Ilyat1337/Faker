using System;
using System.Collections.Generic;
using System.Text;

namespace FakerLib.ContainerGenerators
{
    abstract class RandomContainerGenerator : IContainerGenerator
    {
        protected Random random;

        public abstract object Generate(Type elementType);

        protected Random GetRandom()
        {
            if (random != null)
                return random;
            return random = new Random();
        }
    }
}
