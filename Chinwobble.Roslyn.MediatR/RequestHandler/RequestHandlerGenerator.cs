using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinwobble.Roslyn.MediatR.RequestHandler
{
    public class RequestHandlerGenerator : ICodeGenerator
    {
        public RequestHandlerGenerator(AttributeData attributeData)
        {
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var classDeclaration = GenerateRequestHandler(context);
            return Task.FromResult(SyntaxFactory.List(classDeclaration));
        }

        IEnumerable<MemberDeclarationSyntax> GenerateRequestHandler(TransformationContext context)
        {
            // System.Diagnostics.Debugger.Launch();

            if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration && IsMediatorRequest(classDeclaration))
            {
                var handlerClassName = classDeclaration.Identifier.ValueText + "Handler_Generated";
                var typeInfo = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                var walker = new RequestHandlerCSharpWalker();

                classDeclaration.Accept(walker);

                var fields = walker.HandleMethodParameters.Select(ToFieldSyntax).ToArray();
                var constructor = GenerateConstructor(handlerClassName, walker.HandleMethodParameters);
                var handleMethod = GenerateHandleMethod(classDeclaration.Identifier.ValueText, walker.HandleMethodParameters);

                var requestHandlerType = SyntaxFactory
                    .ClassDeclaration(handlerClassName)
                    .AddMembers(fields)
                    .AddMembers(constructor, handleMethod);

                yield return requestHandlerType;
            }

            MemberDeclarationSyntax ToFieldSyntax((string ParamName, string TypeName) field)
            {
                return SyntaxFactory.ParseMemberDeclaration($@"private readonly {field.TypeName} _{field.ParamName};
");
            }

            MemberDeclarationSyntax GenerateHandleMethod(string className, IEnumerable<(string ParamName, string TypeName)> parameters)
            {
                var parametersToForward = string.Join(", ", parameters.Select(x => $"_{x.ParamName}"));
                return SyntaxFactory.ParseMemberDeclaration($@"public System.Threading.Tasks.Task Handle({className} message, System.Threading.CancellationToken cancellationToken)
{{
    return message.Handle({parametersToForward});
}}
");
            }

            MemberDeclarationSyntax GenerateConstructor(string className, IEnumerable<(string ParamName, string TypeName)> parameters)
            {
                var parameterList = string.Join(", ", parameters.Select(x => $"{x.TypeName} {x.ParamName}"));
                return SyntaxFactory.ParseMemberDeclaration($@"public {className}({parameterList})
{{
    {string.Join("\n", parameters.Select(x => $"_{x.ParamName} = {x.ParamName};"))}
}}");
            }

            bool IsMediatorRequest(ClassDeclarationSyntax classDeclarationSyntax)
            {
                var MediatRRequestInterfaceType = context.Compilation.GetTypeByMetadataName("MediatR.IBaseRequest");
                var targetType = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                return MediatRRequestInterfaceType.IsAssignableFrom(targetType, false);
            }

            yield break;
        }
    }
}
