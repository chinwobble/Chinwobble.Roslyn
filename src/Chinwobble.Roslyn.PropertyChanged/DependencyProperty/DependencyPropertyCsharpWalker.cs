using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Chinwobble.Roslyn.PropertyChanged
{
    internal class DependencyPropertyCSharpWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        public List<(string PropertyType, string PropertyName)> PropertiesToGenerate = new List<(string PropertyName, string PropertyType)>();

        public DependencyPropertyCSharpWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var symbolInfo = _semanticModel.GetTypeInfo(node.Declaration.Type);
            if (
                !node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword))
                || (symbolInfo.ConvertedType.ToDisplayString() != "System.Windows.DependencyProperty"
                && symbolInfo.ConvertedType.ToDisplayString() != "Xamarin.Forms.BindableProperty"))
            {
                return;
            }

            node.Declaration
                .Variables
                .Where(x => x.Initializer != null)
                .Select(x => x.Initializer)
                .Select(x => x.Value)
                .OfType<InvocationExpressionSyntax>()
                .ToList()
                .ForEach(x =>
                {
                    var initValueSymbolInfo = _semanticModel.GetSymbolInfo(x);
                    if (initValueSymbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        if (methodSymbol.ToDisplayString().Contains("System.Windows.DependencyProperty.Register"))
                        {
                            var prop = ToPropertyTypeAndName(x, _semanticModel, "name", "propertyType");
                            PropertiesToGenerate.Add(prop);
                        }

                        if (methodSymbol.ToDisplayString().Contains("Xamarin.Forms.BindableProperty.Create"))
                        {
                            var prop = ToPropertyTypeAndName(x, _semanticModel, "propertyName", "returnType");
                            PropertiesToGenerate.Add(prop);
                        }
                    }
                    
                });
        }

        private static (string PropertyType, string PropertyName) ToPropertyTypeAndName(
            InvocationExpressionSyntax invocationExpressionSyntax, 
            SemanticModel semanticModel,
            string toGeneratePropertyArgumentName,
            string toGeneratePropertyArgumentType)
        {
            var list = invocationExpressionSyntax.ArgumentList
                    .Arguments
                    .Select(paramSyntax => (ArgumentValue: paramSyntax.Expression, ArgumentName: DetermineParameter(paramSyntax, semanticModel).Name))
                    .ToArray();
            var propertyNameExpression = list.Single(arg => arg.ArgumentName == toGeneratePropertyArgumentName).ArgumentValue;
            var propertyType = list.Single(arg => arg.ArgumentName == toGeneratePropertyArgumentType).ArgumentValue;
            var propertyTypeString = ((TypeOfExpressionSyntax)propertyType).Type.ToString();
            var resolvedPropertyName = semanticModel.GetConstantValue(propertyNameExpression).Value.ToString();
            return (propertyTypeString, resolvedPropertyName);
        }

        private static IParameterSymbol DetermineParameter(
            ArgumentSyntax argument,
            SemanticModel semanticModel,
            bool allowParams = false)
        {
            if (!(argument.Parent is BaseArgumentListSyntax argumentList))
            {
                return null;
            }

            if (!(argumentList.Parent is ExpressionSyntax invocableExpression))
            {
                return null;
            }

            var symbol = semanticModel.GetSymbolInfo(invocableExpression).Symbol;
            if (symbol == null)
            {
                return null;
            }

            var parameters = GetParameters(symbol);

            // Handle named argument
            if (argument.NameColon != null && !argument.NameColon.IsMissing)
            {
                var name = argument.NameColon.Name.Identifier.ValueText;
                return parameters.FirstOrDefault(p => p.Name == name);
            }

            // Handle positional argument
            var index = argumentList.Arguments.IndexOf(argument);
            if (index < 0)
            {
                return null;
            }

            if (index < parameters.Length)
            {
                return parameters[index];
            }

            if (allowParams)
            {
                var lastParameter = parameters.LastOrDefault();
                if (lastParameter == null)
                {
                    return null;
                }

                if (lastParameter.IsParams)
                {
                    return lastParameter;
                }
            }

            return null;
        }

        private static ImmutableArray<IParameterSymbol> GetParameters(ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol m: return m.Parameters;
                case IPropertySymbol nt: return nt.Parameters;
                default: return ImmutableArray<IParameterSymbol>.Empty;
            }
        }
    }
}
