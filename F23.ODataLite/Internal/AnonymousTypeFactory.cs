using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace F23.ODataLite.Internal
{
    // Inspired By: https://github.com/dotlattice/LatticeUtils/blob/master/LatticeUtils/AnonymousTypeUtils.cs
    internal static class AnonymousTypeFactory
    {
        private static readonly ModuleBuilder moduleBuilder;
        private static readonly object syncRoot = new object();

        static AnonymousTypeFactory()
        {
            var assemblyName = new AssemblyName { Name = "ODataLiteAnonymousTypes" };
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        public static Type CreateType(IEnumerable<PropertyInfo> properties)
        {
            var propertyNames = properties.Select(p => p.Name).ToList();
            var (name, hash) = GenerateGenericTypeDefinitionNameAndHash(propertyNames);

            Type type;
            lock (syncRoot)
            {
                type = moduleBuilder.GetType(name);

                if (type == null)
                {
                    type = CreateTypeNoLock(name, properties);
                }
            }
            return type;
        }

        private static Type CreateTypeNoLock(string name, IEnumerable<PropertyInfo> properties)
        {
            var dynamicAnonymousType = moduleBuilder.DefineType(name, TypeAttributes.Public);

            foreach (var prop in properties)
            {
                dynamicAnonymousType.DefineField(prop.Name, prop.PropertyType, FieldAttributes.Public);
            }

            return dynamicAnonymousType.CreateTypeInfo().AsType();
        }

        private static (string, string) GenerateGenericTypeDefinitionNameAndHash(ICollection<string> propertyNames)
        {
            var keyJsonBuilder = new StringBuilder();
            keyJsonBuilder.Append('{');
            keyJsonBuilder.Append("properties=[");
            keyJsonBuilder.Append(string.Join(",", propertyNames.Select(n => '"' + n.Replace("\"", "\"\"") + '"')));
            keyJsonBuilder.Append(']');
            keyJsonBuilder.Append('}');

            string keyHashHexString;
            using (var hasher = new SHA1Managed())
            {
                var hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(keyJsonBuilder.ToString()));
                keyHashHexString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }

            string genericTypeDefinitionName = string.Format("<>f__ODataLiteAnonymousType{0}`{1}",
                keyHashHexString,
                propertyNames.Count
            );
            return (genericTypeDefinitionName, keyHashHexString);
        }
    }
}
