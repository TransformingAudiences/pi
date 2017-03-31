using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.Loader;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;

namespace tapi
{
    /// <summary>
    /// 
    /// </summary>
    public static class Script
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string FormatCode(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            return tree.GetRoot().ToFullString();
        }


        #region Compile Internal
        static List<MetadataReference> References
        {
            get
            {
                
                return new List<MetadataReference>{

                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(RuntimeBinderException).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ExpressionType).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Dictionary<,>).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ValueTuple<,>).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Collections")).Location),

                    MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Event).GetTypeInfo().Assembly.Location),
                };
                  
            //  var rt = MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location) as MetadataReference;
            //     var refs = new Type[]
            //     {
            //         typeof(object),
            //         typeof(Enumerable),
            //         typeof(INotifyPropertyChanged),
            //         typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException),
            //         typeof(System.Dynamic.DynamicObject),
            //         typeof(ImmutableDictionary<,>),
            //         typeof(Event),
            //         typeof(CSharpArgumentInfo),
            //         typeof(Uri)
            //     }
            //     .Select(x => x.GetTypeInfo().Assembly)
            //     .GroupBy(x => x)
            //     .Select(x => MetadataReference.CreateFromFile(x.Key.Location))
            //     .Cast<MetadataReference>()
            //     .ToList();
            //     refs.Add(rt);
            //     return refs;
            }
        }
       
       
        static Dictionary<string, object> _cash = new Dictionary<string, object>();
        static object lockobj = new object();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static T Compile<T>(string script,string[] argNames)
        {
            var fsignatur = typeof(T).PrittyPrint();
            var marguments = string.Join(", ", typeof(T).GenericTypeArguments.Select(x => x.PrittyPrint()).Take(argNames.Length).Select((x, i) => x + " " + argNames[i]));
            var returnType = typeof(T).GenericTypeArguments.Select(x => x.PrittyPrint()).Last();

            var code = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using tapi;

public class DynamicScript
{{
    public {fsignatur} TheScript = new {fsignatur}(thescript);
    public static {returnType} thescript({marguments})
    {{
        {script}
    }}
}}
";


            lock (lockobj)
            {
                if (!_cash.ContainsKey(code))
                {
                    _cash.Add(code, CompileInternal(code));
                }
                return (T)_cash[code];
            }
        }

        static int counter = 0;        
        private static object CompileInternal(string code)
        {

            var tree = CSharpSyntaxTree.ParseText(code, encoding: Encoding.UTF8);
            var compilation = CSharpCompilation.Create("ModelScript" + counter++);
            compilation = compilation.AddReferences(References)
                                     .AddSyntaxTrees(tree)
                                     .WithOptions(compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                                                                     .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));

            
            using (var ms = new MemoryStream())
            {
                       var emitedCode = compilation.Emit(ms);
                if (emitedCode.Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
                    throw new ArgumentException(emitedCode.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => "Line:" + (x.Location.GetMappedLineSpan().StartLinePosition.Line - 13) + ", Message:" + x.GetMessage()).FirstOrDefault());




                ms.Position = 0;
                //var data = ms.ToArray();
                try
                {
                    var ass = AssemblyLoadContext.Default.LoadFromStream(ms);

                    var type = ass.GetType("DynamicScript");
                    var func = type.GetField("TheScript").GetValue(Activator.CreateInstance(type));

                    return func;
                }
                catch (Exception ex)
                {
                    throw ex;

                }
            }
        }
        #endregion

        public static string PrittyPrint(this Type type, bool fullName = false)
        {
            var name = fullName ? type.FullName : type.Name;

            if (type.IsConstructedGenericType)
            {
                var types = type.GetGenericArguments().Select(x => x.PrittyPrint(fullName)).ToList();
                var index = name.IndexOf('`');
                return name.Substring(0, index) + "<" + string.Join(", ", types) + ">";
            }
            return name;
        }
    }

    
}
