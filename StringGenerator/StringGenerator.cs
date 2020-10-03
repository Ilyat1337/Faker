using GeneratorInterface;
using System;
using System.Linq;

namespace StringGenerator
{
    public class StringGenerator : IGenerator
    {
        private static readonly string STRING_CHARSET = "abcdefghijklmnopqrstuvwxyzABSDEFGHIJKLMNOPQRSTUVWXYZ0123456789+?=";
        private static readonly int MIN_STRING_LENGTH = 1;
        private static readonly int MAX_STRING_LENGTH = 25;

        private readonly Random random;

        public StringGenerator()
        {
            random = new Random();
        }

        public object Generate()
        {
            int stringLength = random.Next(MIN_STRING_LENGTH, MAX_STRING_LENGTH + 1);
            return new string(Enumerable.Range(1, stringLength).Select(
                _ => STRING_CHARSET[random.Next(STRING_CHARSET.Length)]).ToArray()
            );
        }
    }
}
