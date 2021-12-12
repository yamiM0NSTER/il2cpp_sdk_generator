using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator {
  public class RuleBaseClass {
    List<KeyValuePair<string, MethodInfo>> _methodResolvers = new List<KeyValuePair<string, MethodInfo>>();
    //Dictionary<string, MethodInfo> _methodResolvers = new Dictionary<string, MethodInfo>();
    Dictionary<string, MethodInfo> _invalidMethodResolvers = new Dictionary<string, MethodInfo>();
    //Dictionary<string, ResolvedMethod> _resolvedMethods = new Dictionary<string, ResolvedMethod>();
    List<KeyValuePair<string, ResolvedMethod>> _resolvedMethods = new List<KeyValuePair<string, ResolvedMethod>>();
    Dictionary<string, MethodInfo> _propertyResolvers = new Dictionary<string, MethodInfo>();
    Dictionary<string, MethodInfo> _invalidPropertyResolvers = new Dictionary<string, MethodInfo>();
    Dictionary<string, ResolvedProperty> _resolvedProperties = new Dictionary<string, ResolvedProperty>();

    public ResolvedType Object { get; set; } = null;
    private ResolvedType[] _candidates = new ResolvedType[0];
    public string Name { get; set; } = "BaseRule";
    public string Namespace { get; set; } = "";
    public string FullName {
      get {
        var namespaze = this.Namespace == "" ? this.BaseNamespace : this.Namespace;

        if (namespaze == "")
          return this.Name;

        return $"{namespaze.Replace(".", "::")}::{this.Name}";
      }
    }

    public string Image { get; set; } = "Assembly-CSharp.dll";
    public string BaseNamespace { get; set; } = "";

    public RuleBaseClass() {
      var type = this.GetType();
      var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
      foreach (var method in methods) {
        if (method.Name.StartsWith("method_")) {
          MethodName methodName = (MethodName)Attribute.GetCustomAttribute(method, typeof(MethodName));
          string name = methodName != null ? methodName.Name : method.Name.Substring("method_".Length);

          if (method.ReturnType != typeof(ResolvedMethod[])) {
            this._invalidMethodResolvers.Add(name, method);
            continue;
          }
          this._methodResolvers.Add(new KeyValuePair<string, MethodInfo>(name, method));
          //this._methodResolvers.Add(method.Name.Substring("method_".Length), method);
        }
        else if (method.Name.StartsWith("property_")) {
          if (method.ReturnType != typeof(ResolvedProperty[])) {
            this._invalidPropertyResolvers.Add(method.Name.Substring("property_".Length), method);
            continue;
          }
          this._propertyResolvers.Add(method.Name.Substring("property_".Length), method);
        }
      }

      this._OrderMethodResolvers();
    }

    void _OrderMethodResolvers() {
      this._methodResolvers.Sort((pair1, pair2) => {
        ResolveOrder pair1ResolveOrder = (ResolveOrder)Attribute.GetCustomAttribute(pair1.Value, typeof(ResolveOrder));
        int pair1Order = pair1ResolveOrder != null ? pair1ResolveOrder.Order : Int32.MaxValue;

        ResolveOrder pair2ResolveOrder = (ResolveOrder)Attribute.GetCustomAttribute(pair2.Value, typeof(ResolveOrder));
        int pair2Order = pair2ResolveOrder != null ? pair2ResolveOrder.Order : Int32.MaxValue;

        return pair1Order.CompareTo(pair2Order);
      });
    }

    public void ResolveObject() {
      if (this.Object != null)
        return;

      var candidates = this._resolveObject();

      if (candidates.Length != 1) {
        this._candidates = candidates;
        return;
      }

      this.Object = candidates[0];
      this.Object.Name = this.Name;
      this.Object.isMangled = false;
      if (this.Namespace != "") {
        this.Object.Namespace = this.Namespace;
        ResolvedImage image = Metadata.resolvedImages.Find(img => img.Name == this.Image);
        ResolvedNamespace baseNamespaze = image.Namespaces[this.BaseNamespace];

        if (!image.Namespaces.TryGetValue(this.Namespace, out var resolvedNamespace)) {
          resolvedNamespace = new ResolvedNamespace();
          resolvedNamespace.Name = this.Namespace;
          image.Namespaces.Add(this.Namespace, resolvedNamespace);
        }

        if (!this.Object.isEnum) {
          resolvedNamespace.Types.Add(this.Object);
          baseNamespaze.Types.Remove(this.Object);
        }
        else {
          resolvedNamespace.Enums.Add(this.Object);
          baseNamespaze.Enums.Remove(this.Object);
        }
      }

      foreach (var method in this.Object.miMethods) {
        if (method.isMangled)
          continue;

        this._resolvedMethods.Add(new KeyValuePair<string, ResolvedMethod>(method.Name, method));
      }
      return;
    }

    protected virtual ResolvedType[] _resolveObject() {
      return new ResolvedType[0];
    }

    public virtual void Apply() {
      Console.WriteLine("=====================================================");
      Console.Write($"= {this.FullName}");
      if (this.Object == null) {
        if (this._candidates.Length == 0) {
          Console.Write(": ");
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Failed to find candidates");
        }
        else {
          Console.Write(": ");
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Found {this._candidates.Length} candidates:");
          for (int i = 0; i < this._candidates.Length; i++) {
            Console.WriteLine($" =  {this._candidates[i].Name}");
          }
        }

        Console.ForegroundColor = ConsoleColor.Gray;

        return;
      }
      Console.WriteLine();
      this._Apply();
    }

    protected virtual void _Apply() {
      this.ResolveMethods();
      this.ResolveProperties();
    }

    void ResolveMethods() {
      if (this._methodResolvers.Count == 0 && this._invalidMethodResolvers.Count == 0)
        return;

      Console.WriteLine(" = Methods =");
      foreach (var pair in this._methodResolvers) {
        Console.Write($"  = {pair.Key} : ");
        var candidates = (ResolvedMethod[])pair.Value.Invoke(this, new object[0]);
        if (candidates.Length == 1) {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine("OK");
          Console.ForegroundColor = ConsoleColor.Gray;
          if (candidates[0].isOverride) {
            candidates[0].overridenMethod.Name = pair.Key;
            candidates[0].overridenMethod.isMangled = false;
          }
          else {
            candidates[0].Name = pair.Key;
            candidates[0].isMangled = false;
          }
          this._resolvedMethods.Add(new KeyValuePair<string, ResolvedMethod>(pair.Key, candidates[0]));
        }
        else if (candidates.Length == 0) {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("No candidates found");
          Console.ForegroundColor = ConsoleColor.Gray;
        }
        else {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Found {candidates.Length} candidates:");
          foreach (var candidate in candidates) {
            Console.WriteLine($" = {candidate.Name} 0x{candidate.methodPtr:X16} calls: {CodeScanner.m_mapFunctionReferences[candidate.methodPtr].Count} references: {CodeScanner.m_mapReferencesToFunction[candidate.methodPtr].Count}");
          }
          Console.ForegroundColor = ConsoleColor.Gray;
        }
      }
      Console.ForegroundColor = ConsoleColor.DarkGray;
      foreach (var pair in this._invalidMethodResolvers) {
        Console.WriteLine($"= {pair.Key} (skipped)");
      }
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    void ResolveProperties() {
      if (this._propertyResolvers.Count == 0 && this._invalidPropertyResolvers.Count == 0)
        return;

      Console.WriteLine(" = Properties =");
      foreach (var pair in this._propertyResolvers) {
        Console.Write($"  = {pair.Key} : ");
        var candidates = (ResolvedProperty[])pair.Value.Invoke(this, new object[0]);
        if (candidates.Length == 1) {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine("OK");
          Console.ForegroundColor = ConsoleColor.Gray;
          candidates[0].Name = pair.Key;
          candidates[0].DemangleMethods();
          candidates[0].isMangled = false;
          this._resolvedProperties.Add(pair.Key, candidates[0]);
        }
        else if (candidates.Length == 0) {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine("No candidates found");
          Console.ForegroundColor = ConsoleColor.Gray;
        }
        else {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Found {candidates.Length} candidates:");
          foreach (var candidate in candidates) {
            Console.WriteLine($" = {candidate.Name}:");
            if (candidate.getter != null)
              Console.WriteLine($"   getter: 0x{candidate.getter.methodPtr:X16}");
            if (candidate.setter != null)
              Console.WriteLine($"   setter: 0x{candidate.setter.methodPtr:X16}");
          }
          Console.ForegroundColor = ConsoleColor.Gray;
        }
      }
      Console.ForegroundColor = ConsoleColor.DarkGray;
      foreach (var pair in this._invalidMethodResolvers) {
        Console.WriteLine($"= {pair.Key} (skipped)");
      }
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public ResolvedMethod[] GetMethod(string name) {
      List<ResolvedMethod> result = new List<ResolvedMethod>();

      var candidates = this._resolvedMethods.FindAll(pair => pair.Key == name);

      foreach (var pair in candidates) {
        result.Add(pair.Value);
      }

      return result.ToArray();
    }
  }
}
