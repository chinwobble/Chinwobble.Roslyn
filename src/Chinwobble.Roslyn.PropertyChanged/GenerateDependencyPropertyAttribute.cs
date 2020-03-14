using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace Chinwobble.Roslyn.PropertyChanged
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(DependencyPropertyGenerator))]
    [Conditional("CodeGeneration")]
    public sealed class GenerateDependencyPropertyAttribute : Attribute
    {
    }
}
