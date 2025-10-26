using Mono.Cecil;

namespace Aspekt.Bootstrap
{
    public class PreregisteredAssemblyResolver : DefaultAssemblyResolver
    {
        private readonly Dictionary<string, string> preregistered_ = new Dictionary<string, string>();

        protected override AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
        {
            if (preregistered_.TryGetValue(name.Name, out var filePath))
            {
                if (parameters.AssemblyResolver == null)
                {
                    parameters.AssemblyResolver = this;
                }

                var module = ModuleDefinition.ReadModule(filePath, parameters);

                return module.Assembly;
            }

            return base.SearchDirectory(name, directories, parameters);
        }

        public void PreregisterAssembly(ReferencedAssembly referencedAssembly)
        {
            if (string.IsNullOrEmpty(referencedAssembly.FullName))
            {
                return;
            }

            var simpleName = GetSimpleName(referencedAssembly.FullName);
            if (!string.IsNullOrEmpty(simpleName) &&
                !preregistered_.ContainsKey(simpleName))
            {
                preregistered_.Add(simpleName, referencedAssembly.FilePath);
            }
        }

        private static string GetSimpleName(string fullName)
        {
            return fullName.Split(',')[0].Trim();
        }
    }
}
