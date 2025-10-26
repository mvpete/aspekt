using Mono.Cecil;

namespace Aspekt.Bootstrap
{
    static class TypeDefinitonExtensions
    {
        public static bool IsSubclassOf(this TypeDefinition childType, Type parentType)
        {
            return childType.FullName
               != parentType.FullName
               && childType
              .EnumerateBaseClassTypeDefinitions()
              .Any(b => b.FullName == parentType.FullName);
        }

        public static IEnumerable<TypeDefinition> EnumerateBaseClassTypeDefinitions(this TypeDefinition myType)
        {
            for (var typeDefinition = myType; typeDefinition != null; typeDefinition = typeDefinition.BaseType?.Resolve())
            {
                yield return typeDefinition;
            }
        }

    }
}
