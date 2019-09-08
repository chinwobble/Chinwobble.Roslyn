using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinwobble.Roslyn.PropertyChanged.NotifyPropertyChanged
{
    internal class FieldSyntaxWalker : CSharpSyntaxWalker
    {
        public List<(string Type, string FieldName)> Fields { get; } = new List<(string Type, string FieldName)>();
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            Fields.AddRange(node.Declaration
                    .Variables
                    .Select(x => (node.Declaration.Type.ToFullString(), x.Identifier.Text)));
            base.VisitFieldDeclaration(node);
        }
    }
}
