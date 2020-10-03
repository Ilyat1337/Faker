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
        private readonly HashSet<Type> typesInCreationProcess;

        static Faker()
        {
            loadedGenerators = PluginLoader.LoadPlugins<IGenerator>(PLUGINS_FOLDER_NAME);
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
        }

        public T Create<T>()
        { 
            return (T)Create(typeof(T));
        }

        internal object Create(Type objectType)
        {
            if (typesInCreationProcess.Contains(objectType))
                return CreateDefaultValue(objectType);
            typesInCreationProcess.Add(objectType);

            object createdObject;
            if (HasDefaultGeneratorFor(objectType))
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

            PropertyInfo[] properties = objectType.GetProperties();
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
            return constructorInfos.ToList().Aggregate((firstCI, secondCI) =>
                firstCI.GetParameters().Length > secondCI.GetParameters().Length ? firstCI : secondCI
            );
        }

        private object InvokeConstructor(ConstructorInfo constructor)
        {
            List<object> parameters = new List<object>();
            foreach (ParameterInfo parameterInfo in constructor.GetParameters())
            {
                parameters.Add(Create(parameterInfo.ParameterType));
            }
            return constructor.Invoke(parameters.ToArray());
        }

        private void SetOjectPoperties(object o, PropertyInfo[] properties)
        {
            foreach (PropertyInfo property in properties)
                property.SetValue(o, Create(property.PropertyType));
        }

        private void SetObjectFields(object o, FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
                field.SetValue(o, Create(field.FieldType));
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
