using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator {
  public class ResolvedType : ResolvedObject {
    public ResolvedImage resolvedImage = null;
    protected bool isResolved = false;
    public Int32 typeDefinitionIndex;
    public Il2CppTypeDefinition typeDef = null;
    public string Namespace = null;
    public ResolvedType parentType = null;
    public ResolvedType declaringType = null;

    public List<ResolvedType> nestedTypes = new List<ResolvedType>();

    // Fields
    public List<ResolvedField> instanceFields = new List<ResolvedField>();
    public List<ResolvedField> staticFields = new List<ResolvedField>();
    public List<ResolvedField> constFields = new List<ResolvedField>();
    // Properties
    public List<ResolvedProperty> instanceProperties = new List<ResolvedProperty>();
    public List<ResolvedProperty> staticProperties = new List<ResolvedProperty>();
    // Events
    public List<ResolvedEvent> instanceEvents = new List<ResolvedEvent>();
    public List<ResolvedEvent> staticEvents = new List<ResolvedEvent>();
    // Methods
    public List<ResolvedMethod> instanceMethods = new List<ResolvedMethod>();
    public List<ResolvedMethod> staticMethods = new List<ResolvedMethod>();
    public Dictionary<Int32, ResolvedMethod> slottedMethods = new Dictionary<Int32, ResolvedMethod>();
    public List<ResolvedMethod> miMethods = new List<ResolvedMethod>();

    public bool requiresForwardCode { get; set; } = false;

    public bool isNested {
      get {
        return typeDef.declaringTypeIndex != -1;
      }
    }

    public bool isClass {
      get {
        return this is ResolvedClass;
      }
    }

    public bool isStruct {
      get {
        return this is ResolvedStruct;
      }
    }

    public bool isEnum {
      get {
        return this is ResolvedEnum;
      }
    }

    public bool isInterface {
      get {
        return this is ResolvedInterface;
      }
    }

    public bool isAbstract {
      get {
        return (typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_STATIC) == il2cpp_Constants.TYPE_ATTRIBUTE_ABSTRACT;
      }
    }

    public bool isPublic {
      get {
        return (typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_VISIBILITY_MASK) == il2cpp_Constants.TYPE_ATTRIBUTE_PUBLIC ||
          (typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_VISIBILITY_MASK) == il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_PUBLIC;
      }
    }

    public bool isInternal {
      get {
        var flags = (typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_VISIBILITY_MASK);

        return flags == il2cpp_Constants.TYPE_ATTRIBUTE_NOT_PUBLIC ||
          flags == il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAM_AND_ASSEM ||
          flags == il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_ASSEMBLY;
      }
    }

    public bool isStatic {
      get {
        return (typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_STATIC) == il2cpp_Constants.TYPE_ATTRIBUTE_STATIC;
      }
    }

    public bool hasCctor {
      get {
        return typeDef.hasCctor;
      }
    }

    bool? _isCoroutine = null;

    public bool isCoroutine {
      get {
        if (_isCoroutine != null)
          return _isCoroutine.Value;

        var property = instanceProperties.Find(p => p.Name == "System.Collections.IEnumerator.Current");

        if (property == null) {
          _isCoroutine = false;
          return false;
        }

        var moveNextMethod = instanceMethods.Find(m => m.Name == "MoveNext");

        if (moveNextMethod == null) {
          _isCoroutine = false;
          return false;
        }

        _isCoroutine = true;
        return true;
      }
    }

    bool? _isUniTask = null;

    public bool isUniTask {
      get {
        if (_isUniTask != null)
          return _isUniTask.Value;

        var moveNextMethod = instanceMethods.Find(m => m.Name == "MoveNext");

        if (moveNextMethod == null) {
          _isUniTask = false;
          return false;
        }

        var setStateMachineMethod = instanceMethods.Find(m => m.Name == "SetStateMachine");

        if (setStateMachineMethod == null) {
          _isUniTask = false;
          return false;
        }

        _isUniTask = true;
        return true;
      }
    }

    public ResolvedType() {

    }

    public ResolvedType(Il2CppTypeDefinition type, Int32 idx) {
      typeDef = type;
      typeDefinitionIndex = idx;
    }

    public virtual void Resolve() {
      if (isResolved)
        return;
      Console.WriteLine($"{this.GetType().Name}::Resolve()");
      isResolved = true;
    }

    public virtual async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0) {
      Console.WriteLine($"ResolvedType::ToHeaderCodeSW {this.GetType().Name}");
      await sw.WriteAsync($"{this.GetType().Name}::ToHeaderCode");
    }

    public virtual string ToHeaderCode(Int32 indent = 0) {
      return $"{this.GetType().Name}::ToHeaderCode";
    }

    public virtual string ToHeaderCodeGlobal(Int32 indent = 0) {

      return $"{this.GetType().Name}::ToHeaderCodeGlobal";
    }

    public string CppNamespace() {
      return Namespace.Replace(".", "::");
    }

    public virtual string GetFullName() {
      string str = "";
      if (isNested) {
        str = declaringType.GetFullName();
      }
      else
        str = CppNamespace();

      if (str != "")
        str += "::";
      str += $"{Name}";
      return str;
    }

    internal virtual void ResolveOverrides() {

    }

    public virtual string DemangledPrefix() {
      string prefix = GetVisibility();

      UInt32 staticFlags = typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_STATIC;
      switch (staticFlags) {
        case il2cpp_Constants.TYPE_ATTRIBUTE_STATIC:
          prefix += "Static";
          break;
        case il2cpp_Constants.TYPE_ATTRIBUTE_ABSTRACT:
          prefix += "Abstract";
          break;
        case il2cpp_Constants.TYPE_ATTRIBUTE_SEALED:
          prefix += "Sealed";
          break;
      }

      return prefix;
    }

    public string GetVisibility() {
      UInt32 visFlags = typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_VISIBILITY_MASK;
      switch (visFlags) {
        case il2cpp_Constants.TYPE_ATTRIBUTE_PUBLIC:
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_PUBLIC:
          return "Public";
        case il2cpp_Constants.TYPE_ATTRIBUTE_NOT_PUBLIC:
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAM_AND_ASSEM:
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_ASSEMBLY:
          return "Internal";
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_PRIVATE:
          return "Private";
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAMILY:
          return "Protected";
        case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAM_OR_ASSEM:
          return "ProtectedInternal";
      }
      return "";
    }

    public virtual Dictionary<string, Int32> Demangle(Dictionary<string, Int32> demangledPrefixesParent = null) {
      // Demangle type
      Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();
      if (demangledPrefixesParent != null)
        demangledPrefixes = new Dictionary<string, Int32>(demangledPrefixesParent);
      return demangledPrefixes;
    }

    public bool isDemangled = false;

    public virtual void Demangle(bool force = false) {
      //if (isDemangled && force == false)
      //  return;

      // Demangle Nested types.
      for (int i = 0; i < nestedTypes.Count; i++) {
        nestedTypes[i].Demangle(force);
      }
    }

    public virtual void DemangleNestedTypeNames() {
      Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();

      for (int i = 0; i < nestedTypes.Count; i++) {
        ResolvedType resolvedType = nestedTypes[i];
        resolvedType.DemangleNestedTypeNames();
        if (resolvedType.Name.isCSharpIdentifier()) {
          if (resolvedType.Name.isCppIdentifier())
            continue;

          resolvedType.Name = resolvedType.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedType.DemangledPrefix();
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;

        resolvedType.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
      }
    }

    public virtual async Task ToCppCode(StreamWriter sw, Int32 indent = 0) {
      Console.WriteLine($"ResolvedType::ToCppCodeSW {this.GetType().Name}");
      await sw.WriteAsync($"{this.GetType().Name}::ToCppCode");
    }

    public virtual string ToCppCode(Int32 indent = 0) {
      return $"{this.GetType().Name}::ToCppCode";
    }

    public string NestedName(string name = "") {
      if (declaringType == null)
        return $"{Name}::{name}";

      name = $"{Name}::{name}";
      return declaringType.NestedName(name);
    }

    public virtual string ForwardDeclaration(Int32 indent = 0) {
      return $"{this.GetType().Name}::ForwardDeclaration";
    }

    public virtual string ReturnTypeString(bool nestedCall = false) {
      string str = "";
      if (isNested) {
        str = declaringType.ReturnTypeString(true);
        str += "::";
      }
      else {
        str = CppNamespace();
        if (str != "")
          str += "::";
      }

      str += $"{Name}";
      //if (typeDef.genericContainerIndex >= 0 && !isNested)
      //{
      //    str += "::";
      //    Il2CppGenericContainer generic_container = Metadata.genericContainers[typeDef.genericContainerIndex];

      //    //str += "<";
      //    for (int i = 0; i < generic_container.type_argc; i++)
      //    {
      //        str += $"{{{i}}}";
      //        if (i < generic_container.type_argc - 1)
      //            str += "_";
      //    }
      //    //str += ">";
      //}

      return str;
    }

    public virtual string DeclarationString(bool nestedCall = false) {
      string str = "";
      if (isNested) {
        str = declaringType.DeclarationString(true);
        str += "::";
      }
      str += Name;
      //if (typeDef.genericContainerIndex >= 0 && nestedCall)
      //if (typeDef.genericContainerIndex >= 0 && !isNested && nestedCall)
      //{
      //    Il2CppGenericContainer generic_container = Metadata.genericContainers[typeDef.genericContainerIndex];

      //    str += "<";
      //    for (int i = 0; i < generic_container.type_argc; i++)
      //    {
      //        Il2CppGenericParameter generic_parameter = Metadata.genericParameters[generic_container.genericParameterStart + i];
      //        str += $"{MetadataReader.GetString(generic_parameter.nameIndex)}";
      //        if (i < generic_container.type_argc - 1)
      //            str += ", ";
      //    }
      //    str += ">";
      //}

      return str;
    }
  }
}
