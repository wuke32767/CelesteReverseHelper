using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
#pragma warning disable CS0162
#nullable enable
namespace Celeste.Mod.ReverseHelper.SourceGen
{
    internal class FakeMethodSymbol(ITypeSymbol ContainingType) : IMethodSymbol
    {
        MethodKind IMethodSymbol.MethodKind => throw new NotImplementedException();

        int IMethodSymbol.Arity => throw new NotImplementedException();

        bool IMethodSymbol.IsGenericMethod => throw new NotImplementedException();

        bool IMethodSymbol.IsExtensionMethod => throw new NotImplementedException();

        bool IMethodSymbol.IsAsync => throw new NotImplementedException();

        bool IMethodSymbol.IsVararg => throw new NotImplementedException();

        bool IMethodSymbol.IsCheckedBuiltin => throw new NotImplementedException();

        bool IMethodSymbol.HidesBaseMethodsByName => throw new NotImplementedException();

        bool IMethodSymbol.ReturnsVoid => throw new NotImplementedException();

        bool IMethodSymbol.ReturnsByRef => throw new NotImplementedException();

        bool IMethodSymbol.ReturnsByRefReadonly => throw new NotImplementedException();

        RefKind IMethodSymbol.RefKind => throw new NotImplementedException();

        ITypeSymbol IMethodSymbol.ReturnType => throw new NotImplementedException();

        NullableAnnotation IMethodSymbol.ReturnNullableAnnotation => throw new NotImplementedException();

        ImmutableArray<ITypeSymbol> IMethodSymbol.TypeArguments => throw new NotImplementedException();

        ImmutableArray<NullableAnnotation> IMethodSymbol.TypeArgumentNullableAnnotations => throw new NotImplementedException();

        ImmutableArray<ITypeParameterSymbol> IMethodSymbol.TypeParameters => throw new NotImplementedException();

        ImmutableArray<IParameterSymbol> IMethodSymbol.Parameters => throw new NotImplementedException();

        IMethodSymbol IMethodSymbol.ConstructedFrom => throw new NotImplementedException();

        bool IMethodSymbol.IsReadOnly => throw new NotImplementedException();

        bool IMethodSymbol.IsInitOnly => throw new NotImplementedException();

        IMethodSymbol IMethodSymbol.OriginalDefinition => throw new NotImplementedException();

        ISymbol ISymbol.OriginalDefinition => throw new NotImplementedException();

        IMethodSymbol? IMethodSymbol.OverriddenMethod => throw new NotImplementedException();

        ITypeSymbol? IMethodSymbol.ReceiverType => throw new NotImplementedException();

        NullableAnnotation IMethodSymbol.ReceiverNullableAnnotation => throw new NotImplementedException();

        IMethodSymbol? IMethodSymbol.ReducedFrom => throw new NotImplementedException();

        ImmutableArray<IMethodSymbol> IMethodSymbol.ExplicitInterfaceImplementations => throw new NotImplementedException();

        ImmutableArray<CustomModifier> IMethodSymbol.ReturnTypeCustomModifiers => throw new NotImplementedException();

        ImmutableArray<CustomModifier> IMethodSymbol.RefCustomModifiers => throw new NotImplementedException();

        SignatureCallingConvention IMethodSymbol.CallingConvention => throw new NotImplementedException();

        ImmutableArray<INamedTypeSymbol> IMethodSymbol.UnmanagedCallingConventionTypes => throw new NotImplementedException();

        ISymbol? IMethodSymbol.AssociatedSymbol => throw new NotImplementedException();

        IMethodSymbol? IMethodSymbol.PartialDefinitionPart => throw new NotImplementedException();

        IMethodSymbol? IMethodSymbol.PartialImplementationPart => throw new NotImplementedException();

        MethodImplAttributes IMethodSymbol.MethodImplementationFlags => throw new NotImplementedException();

        bool IMethodSymbol.IsPartialDefinition => throw new NotImplementedException();

        INamedTypeSymbol? IMethodSymbol.AssociatedAnonymousDelegate => throw new NotImplementedException();

        bool IMethodSymbol.IsConditional => throw new NotImplementedException();

        SymbolKind ISymbol.Kind => throw new NotImplementedException();

        string ISymbol.Language => throw new NotImplementedException();

        string ISymbol.Name => throw new NotImplementedException();

        string ISymbol.MetadataName => throw new NotImplementedException();

        int ISymbol.MetadataToken => throw new NotImplementedException();

        ISymbol ISymbol.ContainingSymbol => throw new NotImplementedException();

        IAssemblySymbol ISymbol.ContainingAssembly => throw new NotImplementedException();

        IModuleSymbol ISymbol.ContainingModule => throw new NotImplementedException();

        INamedTypeSymbol ISymbol.ContainingType => (INamedTypeSymbol)ContainingType;

        INamespaceSymbol ISymbol.ContainingNamespace => throw new NotImplementedException();

        bool ISymbol.IsDefinition => throw new NotImplementedException();

        bool ISymbol.IsStatic => throw new NotImplementedException();

        bool ISymbol.IsVirtual => throw new NotImplementedException();

        bool ISymbol.IsOverride => throw new NotImplementedException();

        bool ISymbol.IsAbstract => throw new NotImplementedException();

        bool ISymbol.IsSealed => throw new NotImplementedException();

        bool ISymbol.IsExtern => throw new NotImplementedException();

        bool ISymbol.IsImplicitlyDeclared => throw new NotImplementedException();

        bool ISymbol.CanBeReferencedByName => throw new NotImplementedException();

        ImmutableArray<Location> ISymbol.Locations => throw new NotImplementedException();

        ImmutableArray<SyntaxReference> ISymbol.DeclaringSyntaxReferences => throw new NotImplementedException();

        Accessibility ISymbol.DeclaredAccessibility => throw new NotImplementedException();

        bool ISymbol.HasUnsupportedMetadata => throw new NotImplementedException();

        void ISymbol.Accept(SymbolVisitor visitor)
        {
            throw new NotImplementedException();
        }

        TResult? ISymbol.Accept<TResult>(SymbolVisitor<TResult> visitor) where TResult : default
        {
            throw new NotImplementedException();
        }

        TResult ISymbol.Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            throw new NotImplementedException();
        }

        IMethodSymbol IMethodSymbol.Construct(params ITypeSymbol[] typeArguments)
        {
            throw new NotImplementedException();
        }

        IMethodSymbol IMethodSymbol.Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations)
        {
            throw new NotImplementedException();
        }

        bool ISymbol.Equals(ISymbol? other, SymbolEqualityComparer equalityComparer)
        {
            throw new NotImplementedException();
        }

        bool IEquatable<ISymbol?>.Equals(ISymbol? other)
        {
            return this == other;
        }

        ImmutableArray<AttributeData> ISymbol.GetAttributes()
        {
            throw new NotImplementedException();
        }

        DllImportData? IMethodSymbol.GetDllImportData()
        {
            throw new NotImplementedException();
        }

        string? ISymbol.GetDocumentationCommentId()
        {
            throw new NotImplementedException();
        }

        string? ISymbol.GetDocumentationCommentXml(CultureInfo? preferredCulture, bool expandIncludes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<AttributeData> IMethodSymbol.GetReturnTypeAttributes()
        {
            throw new NotImplementedException();
        }

        ITypeSymbol? IMethodSymbol.GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter)
        {
            throw new NotImplementedException();
        }

        IMethodSymbol? IMethodSymbol.ReduceExtensionMethod(ITypeSymbol receiverType)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<SymbolDisplayPart> ISymbol.ToDisplayParts(SymbolDisplayFormat? format)
        {
            throw new NotImplementedException();
        }

        string ISymbol.ToDisplayString(SymbolDisplayFormat? format)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<SymbolDisplayPart> ISymbol.ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format)
        {
            throw new NotImplementedException();
        }

        string ISymbol.ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format)
        {
            throw new NotImplementedException();
        }
    }

}

