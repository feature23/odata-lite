using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace F23.ODataLite.Tests
{
    internal static class TypeExtensions
    {
        // https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        public static bool IsAnonymousType(this Type type, bool requireCompilerGenerated = true)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var isCompilerGenerated = !requireCompilerGenerated || 
                Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
            var nameContainsAnonymousType = type.Name.Contains("AnonymousType");
            var startsWithAnonTypePrefix = type.Name.StartsWith("<>") || type.Name.StartsWith("VB$");
            var hasNonPublicTypeAttribute = type.Attributes.HasFlag(TypeAttributes.NotPublic);

            return isCompilerGenerated
                && nameContainsAnonymousType
                && startsWithAnonTypePrefix
                && hasNonPublicTypeAttribute;
        }
    }
}
