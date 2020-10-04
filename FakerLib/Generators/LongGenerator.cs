namespace FakerLib.Generators
{
    class LongGenerator : RandomGenerator
    {
        public override object Generate()
        {
            return ((long)GetRandom().Next()) << 32 | ((long)GetRandom().Next());
        }
    }
}
