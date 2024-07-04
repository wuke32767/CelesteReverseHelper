using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
namespace Celeste.Mod.ReverseHelper.SourceGen
{
    internal static class _FullName
    {
        internal static string FullNamespace(this ITypeSymbol type)
        {
            string s = "";
            ISymbol tp2 = type.ContainingSymbol;
            return tp2.FullName();
        }
        static SymbolDisplayFormat callingformatter = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions:
                    SymbolDisplayMemberOptions.IncludeContainingType,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
        static SymbolDisplayFormat declarationformatter = new(
            genericsOptions: 
            SymbolDisplayGenericsOptions.IncludeTypeParameters|
            SymbolDisplayGenericsOptions.IncludeTypeConstraints|
            SymbolDisplayGenericsOptions.IncludeVariance
            ,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeModifiers |
                SymbolDisplayMemberOptions.IncludeType|
                SymbolDisplayMemberOptions.IncludeAccessibility|
                SymbolDisplayMemberOptions.IncludeParameters |
                SymbolDisplayMemberOptions.IncludeRef
            ,
            
            kindOptions:
            SymbolDisplayKindOptions.IncludeNamespaceKeyword|
            SymbolDisplayKindOptions.IncludeTypeKeyword|
            SymbolDisplayKindOptions.IncludeMemberKeyword,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        internal static string Declared(this ISymbol tp2)
        {
            return tp2.ToDisplayString(declarationformatter);
        }
        internal static string FullName(this ISymbol tp2)
        {
            return tp2.ToDisplayString(callingformatter);
        }
        internal static string Voidize(this string isvoid)
        {
            if (isvoid == "global::System.Void" || isvoid == "System.Void")
            {
                return "void";
            }
            return isvoid;
        }
    }
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private const bool format_indent = false;
        private const string attr_namespace_base = "global::Celeste.Mod.ReverseHelper.SourceGen";
        private const string attr_namespace = attr_namespace_base + ".Loader";

        public void Execute(GeneratorExecutionContext context)
        {
            var rec = context.SyntaxContextReceiver as Receiver;
            if (rec is null)
            {
                return;
            }
            StringBuilder load = new();
            StringBuilder unload = new();
            StringBuilder loadcontent = new();
            Dictionary<ITypeSymbol, ITypeSymbol> typedeps = new();
            Dictionary<string, ITypeSymbol> names = new();
            Dictionary<ITypeSymbol, IMethodSymbol> loads = new();
            Dictionary<ITypeSymbol, IMethodSymbol> unloads = new();
            Dictionary<ITypeSymbol, string> alloc = new();
            //Debugger.Launch();
            foreach (var type in context.Compilation.GetSymbolsWithName(_ => true).OfType<ITypeSymbol>())
            {
                foreach (var attr in type.GetAttributes())
                {
                    if (attr.AttributeClass.FullNamespace() == "global::MonoMod.ModInterop")
                    {
                        if (attr.AttributeClass.Name == "ModExportNameAttribute"
                            || attr.AttributeClass.Name == "ModImportNameAttribute")
                        {
                            load.Append($"global::MonoMod.ModInterop.ModInteropManager.ModInterop(typeof({type.FullName()}));");
                        }
                    }
                    else if (attr.AttributeClass.FullNamespace() == attr_namespace)
                    {
                        if (attr.AttributeClass.Name == "DependencyAttribute")
                        {
                            typedeps.Add(type, attr.AttributeClass.TypeArguments.FirstOrDefault());
                        }
                    }
                    else if (attr.AttributeClass.FullNamespace() == "global::Celeste.Mod.Entities")
                    {
                        if (attr.AttributeClass.Name == "CustomEntityAttribute")
                        {
                            var a = attr.ConstructorArguments;
                            if (a.Length == 1)
                            {
                                var b = a.First();
                                if (b.Kind == TypedConstantKind.Array)
                                {
                                    foreach (var name in b.Values.Select(x => x.Value).OfType<string>())
                                    {
                                        names.Add(name, type);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var method in context.Compilation.GetSymbolsWithName(_ => true).OfType<IMethodSymbol>())
            {
                foreach (var attr in method.GetAttributes())
                {
                    //log.Append(attr.AttributeClass.FullName()).Append(' ');
                    if (attr.AttributeClass.FullNamespace() == attr_namespace)
                    {
                        if (attr.AttributeClass.Name switch
                        {
                            "LoadAttribute" => load,
                            "UnloadAttribute" => unload,
                            "LoadContentAttribute" => loadcontent,
                            _ => null,
                        } is StringBuilder sb)
                        {
                            sb.Append(method.FullName());
                            sb.Append("();");
                            break;
                        }
                    }
                }
            }
            BuildPartialClass(rec.loader, load, rec.loadermod);
            BuildPartialClass(rec.unloader, unload, rec.unloadermod);
            BuildPartialClass(rec.loadcontenter, loadcontent, rec.loadcontentermod);
            var src = load.ToString() + unload.ToString() + loadcontent.ToString();
            if (format_indent)
            {
                StringBuilder ret = new StringBuilder();
                var read = new StringReader(src);
                int indent = 0;
                bool newl = false;
                void endl()
                {
                    newl = true;
                    ret.Append('\n');
                }
                void append(int v)
                {
                    if (newl)
                    {
                        ret.Append('\t', indent);
                        newl = false;
                    }
                    ret.Append((char)v);
                }
                while (true)
                {
                    int lx = read.Read();
                    if (lx == -1)
                    {
                        break;
                    }
                    else if (lx == '{')
                    {
                        endl();
                        append(lx);
                        indent++;
                        endl();
                    }
                    else if (lx == '}')
                    {
                        indent--;
                        append(lx);
                        endl();
                    }
                    else if (lx == ';')
                    {
                        append(lx);
                        endl();
                    }
                    else if (lx == ']')
                    {
                        append(lx);
                        endl();
                    }
                    else
                    {
                        append(lx);
                    }
                }
                src = ret.ToString();
            }
            context.AddSource("AllLoader.g.cs", src);
        }

        private void BuildPartialClass(IMethodSymbol? loader, StringBuilder str, string mod)
        {
            if (loader is null)
            {
                str.Clear();
                return;
            }
            ISymbol tp2 = loader;
            while (tp2 is not null && tp2.Name != "")
            {
                _ = tp2 switch
                {
                    IMethodSymbol method => str.Insert(0, $"[{attr_namespace_base}.GeneratedAttribute]{mod} {method.ReturnType.FullName().Voidize()} {method.Name}(){{").Append($"}}"),
                    INamespaceSymbol ns => str.Insert(0, $"namespace {ns.Name}{{").Append($"}}"),
                    ITypeSymbol type => str.Insert(0, $"partial class {type.Name}{{").Append($"}}"),
                };
                tp2 = tp2.ContainingSymbol;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
            context.RegisterForSyntaxNotifications(() => new Receiver());
        }
        internal class Receiver : ISyntaxContextReceiver
        {
            public IMethodSymbol loader = null;
            public IMethodSymbol unloader = null;
            public IMethodSymbol loadcontenter = null;
            public string loadermod = null;
            public string unloadermod = null;
            public string loadcontentermod = null;

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is MethodDeclarationSyntax method)
                {
                    var node = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;
                    foreach (var attr in node.GetAttributes())
                    {
                        //log.Append(attr.AttributeClass.FullName()).Append(' ');
                        if (attr.AttributeClass.FullNamespace() == attr_namespace)
                        {
                            switch (attr.AttributeClass.Name)
                            {
                                case "LoaderAttribute":
                                    loader = node;
                                    loadermod = method.Modifiers.ToString();
                                    break;
                                case "UnloaderAttribute":
                                    unloader = node;
                                    unloadermod = method.Modifiers.ToString();
                                    break;
                                case "LoadContenterAttribute":
                                    loadcontenter = node;
                                    loadcontentermod = method.Modifiers.ToString();
                                    break;
                                default:
                                    //continue;
                                    goto next;
                            }
                            break;
                        next:;
                        }
                    }

                }
            }
        }
    }

}

