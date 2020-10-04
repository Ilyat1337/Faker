namespace FakerLib.Generators
{
    class DoubleGenerator : RandomGenerator
    {
        public override object Generate()
        {
            return GetRandom().NextDouble();
        }
    }
}
