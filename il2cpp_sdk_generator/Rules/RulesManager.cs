using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator {
  public class RulesManager {
    static List<RuleBaseClass> _rules = new List<RuleBaseClass>();
    static List<KeyValuePair<string, RuleBaseClass>> _ruleResolveOrder = new List<KeyValuePair<string, RuleBaseClass>>();
    static List<KeyValuePair<string, RuleBaseClass>> _ruleApplyOrder = new List<KeyValuePair<string, RuleBaseClass>>();
    static Dictionary<string, RuleBaseClass> _ruleDictionary = new Dictionary<string, RuleBaseClass>();
    static Dictionary<string, Type[]> _dllTypes = new Dictionary<string, Type[]>();

    public static void Initialize() {
      // Read all .dll files ending with .Rule.Dll
      string[] ruleDlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.Rules.dll", SearchOption.TopDirectoryOnly);
      // Load ruleDlls
      for (int i = 0; i < ruleDlls.Length; i++) {
        Assembly assembly = Assembly.LoadFrom(ruleDlls[i]);
        // Get all types that are children of RuleBaseClass
        var assemblyTypes = assembly.GetAllTypes().Where(mytype => mytype.IsSubclassOf(typeof(RuleBaseClass))).ToArray();
        _dllTypes.Add(ruleDlls[i], assemblyTypes);
      }

      foreach (var pair in _dllTypes) {
        foreach (var type in pair.Value) {
          var rule = (RuleBaseClass)Activator.CreateInstance(type);
          _ruleDictionary.Add(rule.FullName, rule);
        }
      }

      _OrderRuleResolvers();
      _OrderRuleAppliers();
    }

    public static void Apply() {
      foreach (var pair in _ruleResolveOrder) {
        pair.Value.ResolveObject();
      }

      foreach (var pair in _ruleApplyOrder) {
        pair.Value.Apply();
      }

      MetadataReader.mapSimpleTypeStringCache.Clear();
      MetadataReader.mapTypeStringCache.Clear();

      foreach (var pair in _ruleResolveOrder) {
        var resolvedType = pair.Value.Object;
        if (resolvedType == null)
          continue;

        if (Demangler.demangledTypes.TryGetValue(resolvedType, out var val)) {
          Demangler.demangledTypes.Remove(resolvedType);
        }

        for (int i = 0; i < resolvedType.nestedTypes.Count; i++) {
          if (Demangler.demangledTypes.TryGetValue(resolvedType.nestedTypes[i], out var val2)) {
            Demangler.demangledTypes.Remove(resolvedType.nestedTypes[i]);
            resolvedType.nestedTypes[i].isDemangled = false;
          }
        }

        resolvedType.isDemangled = false;
        resolvedType.Demangle(true);

        for (int i = 0; i < resolvedType.nestedTypes.Count; i++) {
          resolvedType.nestedTypes[i].Demangle(true);
        }
      }
    }

    public static RuleBaseClass GetRule(string name) {
      _ruleDictionary.TryGetValue(name, out var ret);
      return ret;
    }

    public static ResolvedType GetRuleObject(string ruleName) {
      _ruleDictionary.TryGetValue(ruleName, out var ret);
      if (ret == null)
        return null;

      return ret.Object;
    }

    static void _OrderRuleResolvers() {
      var rules = _ruleDictionary.ToList();

      Dictionary<RuleBaseClass, ResolveAfter[]> ruleAttributes = new Dictionary<RuleBaseClass, ResolveAfter[]>();

      var unrestricted = rules.FindAll(pair => {
        RuleBaseClass ruleBaseClassInstance = pair.Value;

        ResolveAfter[] attributes = (ResolveAfter[])Attribute.GetCustomAttributes(ruleBaseClassInstance.GetType(), typeof(ResolveAfter));

        if (attributes.Length > 0) {
          Console.WriteLine($"{ruleBaseClassInstance.FullName}");
          ruleAttributes.Add(ruleBaseClassInstance, attributes);
          return false;
        }

        return true;
      });

      _ruleResolveOrder.AddRange(unrestricted);

      foreach (var pair in unrestricted) {
        rules.Remove(pair);
      }

      while (rules.Count > 0) {
        var rulePair = rules[0];
        var rule = rulePair.Value;

        var attribs = ruleAttributes[rule];

        bool canAdd = true;
        for (int i = 0; i < attribs.Length; i++) {
          var orderedRule = _ruleResolveOrder.Any(pair => {
            if (pair.Key != attribs[i].RuleName)
              return false;

            return true;
          });

          if (orderedRule == false) {
            canAdd = false;
            break;
          }
        }

        if (canAdd) {
          _ruleResolveOrder.Add(rulePair);
          rules.Remove(rulePair);
          continue;
        }

        rules.Remove(rulePair);
        rules.Insert(rules.Count, rulePair);
      }

      //Console.WriteLine("Rule resolve order:");
      //_ruleResolveOrder.Any(pair =>
      //{
      //  Console.WriteLine($"  {pair.Key}");
      //  return false;
      //});
    }

    static void _OrderRuleAppliers() {
      var rules = _ruleDictionary.ToList();

      var unrestricted = rules.FindAll(pair => {
        RuleBaseClass ruleBaseClassInstance = pair.Value;

        ApplyAfter[] attributes = (ApplyAfter[])Attribute.GetCustomAttributes(ruleBaseClassInstance.GetType(), typeof(ApplyAfter));

        if (attributes.Length > 0)
          return false;

        return true;
      });

      _ruleApplyOrder.AddRange(unrestricted);

      foreach (var pair in unrestricted) {
        rules.Remove(pair);
      }

      _ruleApplyOrder.AddRange(rules);
      // TODO: Add code for more complex cases
    }
  }
}
