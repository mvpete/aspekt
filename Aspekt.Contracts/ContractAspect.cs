using System.Reflection;

namespace Aspekt.Contracts
{
    /// <summary>
    /// Base class for all contract aspects.
    /// </summary>
    public abstract class ContractAspect : Aspekt.Aspect
    {
        protected readonly List<IContractEvaluator> Evaluators = new List<IContractEvaluator>();
        protected readonly List<string> ParameterNames = new List<string>();
        protected readonly Contract.Target Target;
        protected readonly string TargetName;

        protected ContractAspect(Contract.Target target, string targetName)
        {
            Target = target;
            TargetName = targetName;
        }

        /// <summary>
        /// Evaluates all contract conditions and throws appropriate exception if any fail.
        /// </summary>
        protected void EvaluateContracts(MethodArguments args, Func<string, string, ContractException> exceptionFactory)
        {
            var targetValue = GetTargetValue(args);

            for (var i = 0; i < Evaluators.Count; i++)
            {
                var evaluator = Evaluators[i];
                var parameterName = i < ParameterNames.Count ? ParameterNames[i] : TargetName;

                try
                {
                    if (!evaluator.Evaluate(targetValue))
                    {
                        var condition = $"{parameterName}: {evaluator}";
                        throw exceptionFactory(args.MethodName, condition);
                    }
                }
                catch (ContractException)
                {
                    throw; // Re-throw contract exceptions
                }
                catch (Exception)
                {
                    var condition = $"{parameterName}: {evaluator} (evaluation failed)";
                    throw exceptionFactory(args.MethodName, condition);
                }
            }
        }

        /// <summary>
        /// Gets the value of the target (parameter, field, property, or return value).
        /// </summary>
        protected virtual object? GetTargetValue(MethodArguments args)
        {
            return Target switch
            {
                Contract.Target.Property => GetPropertyValue(args),
                Contract.Target.Field => GetFieldValue(args),
                Contract.Target.Parameter => args.Arguments.GetArgumentValueByName(TargetName),
                _ => args.Arguments.GetArgumentValueByName(TargetName),// Default to parameter lookup
            };
        }

        private object? GetPropertyValue(MethodArguments args)
        {
            if (args.Instance == null)
                throw new InvalidOperationException("Cannot access property on null instance");

            var property = args.Instance.GetType().GetProperty(TargetName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
                throw new InvalidOperationException($"Property '{TargetName}' not found");

            return property.GetValue(args.Instance);
        }

        private object? GetFieldValue(MethodArguments args)
        {
            if (args.Instance == null)
                throw new InvalidOperationException("Cannot access field on null instance");

            var field = args.Instance.GetType().GetField(TargetName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
                throw new InvalidOperationException($"Field '{TargetName}' not found");

            return field.GetValue(args.Instance);
        }

        /// <summary>
        /// Adds a contract evaluator for the specified parameter.
        /// </summary>
        protected void AddContract(IContractEvaluator evaluator, string? parameterName = null)
        {
            Evaluators.Add(evaluator);
            ParameterNames.Add(parameterName ?? TargetName);
        }
    }
}
