using Chinwobble.Roslyn.PropertyChanged.NotifyPropertyChanged;
using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace Chinwobble.Roslyn.PropertyChanged
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(PropertyChangedGenerator))]
    [Conditional("CodeGeneration")]
    public sealed class GenerateNotifyPropertyChangedAttribute : Attribute
    {
    }
}
