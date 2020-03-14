using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Chinwobble.Roslyn.MediatR.RequestHandler
{
    public class RequestHandlerCSharpWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;

        public int? HandleMethodCancellationTokenParameterIndex { get; private set; }
        public List<(string ParamName, string TypeName)> HandleMethodParameters { get; } = new List<(string ParamName, string TypeName)>();

        public RequestHandlerCSharpWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Identifier.ValueText == "Handle")
            {

                for (int i = 0; i < node.ParameterList.Parameters.Count; i++)
                {
                    var param = node.ParameterList.Parameters[i];
                    var symbol = _semanticModel.GetTypeInfo(param.Type);
                    if (symbol.ConvertedType?.ToDisplayString() == "System.Threading.CancellationToken")
                    {
                        if (HandleMethodCancellationTokenParameterIndex.HasValue)
                        {
                            throw new System.Exception("Cannot have mutliple cancellation tokens");
                        }

                        HandleMethodCancellationTokenParameterIndex = i;
                        continue;
                    }

                    HandleMethodParameters.Add((param.Identifier.ValueText, param.Type.ToString()));
                }
            }
        }
    }
}
