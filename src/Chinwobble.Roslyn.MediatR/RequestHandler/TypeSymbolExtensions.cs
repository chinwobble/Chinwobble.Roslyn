using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Chinwobble.Roslyn.MediatR.RequestHandler
{
    public static class TypeSymbolExtensions
    {
        //public static bool IsMediatorRequest(
        //    this ClassDeclarationSyntax classDeclarationSyntax,
        //    SemanticModel semanticModel)
        //{
        //    var typeInfo = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);


        //}

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType != null)
            {
                while (sourceType != null)
                {
                    if (sourceType.Equals(targetType))
                        return true;

                    if (exactMatch)
                        return false;

                    if (targetType.TypeKind == TypeKind.Interface)
                        return sourceType.AllInterfaces
                            .Select(i => i.IsGenericType ? i.OriginalDefinition : i)
                            .Any(i => Equals(i, targetType));

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
        }
    }
}
