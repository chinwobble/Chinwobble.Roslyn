using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Chinwobble.Roslyn.MediatR.RequestHandler
{
    public class RequestHandlerCSharpWalker : CSharpSyntaxWalker
    {
        public List<(string ParamName, string TypeName)> HandleMethodParameters { get; } = new List<(string ParamName, string TypeName)>();

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Identifier.ValueText == "Handle")
            {
                foreach (var param in node.ParameterList.Parameters)
                {
                    HandleMethodParameters.Add((param.Identifier.ValueText, param.Type.ToString()));
                }
            }
        }
    }
}
