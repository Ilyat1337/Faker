using FakerLib.ContainerGenerators;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace FakerLib.Generators
{
    class GenericListGenerator : RandomContainerGenerator
    {
        private static readonly int MIN_ELEMENT_COUNT = 0;
        private static readonly int MAX_ELEMENT_COUNT = 10;

        private readonly Faker faker;
        private readonly Type listType;

        public GenericListGenerator(Faker faker)
        {
            this.faker = faker;
            listType = typeof(List<>);
        }

        public override object Generate(Type elementType)
        {
            Type genericListType = listType.MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(genericListType);

            int elementCount = GetRandom().Next(MIN_ELEMENT_COUNT, MAX_ELEMENT_COUNT + 1);
            for (int i = 0; i < elementCount; i++)
                list.Add(faker.Create(elementType));
            return list;
        }
    }
}
