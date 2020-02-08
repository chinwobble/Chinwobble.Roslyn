using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            return Task.FromResult(List(classDeclaration));
        }

        IEnumerable<MemberDeclarationSyntax> GenerateRequestHandler(TransformationContext context)
        {
            if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration && IsMediatorRequest(classDeclaration, out var details))
            {
                var handlerClassName = classDeclaration.Identifier.ValueText + "Handler_Generated";
                var typeInfo = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                var walker = new RequestHandlerCSharpWalker(context.SemanticModel);

                classDeclaration.Accept(walker);

                var fields = walker.HandleMethodParameters.Select(ToFieldSyntax).ToArray();
                var constructor = GenerateConstructor(handlerClassName, walker.HandleMethodParameters);
                var handleMethod = GenerateHandleMethod(classDeclaration.Identifier.ValueText, walker.HandleMethodCancellationTokenParameterIndex, walker.HandleMethodParameters.ToArray(), details);
                var newClassDefinition = ClassDeclaration(handlerClassName);

                var requestHandlerType = ClassDeclaration(handlerClassName)
                    .AddMembers(fields)
                    .AddMembers(constructor, handleMethod);

                var tResponse = details.IsGeneric
                    ? $", {details.ReturnTypeName}"
                    : "";
                requestHandlerType = requestHandlerType
                    .AddBaseListTypes(SimpleBaseType(ParseName($"global::MediatR.IRequestHandler<{classDeclaration.Identifier.ValueText}{tResponse}>")));
                yield return requestHandlerType;
            }

            MemberDeclarationSyntax ToFieldSyntax((string ParamName, string TypeName) field)
            {
                return ParseMemberDeclaration($@"private readonly {field.TypeName} _{field.ParamName};
");
            }

            MemberDeclarationSyntax GenerateHandleMethod(
                string className,
                int? cancellationTokenIndex,
                (string ParamName, string TypeName)[] parameters,
                in MediatorDetails returnTypeDetails)
            {
                var parametersToForward = string.Empty;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i == cancellationTokenIndex)
                    {
                        parametersToForward += i != parameters.Length
                            ? $"cancellationToken, "
                            : $"cancellationToken";
                    }

                    parametersToForward += i != parameters.Length - 1
                        ? $"_{parameters[i].ParamName}, "
                        : $"_{parameters[i].ParamName}";
                }

                if (parameters.Length == cancellationTokenIndex)
                {
                    parametersToForward += $"cancellationToken";
                }

                var returnType = returnTypeDetails.IsGeneric
                    ? $"<{returnTypeDetails.ReturnTypeName}>"
                    : "<global::MediatR.Unit>";

                if (returnTypeDetails.IsGeneric)
                {
                    return ParseMemberDeclaration($@"public System.Threading.Tasks.Task{returnType} Handle({className} message, System.Threading.CancellationToken cancellationToken)
{{
    return message.Handle({parametersToForward});
}}
");
                }
                else
                {
                    return ParseMemberDeclaration($@"public async System.Threading.Tasks.Task{returnType} Handle({className} message, System.Threading.CancellationToken cancellationToken)
{{
    await message.Handle({parametersToForward});
    return global::MediatR.Unit.Value;
}}
");
                }

            }

            MemberDeclarationSyntax GenerateConstructor(string className, IEnumerable<(string ParamName, string TypeName)> parameters)
            {
                var parameterList = string.Join(", ", parameters.Select(x => $"{x.TypeName} {x.ParamName}"));
                return ParseMemberDeclaration($@"public {className}({parameterList})
{{
    {string.Join("\n", parameters.Select(x => $"_{x.ParamName} = {x.ParamName};"))}
}}");
            }

            bool IsMediatorRequest(ClassDeclarationSyntax classDeclarationSyntax, out MediatorDetails mediatorDetails)
            {
                var mediatRGenericRequestType = context.Compilation.GetTypeByMetadataName("MediatR.IRequest`1");
                var mediatRRequestType = context.Compilation.GetTypeByMetadataName("MediatR.IRequest");
                var mediatorBaseRequestType = context.Compilation.GetTypeByMetadataName("MediatR.IBaseRequest");
                var targetType = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                var isMediatorRequest = mediatorBaseRequestType.IsAssignableFrom(targetType, false);
                if (isMediatorRequest)
                {
                    var baseType = targetType;
                    while (baseType != null)
                    {
                        foreach (var inter in baseType.AllInterfaces)
                        {
                            var isRequestWithResponse = inter.OriginalDefinition.Equals(mediatRGenericRequestType);

                            if (isRequestWithResponse)
                            {
                                var responseTypeName = inter.TypeArguments[0].ToDisplayString();
                                mediatorDetails = new MediatorDetails(true, responseTypeName);
                                return true;
                            }

                            var isRequestWithoutResponse = inter.OriginalDefinition.Equals(mediatRGenericRequestType);

                            var isRequest = !inter.IsGenericType && inter.Equals(mediatRRequestType);
                            if (isRequest)
                            {
                                mediatorDetails = new MediatorDetails(false, null);
                                return true;
                            }
                        }

                        baseType = baseType.BaseType;
                    }
                }

                mediatorDetails = default;
                return false;
            }

            yield break;
        }
    }
}
