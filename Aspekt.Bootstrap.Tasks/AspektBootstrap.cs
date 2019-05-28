using System;
using System.Linq;
using Microsoft.Build.Framework;

namespace Aspekt.Bootstrap.Tasks
{
    public class AspektBootstrap : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        public override bool Execute()
        {
            var success = true;

            foreach (var item in Assemblies)
            {
                var assemblyPath = item.GetMetadata("FullPath");

                try
                {
                    Log.LogMessage(MessageImportance.Low, "Aspekt bootstrapping \"{0}\"", assemblyPath);

                    Bootstrap.Apply(assemblyPath,
                        References
                            .Select(p => new ReferencedAssembly
                            {
                                FilePath = p.ItemSpec,
                                FullName = p.GetMetadata("FusionName")
                            }));

                    Log.LogMessage(MessageImportance.High, "Aspekt bootstrapped \"{0}\"", assemblyPath);
                }
                catch (Exception ex)
                {
                    #if DEBUG
                    var showStackTrace = true;
                    #else
                    var showStackTrace = false;
                    #endif

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    Log.LogErrorFromException(ex, showStackTrace, true, assemblyPath);
                    success = false;
                }
            }

            return success;
        }
    }
}
