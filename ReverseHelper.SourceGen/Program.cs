using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text;
#pragma warning disable CS0162
#nullable enable
namespace Celeste.Mod.ReverseHelper.SourceGen
{
    internal static class _FullName
    {
        public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> self, out TK k, out TV v)
        {
            k = self.Key;
            v = self.Value;
        }
        public static bool TryAdd<TK, TV>(this Dictionary<TK, TV> self, TK k, TV v)
        {
            if (!self.ContainsKey(k))
            {
                self[k] = v;
                return true;
            }
            return false;
        }
        internal static string FullNamespace(this ITypeSymbol type)
        {
            string s = "";
            ISymbol tp2 = type.ContainingSymbol;
            return tp2.FullName();
        }
        //global::Celeste.Mod.ReverseHelperModule
        //global::Celeste.Mod.ReverseHelperModule.Load
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
            SymbolDisplayGenericsOptions.IncludeTypeParameters |
            SymbolDisplayGenericsOptions.IncludeTypeConstraints |
            SymbolDisplayGenericsOptions.IncludeVariance
            ,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeModifiers |
                SymbolDisplayMemberOptions.IncludeType |
                SymbolDisplayMemberOptions.IncludeAccessibility |
                SymbolDisplayMemberOptions.IncludeParameters |
                SymbolDisplayMemberOptions.IncludeRef
            ,

            kindOptions:
            SymbolDisplayKindOptions.IncludeNamespaceKeyword |
            SymbolDisplayKindOptions.IncludeTypeKeyword |
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
        private const bool format_indent = true;
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
            Dictionary<IMethodSymbol, string> loadendpoint = new();
            Dictionary<IMethodSymbol, string> unloadendpoint = new();
            Dictionary<IMethodSymbol, string> loadcontentendpoint = new();
            HashSet<ITypeSymbol> lazylist = new();
            Dictionary<ITypeSymbol, IMethodSymbol> lazyloadlist = new();
            Dictionary<ITypeSymbol, IMethodSymbol> lazyunloadlist = new();
            Dictionary<ITypeSymbol, List<ITypeSymbol>> typedeps = new();
            Dictionary<string, ITypeSymbol> names = new();
            Dictionary<ITypeSymbol, List<string>> revnames = new();
            Dictionary<ITypeSymbol, IMethodSymbol> loads = new();
            Dictionary<ITypeSymbol, IMethodSymbol> unloads = new();
            Dictionary<ITypeSymbol, List<string>> Preload = new();
            if (false)
            {
                Debugger.Launch();
            }
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
                        switch (attr.AttributeClass.Name)
                        {
                            case "DependencyAttribute":

                                if (!typedeps.TryGetValue(type, out var val))
                                {
                                    typedeps.Add(type, val = new List<ITypeSymbol>());
                                }
                                //val.Add(attr.AttributeClass.TypeArguments.FirstOrDefault());
                                val.AddRange(attr.ConstructorArguments[0].Values.Select(x => x.Value as ITypeSymbol));
                                break;


                            case "PreloadAttribute":

                                if (!Preload.TryGetValue(type, out var val2))
                                {
                                    Preload.Add(type, val2 = new List<string>());
                                }
                                val2.AddRange(attr.ConstructorArguments[0].Values.Select(x => x.Value as string));
                                break;

                            case "LazyLoadAttribute":
                                lazylist.Add(type);
                                break;
                            default:
                                break;
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
                                    var eid = b.Values.Select(x => x.Value).OfType<string>();
                                    foreach (var name in eid)
                                    {
                                        names.Add(name, type);
                                    }
                                    revnames.Add(type, eid.ToList());
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
                    if (attr.AttributeClass.FullNamespace() == attr_namespace)
                    {
                        switch (attr.AttributeClass.Name)
                        {
                            case "LoadAttribute":
                                loads.Add(method.ContainingType, method);
                                loadendpoint.Add(method, method.FullName() + "();");
                                break;
                            case "UnloadAttribute":
                                unloads.Add(method.ContainingType, method);
                                unloadendpoint.Add(method, method.FullName() + "();");
                                break;
                            case "LoadContentAttribute":
                                loadcontentendpoint.Add(method, method.FullName() + "();");
                                break;
                            default: continue;
                        }
                        break;
                    }
                }
            }
            Dictionary<IMethodSymbol, string> lazyloadep = new();
            Dictionary<IMethodSymbol, string> lazyunloadep = new();
            StringBuilder lazy_tocreate = new();
            foreach (var type in lazylist)
            {
                var ll = loads[type];
                var uu = unloads[type];
                lazy_tocreate.Append(
                    $$"""

                        public static class Lazy{{type.Name}}
                        {
                            static bool loaded = false;
                            public static void Load()
                            {
                                if (loaded) return;
                                loaded = true;
                                {{loadendpoint[ll]}}
                            }
                            public static void Unload()
                            {
                                if (!loaded) return;
                                loaded = false;
                                {{unloadendpoint[uu]}}
                            }
                        }
                    """
                    );
                lazyloadep[ll] = $"SourceGenHelper.Lazy{type.Name}.Load();";
                loadendpoint.Remove(ll);
                lazyunloadep[uu] = unloadendpoint[uu] = $"SourceGenHelper.Lazy{type.Name}.Unload();";
            }
            StringBuilder pr = new();

            Dictionary<IMethodSymbol, string> prevlazyloadep = new();
            foreach (var (v, l) in typedeps)
            {
                if (!loads.TryGetValue(v, out var ll))
                {
                    ll = new FakeMethodSymbol(v);

                }

                //var point2 = lazyunloadep[uu];
                lazy_tocreate.Append(
                    $$"""

                            public static class Dep{{v.Name}}
                            {
                                public static void Load()
                                {
                                    {{(lazyloadep.TryGetValue(ll, out var ep) ? ep : "")}}
                                    {{string.Concat(l.Select(x => typedeps.ContainsKey(x) ?
                    $""" 

                                    SourceGenHelper.Dep{x.Name}.Load();
                        """
                                : (lazyloadep.TryGetValue(loads[x], out var ep) ? ep : "")))}}
                                }
                            }
                        """
                );
                lazyloadep[ll] = $"SourceGenHelper.Dep{v.Name}.Load();";
            }

            if (Preload.Count > 0 || lazyloadep.Count > 0)
            {
                load.Append($$"""global::Celeste.Mod.Everest.Events.LevelLoader.OnLoadingThread += SourceGenHelper.LoadingThread;""");
                load.Append($$"""global::On.Celeste.OverworldLoader.ctor += SourceGenHelper.OnOverworldLoad;""");
                unload.Append($$"""global::On.Celeste.OverworldLoader.ctor -= SourceGenHelper.OnOverworldLoad;""");
                unload.Append($$"""global::Celeste.Mod.Everest.Events.LevelLoader.OnLoadingThread -= SourceGenHelper.LoadingThread;""");
                pr.Append(
                $$"""
                [{{attr_namespace_base}}.Generated]
                file partial class SourceGenHelper
                {
                    public static void LoadingThread(global::Celeste.Level level)
                    {
                        foreach(var ed in level.Session.MapData.Levels.SelectMany(x => x.Entities))
                        {{{string.Concat(Preload.SelectMany(x =>
                            x.Value.Select(y =>
                    $$"""

                                if(ed.Name == "{{y}}")
                                {
                                    global::Celeste.Level.LoadCustomEntity(ed, level);
                                }
                    """
                            )))}}
                            {{string.Concat(lazyloadep.SelectMany(x =>
                                revnames.TryGetValue(x.Key.ContainingType, out var qwe) ? qwe.Select(y =>
                    $$"""

                                if(ed.Name == "{{y}}")
                                {
                                    {{x.Value}}
                                }
                    """
                            ) : []))}}
                        }
                    }
                    public static void OnOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, global::Celeste.OverworldLoader self, global::Celeste.Overworld.StartMode startMode, global::Celeste.HiresSnow snow)
                    {
                        orig(self, startMode, snow);

                        {{string.Join("\n        ", lazyunloadep.Values)}}
                    }


                    {{lazy_tocreate}}
                }
                """);

            }


            load.Append(string.Concat(loadendpoint.Values));
            unload.Append(string.Concat(unloadendpoint.Values));
            loadcontent.Append(string.Concat(loadcontentendpoint.Values));


            BuildPartialClass(rec.loader, load, rec.loadermod);
            BuildPartialClass(rec.unloader, unload, rec.unloadermod);
            BuildPartialClass(rec.loadcontenter, loadcontent, rec.loadcontentermod);
            var src = load.ToString() + unload.ToString() + loadcontent.ToString()
                + string.Concat(Preload.Select(x => AddAttr(x.Key, $"""[global::Celeste.Mod.Entities.CustomEntity("{string.Join(@""",""", x.Value)}")]""")));
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
            src += pr.ToString();
            context.AddSource("AllLoader.g.cs", src);
        }


        private string AddAttr(ITypeSymbol? loader, string mod)
        {
            StringBuilder sb = new();
            ISymbol tp2 = loader;
            while (tp2 is not null && tp2.Name != "")
            {
                _ = tp2 switch
                {
                    INamespaceSymbol ns => sb.Insert(0, $"namespace {ns.Name}{{").Append($"}}"),
                    ITypeSymbol type => sb.Insert(0, $"{mod} partial class {type.Name}{{").Append($"}}"),
                };
                tp2 = tp2.ContainingSymbol;
            }
            return sb.ToString();
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
                if (tp2 is IMethodSymbol method)
                {
                    str.Insert(0, $"[{attr_namespace_base}.Generated]{mod} {method.ReturnType.FullName().Voidize()} {method.Name}(){{").Append($"}}");
                }
                else if (tp2 is INamespaceSymbol ns)
                {
                    str.Insert(0, $"namespace {ns.Name}{{").Append($"}}");
                }
                else if (tp2 is ITypeSymbol type)
                {
                    str.Insert(0, $"partial class {type.Name}{{").Append($"}}");
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

