using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedField : ResolvedObject
    {
        public Int32 declaringTypeDefIndex;
        public Int32 fieldIndex;
        public Il2CppFieldDefinition fieldDef = null;
        public Il2CppType type = null;

        public ResolvedField(Il2CppFieldDefinition fDef, Int32 declaringTypeDefIdx, Int32 fieldIdx)
        {
            fieldDef = fDef;
            type = il2cpp.types[fieldDef.typeIndex];
            Name = MetadataReader.GetString(fieldDef.nameIndex);
            declaringTypeDefIndex = declaringTypeDefIdx;
            fieldIndex = fieldIdx;
        }

        public bool isStatic
        {
            get
            {
                return (type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_STATIC) != 0 && (type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_LITERAL) == 0;
            }
        }

        public bool isConst
        {
            get
            {
                return (type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_STATIC) != 0 && (type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_LITERAL) != 0;
            }
        }

        public string ToCode(Int32 indent)
        {
            string code = "";

            // TODO: nested type simplify names maybe

            string TypeString = MetadataReader.GetTypeString(type);

            code += $"{TypeString} {Name}; // 0x{il2cppReader.GetFieldOffset(declaringTypeDefIndex, fieldIndex):X}\n".Indent(indent);

            return code;
        }

        public string DemangledPrefix()
        {
            return $"f_{AccessString()}{MetadataReader.GetSimpleTypeString(type)}";
        }

        public string AccessString()
        {
            var accessFlag = type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_FIELD_ACCESS_MASK;
            switch (accessFlag)
            {
                case il2cpp_Constants.FIELD_ATTRIBUTE_PRIVATE:
                    return "Private";
                case il2cpp_Constants.FIELD_ATTRIBUTE_PUBLIC:
                    return "Public";
                case il2cpp_Constants.FIELD_ATTRIBUTE_FAMILY:
                    return "Protected";
                case il2cpp_Constants.FIELD_ATTRIBUTE_ASSEMBLY:
                case il2cpp_Constants.FIELD_ATTRIBUTE_FAM_AND_ASSEM:
                    return "Internal";
                case il2cpp_Constants.FIELD_ATTRIBUTE_FAM_OR_ASSEM:
                    return "ProtectedInternal";
            }
            return "";
        }
    }
}
