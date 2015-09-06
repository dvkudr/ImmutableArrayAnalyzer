namespace ImmutableArrayAnalyzer
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using System.Threading.Tasks;

    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class BuildCodeFixProvider : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                        .GetSyntaxRootAsync(context.CancellationToken);

            var objectCreation = root.FindNode(context.Span)
                         .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

            context.RegisterCodeFix(
            CodeAction.Create("Use ImmutableArray<T>.Empty",
                              c => ChangeToImmutableArrayEmpty(objectCreation,
                                                               context.Document,
                                                               c)),
            context.Diagnostics[0]);
        }

        private async Task<Document> ChangeToImmutableArrayEmpty(
            ObjectCreationExpressionSyntax objectCreation,
            Document document,
            CancellationToken cancellationToken)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var memberAccess =
                generator.MemberAccessExpression(objectCreation.Type, "Empty");
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(objectCreation, memberAccess);

            return document.WithSyntaxRoot(newRoot);
        }

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ImmutableArrayAnalyzerAnalyzer.DiagnosticId);
    }
}
