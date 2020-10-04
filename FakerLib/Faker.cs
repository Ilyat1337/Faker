using FakerLib.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneratorInterface;

namespace FakerLib
{
    public class Faker
    {
        private static readonly string PLUGINS_FOLDER_NAME = "Plugins";

        private static readonly List<IGenerator> loadedGenerators;

        private readonly Dictionary<Type, IGenerator> defaultGenerators;
        private readonly Dictionary<Type, IContainerGenerator> containerGenerators;
        private readonly Dictionary<Type, Dictionary<(string, Type), IGenerator>> userGenerators;
        private readonly HashSet<Type> typesInCreationProcess;

        static Faker()
        {
            loadedGenerators = PluginLoader.LoadPlugins<IGenerator>(PLUGINS_FOLDER_NAME);
        }

        public Faker(FakerConfig fakerConfig) : this()
        {
            foreach (var generatorInfo in fakerConfig.GetUserGenerators())
            {
                if (!userGenerators.ContainsKey(generatorInfo.Item1))
                    userGenerators.Add(generatorInfo.Item1, new Dictionary<(string, Type), IGenerator>());
                userGenerators[generatorInfo.Item1].Add((generatorInfo.Item2, generatorInfo.Item3), generatorInfo.Item4);
            }
        }

        public Faker()
        {
            defaultGenerators = new Dictionary<Type, IGenerator>();
            defaultGenerators.Add(typeof(int), new IntGenerator());

            if (loadedGenerators != null)
                foreach (IGenerator generator in loadedGenerators)
                    defaultGenerators.Add(generator.Generate().GetType(), generator);

            containerGenerators = new Dictionary<Type, IContainerGenerator>();
            containerGenerators.Add(typeof(List<>), new GenericListGenerator(this));

            typesInCreationProcess = new HashSet<Type>();
            userGenerators = new Dictionary<Type, Dictionary<(string, Type), IGenerator>>();
        }

        public T Create<T>()
        { 
            return (T)Create(typeof(T));
        }

        internal object Create(Type obobjectType)
        {
            return Create(obobjectType, null, null);
        }

        internal object Create(Type objectType, Type classType, string memberName)
        {
            if (typesInCreationProcess.Contains(objectType))
                return CreateDefaultValue(objectType);
            typesInCreationProcess.Add(objectType);

            object createdObject;
            if (HasUserGeneratorFor(classType, memberName, objectType))
                createdObject = GenerateUsingUserGenerator(classType, memberName, objectType);
            else if (HasDefaultGeneratorFor(objectType))
                createdObject = GenerateUsingDefault(objectType);
            else if (IsGenericCollection(objectType))
                createdObject = CreateGenericCollection(objectType);
            else if (IsDTO(objectType))
                createdObject = CreateDTO(objectType);
            else
                createdObject = CreateDefaultValue(objectType);

            typesInCreationProcess.Remove(objectType);
            return createdObject;
        }

        private bool HasUserGeneratorFor(Type classType, string memberName, Type returnType)
        {
            return classType != null && memberName != null &&
                userGenerators.ContainsKey(classType) && userGenerators[classType].ContainsKey((memberName, returnType));
        }

        private object GenerateUsingUserGenerator(Type classType, string memberName, Type returnType)
        {
            return userGenerators[classType]?[(memberName, returnType)]?.Generate();
        }

        private bool HasDefaultGeneratorFor(Type objectType)
        {
            return defaultGenerators.ContainsKey(objectType);
        }

        private object GenerateUsingDefault(Type objectType)
        {
            return defaultGenerators[objectType]?.Generate();
        }

        private bool IsGenericCollection(Type objectType)
        {
            return objectType.GetInterface(typeof(ICollection<>).Name) != null;
        }

        private object CreateGenericCollection(Type objectType)
        {
            Type collectionType = objectType.GetGenericTypeDefinition();
            if (!containerGenerators.ContainsKey(collectionType))
                return CreateDefaultValue(objectType);

            Type elementType = objectType.GetGenericArguments()[0];
            return containerGenerators[collectionType].Generate(elementType);
        }

        private bool IsDTO(Type objectType)
        {
            if (objectType.IsPrimitive)
                return false;
            return true;
        }

        private object CreateDTO(Type objectType)
        {
            object dtoObject = CreateUninitializedObject(objectType);

            PropertyInfo[] properties = objectType.GetProperties().Where(property => property.GetSetMethod() != null).ToArray();
            SetOjectPoperties(dtoObject, properties);

            FieldInfo[] fields = objectType.GetFields();
            SetObjectFields(dtoObject, fields);

            MethodInfo[] setters = GetClassSetters(objectType);
            InvokeObjectSetters(dtoObject, setters);

            return dtoObject;
        }

        private object CreateUninitializedObject(Type objectType)
        {
            ConstructorInfo[] constructorInfos = GetConstructorsFor(objectType);
            ConstructorInfo bestConstructor = ChooseBestConstructor(constructorInfos);
            return InvokeConstructor(bestConstructor);
        }

        private ConstructorInfo[] GetConstructorsFor(Type objectType)
        {
            ConstructorInfo[] constructors = objectType.GetConstructors();
            if (constructors.Length == 0)
                constructors = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            return constructors;
        }

        private ConstructorInfo ChooseBestConstructor(ConstructorInfo[] constructorInfos)
        {
            ConstructorInfo[] suitableConstructors = constructorInfos;
            if (userGenerators.ContainsKey(constructorInfos[0].DeclaringType))
            {
                suitableConstructors = SelectConstructorsByFields(constructorInfos, 
                                    userGenerators[constructorInfos[0].DeclaringType]);
                if (suitableConstructors.Length == 0)
                    suitableConstructors = constructorInfos;
            }
            return suitableConstructors.ToList().Aggregate((firstCI, secondCI) =>
                firstCI.GetParameters().Length > secondCI.GetParameters().Length ? firstCI : secondCI
            );
        }

        private ConstructorInfo[] SelectConstructorsByFields(ConstructorInfo[] constructorInfos, 
                                Dictionary<(string, Type), IGenerator> generatorsForClass)
        {
            return constructorInfos.Where(constructorInfo =>
            {
                foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
                    if (generatorsForClass.ContainsKey((parameterInfo.Name, parameterInfo.ParameterType)))
                        return true;
                return false;
            }).ToArray();
        }

        private object InvokeConstructor(ConstructorInfo constructor)
        {
            List<object> parameters = new List<object>();
            foreach (ParameterInfo parameterInfo in constructor.GetParameters())
            {
                parameters.Add(Create(parameterInfo.ParameterType, constructor.DeclaringType, parameterInfo.Name));
            }
            return constructor.Invoke(parameters.ToArray());
        }

        private void SetOjectPoperties(object o, PropertyInfo[] properties)
        {
            foreach (PropertyInfo property in properties)
                property.SetValue(o, Create(property.PropertyType, o.GetType(), property.Name));
        }

        private void SetObjectFields(object o, FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
                field.SetValue(o, Create(field.FieldType, o.GetType(), field.Name));
        }

        private MethodInfo[] GetClassSetters(Type objectType)
        {
            List<MethodInfo> setters = new List<MethodInfo>();
            foreach (MethodInfo method in objectType.GetMethods().Where(methodInfo => !methodInfo.IsSpecialName))
            {
                if (MethodCheckHelper.IsSetter(method))
                    setters.Add(method);
            }
            return setters.ToArray();
        }

        private void InvokeObjectSetters(object o, MethodInfo[] setters)
        {
            foreach (MethodInfo setter in setters)
                setter.Invoke(o, new object[] { Create(setter.GetParameters()[0].ParameterType) });
        }

        private object CreateDefaultValue(Type objectType)
        {
            if (objectType.IsValueType)
                return Activator.CreateInstance(objectType);

            return null;
        }
    }
}
