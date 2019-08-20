using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xrm.Framework.CI.Common
{
    public interface IReflectionLoader
    {
        void Initialise(string assemblyPath, string attributeClassName);
        List<Dictionary<string, object>> Constructors { get; set; }
        List<string> ClassNames { get; }
        string AssemblyName { get; }
    }

    public class ReflectionLoader : IReflectionLoader
    {
        #region Members and Constructors
        public List<Dictionary<string, object>> Constructors { get; set; }
        public List<string> ClassNames { get; private set; }
        public string AssemblyName { get; private set; }
        private System.Reflection.Assembly _assembly;
        private string CustomAttributeClassName { get; set; }

        public ReflectionLoader()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve +=
                new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
            Constructors = null;
            ClassNames = null;
            
        }
        #endregion

        public void Initialise(string assemblyPath, string attributeClassName)
        {
            CustomAttributeClassName = attributeClassName;
            AssemblyName = LoadAssemblyForReflection(assemblyPath);
            ClassNames = GetClassesUsingCustomAttribute(attributeClassName);
            InitialiseConstructors();
        }

        #region Private Methods
        private ConstructorInfo MatchAttributesToConstructor(ICollection<CustomAttributeTypedArgument> customAttributes,
            string className, string attributeClassName)
        {
            var contenders = GetConstructorInfos(attributeClassName, customAttributes.Count);
            var calledConstructor = GetCustomAttributeData(className, attributeClassName);
            var constructorArgs = calledConstructor.ConstructorArguments.Select(ca => ca.ArgumentType).ToList();

            foreach (var contender in contenders)
            {
                var paramTypes = contender.GetParameters().Select(p => p.ParameterType).ToList();
                if (constructorArgs.SequenceEqual(paramTypes))
                    return contender;
            }
            return null;
        }

        private string LoadAssemblyForReflection(string assemblyPath)
        {
            _assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
            return _assembly.ManifestModule.Name;
        }

        private CustomAttributeData GetCustomAttributeData(string className, string attributeClassName)
        {
            return CustomAttributeData.GetCustomAttributes(GetType(className)).First(
                c => c.AttributeType == GetType(attributeClassName));
        }

        private System.Type GetType(string name)
        {
            return _assembly.GetType(name);
        }

        private List<ConstructorInfo> GetConstructorInfos(string className)
        {
            var t = GetType(className);
            return t.GetConstructors().ToList();
        }

        private List<ConstructorInfo> GetConstructorInfos(string className, int parameterCount)
        {
            return (from x in GetConstructorInfos(className)
                where x.GetParameters().ToList().Count == parameterCount
                select x).ToList();
        }

        private void InitialiseConstructors()
        {
            Constructors = new List<Dictionary<string, object>>();
            foreach (var pluginClass in ClassNames)
            {
                var constructorArgs = GetCustomAttributeData(pluginClass, CustomAttributeClassName).ConstructorArguments;
                var constructorParameters = MatchAttributesToConstructor(
                    constructorArgs, pluginClass, CustomAttributeClassName).GetParameters();

                var argDictionary = new Dictionary<string, object>();
                for (var i = 0; i < constructorArgs.Count; i++)
                {
                    argDictionary.Add(constructorParameters[i].Name, constructorArgs[i].Value);
                }
                Constructors.Add(argDictionary);
            }
        }

        private List<string> GetClassesUsingCustomAttribute(string customAttributeClassName)
        {
            var allTypes = _assembly.GetTypes();
            return (from t in allTypes let ca = t.GetCustomAttributes().ToList()
                where ca.FirstOrDefault(x => x.GetType().FullName == customAttributeClassName) != null  
                      && t.FullName != customAttributeClassName
                    select t.FullName).ToList();
        }

        private System.Reflection.Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.ReflectionOnlyLoad(args.Name);
        }
        #endregion
    }
}
