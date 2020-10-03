namespace FakerLib.Generators
{
    class IntGenerator : RandomGenerator
    {
        public override object Generate()
        {
            return GetRandom().Next();
        }
    }
}
