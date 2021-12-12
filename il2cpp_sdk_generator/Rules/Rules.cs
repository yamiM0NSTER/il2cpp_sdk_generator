using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace il2cpp_sdk_generator
{
  public class Rules
  {
    static List<Type> rules = new List<Type>();
    static Dictionary<string, Type> mapObjRule = new Dictionary<string, Type>();

    public static void Initialize()
    {
      // Read all .dll files ending with .Rule.Dll
      string[] ruleDlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.Rules.dll", SearchOption.TopDirectoryOnly);
      // Load ruleDlls
      for (int i = 0; i < ruleDlls.Length; i++)
      {
        Assembly assembly = Assembly.LoadFrom(ruleDlls[i]);
        // Get all types containing IRule interface
        rules.AddRange(assembly.GetAllTypes().Where(mytype => typeof(IRule).IsAssignableFrom(mytype)
                            && mytype.GetInterfaces().Contains(typeof(IRule))
                            && mytype.IsSubclassOf(typeof(RuleBase))).ToArray());
      }

      for (int i = 0; i < rules.Count; i++)
      {
        string name = (string)rules[i].GetField("object_name").GetValue(null);
        mapObjRule.Add(name, rules[i]);
      }
    }

    public static void AssignResolvedObject(ResolvedType resolvedType)
    {
      if (!mapObjRule.TryGetValue(resolvedType.GetFullName(), out var type))
        return;

      type.GetField("resolved_object").SetValue(null, resolvedType);
    }

    public static void Apply()
    {
      for (int i = 0; i < rules.Count; i++)
      {
        var resolveObject = rules[i].GetMethod("ResolveObject");
        if (resolveObject == null)
          continue;

        resolveObject.Invoke(null, null);
      }

      for (int i = 0; i < rules.Count; i++)
      {
        var apply = rules[i].GetMethod("Apply");

        if (apply == null)
          continue;

        apply.Invoke(null, null);
      }

      MetadataReader.mapSimpleTypeStringCache.Clear();
      MetadataReader.mapTypeStringCache.Clear();

      for (int i = 0; i < rules.Count; i++)
      {
        var resolvedType = rules[i].GetField("resolved_object").GetValue(null) as ResolvedType;
        if (resolvedType == null)
          continue;

        if (Demangler.demangledTypes.TryGetValue(resolvedType, out var val))
        {
          Demangler.demangledTypes.Remove(resolvedType);
        }


        resolvedType.isDemangled = false;
        resolvedType.Demangle(true);
      }
    }
  }
}
