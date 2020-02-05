using Chinwobble.Roslyn.MediatR.RequestHandler;
using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace Chinwobble.Roslyn.MediatR
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(RequestHandlerGenerator))]
    [Conditional("CodeGeneration")]
    public class GenerateRequestHandlerAttribute : Attribute
    {
    }
}
