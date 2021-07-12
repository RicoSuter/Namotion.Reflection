using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A method info with contextual information.
    /// </summary>
    public class ContextualMethodInfo
    {
        /// <summary>
        /// Gets the type context's method info.
        /// </summary>
        public MethodInfo MethodInfo { get; internal set; }

        /// <summary>
        /// Gets the name of the cached method name.
        /// </summary>
        public string Name => MethodInfo.Name;

        /// <summary>
        /// Gets the contextual parameters.
        /// </summary>
        public ContextualParameterInfo[] Parameters { get; internal set; }

        /// <summary>
        /// Gets the contextual return parameter.
        /// </summary>
        public ContextualParameterInfo ReturnParameter { get; internal set; }

        /// <inheritdocs />
        public override string ToString()
        {
            return Name + " (" + GetType().Name.Replace("Contextual", "").Replace("Info", "") + ") - " + base.ToString();
        }
    }
}
