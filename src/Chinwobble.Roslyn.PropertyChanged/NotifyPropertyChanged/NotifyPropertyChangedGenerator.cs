using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace Chinwobble.Roslyn.PropertyChanged.NotifyPropertyChanged
{
    public class PropertyChangedGenerator : ICodeGenerator
    {
        public PropertyChangedGenerator(AttributeData attributeData)
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

                    var fieldWalker = new FieldSyntaxWalker();
                    classDeclaration.Accept(fieldWalker);
                    if (fieldWalker.Fields.Count == 0)
                    {
                        yield break;
                    }
                    var newProperties = fieldWalker.Fields.Select(CreateIdProperty);
                    yield return newPartialType
                        ?.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                        .AddMembers(newProperties.ToArray());
                }
            }

            
            MemberDeclarationSyntax CreateIdProperty((string Type, string FieldName) field)
            {
                var propertyName = field.FieldName.Replace("_", string.Empty).Pascalize();
                Console.WriteLine(propertyName);
                return SyntaxFactory.ParseMemberDeclaration($@"
public {field.Type} {propertyName} 
{{ 
    get => {field.FieldName};
    set
    {{
        if ({field.FieldName} != value)
        {{
            {field.FieldName} = value;
            OnPropertyChanged(nameof({propertyName}));
        }}
    }}
}}
");
            }
        }
    }
}
