using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chinwobble.Roslyn.PropertyChanged
{
    internal class DependencyPropertyGenerator : ICodeGenerator
    {
        public DependencyPropertyGenerator(AttributeData attributeData)
        {

        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var partialType = CreatePartialType();
            return Task.FromResult(SyntaxFactory.List(partialType));

            IEnumerable<MemberDeclarationSyntax> CreatePartialType()
            {
                if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration)
                {
                    var newPartialType = SyntaxFactory.ClassDeclaration(classDeclaration.Identifier.ValueText);

                    var fieldWalker = new DependencyPropertyCSharpWalker(context.SemanticModel);
                    classDeclaration.Accept(fieldWalker);
                    if (!fieldWalker.PropertiesToGenerate.Any())
                    {
                        yield break;
                    }
                    var newProperties = fieldWalker
                        .PropertiesToGenerate
                        .Select(CreateIdProperty);
                    yield return newPartialType
                        ?.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                        .AddMembers(newProperties.ToArray());
                }
            }


            MemberDeclarationSyntax CreateIdProperty((string Type, string Name) property)
            {
                return SyntaxFactory.ParseMemberDeclaration($@"
public {property.Type} {property.Name}
{{
    get 
    {{
        return ({property.Type})GetValue({property.Name}Property);
    }}
    set
    {{
        SetValue({property.Name}Property, value);
    }}
}}
");
            }
        }
    }
}
