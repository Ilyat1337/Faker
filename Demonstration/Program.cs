using Demonstration.Serialization;
using FakerLib;
using GeneratorInterface;
using System;
using System.Collections.Generic;

namespace Demonstration
{
    class Program
    {
        static void Main(string[] args)
        {
            FakerConfig fakerConfig = new FakerConfig();
            fakerConfig.Add<Foo, string>(foo => foo.City, new CityGenerator());
            Faker faker = new Faker(fakerConfig);

            Foo a = faker.Create<Foo>();

            ISerializer jsonSerializer = new JSONSerializer();
            Console.WriteLine(jsonSerializer.Serizlize(a));
        }
    }

    class CityGenerator : IGenerator
    {
        private static readonly List<string> cityList = new List<string>() { "Minsk", "Brest", "Grodno", "Gomel", "Vitebsk" };

        private readonly Random random = new Random();

        public object Generate()
        {
            return cityList[random.Next(cityList.Count)];
        }
    }

    class Foo
    {
        public string Str
        {get; set;}
        public string City
        { get; }
        public double doubleValue;
        private int intValue;
        public List<Bar> bars;

        public Foo() { }

        public Foo(string City)
        {
            this.City = City;
        }

        public void SetC(int intValue)
        {
            this.intValue = intValue;
        }

    }

    class Bar
    {
        public DateTime date;
        public byte byteValue
        { get; set; }

        public Foo foo;
    }
}
