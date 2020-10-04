using GeneratorInterface;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakerLib.Tests
{
    public class FakerLibUnitTests
    {
        [Fact]
        public void ShouldCreateOnlyDTO()
        {
            Faker faker = new Faker();

            Bar bar = faker.Create<Bar>();
            NotDtoClass notDto = faker.Create<NotDtoClass>();

            Assert.NotNull(bar);
            Assert.Null(notDto);
        }

        [Fact]
        public void ShouldCreateAllReferenceTypes()
        {
            Faker faker = new Faker();

            Bar bar = faker.Create<Bar>();

            Assert.NotNull(bar.date);
            Assert.NotNull(bar.Str);
        }

        [Fact]
        public void ShoulUseUserGeneratorForFieldWithoutSetter()
        {
            FakerConfig fakerConfig = new FakerConfig();
            fakerConfig.Add<Foo, string>(foo => foo.City, new CityGenerator());
            Faker faker = new Faker(fakerConfig);

            Foo foo = faker.Create<Foo>();

            Assert.NotNull(foo.City);
            Assert.Contains(foo.City, CityGenerator.cityList);
        }

        [Fact]
        public void ShouldCreateListOfDTO()
        {
            Faker faker = new Faker();

            Foo foo = faker.Create<Foo>();

            Assert.NotNull(foo.bars);
            Assert.NotEmpty(foo.bars);
        }

        [Fact]
        public void ShouldDetectWrongGeneratorType()
        {
            FakerConfig fakerConfig = new FakerConfig();

            Assert.Throws< FakerConfigException>(() => fakerConfig.Add<Foo, double>(foo => foo.doubleValue, new CityGenerator()));
        }
    }

    class CityGenerator : IGenerator
    {
        public static readonly List<string> cityList = new List<string>() { "Minsk", "Brest", "Grodno", "Gomel", "Vitebsk" };

        private readonly Random random = new Random();

        public object Generate()
        {
            return cityList[random.Next(cityList.Count)];
        }
    }

    class Foo
    {
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
        public string Str
        { get; set; }
        public byte byteValue
        { get; set; }

        public Foo foo;
    }

    class NotDtoClass
    {
        public int number;

        public void SayHello()
        {
            Console.WriteLine("Hello, world!");
        }
    }
}
