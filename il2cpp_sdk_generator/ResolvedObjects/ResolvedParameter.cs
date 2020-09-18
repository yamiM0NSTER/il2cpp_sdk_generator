using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedParameter : ResolvedObject
    {
        public Il2CppParameterDefinition paramDef = null;
        public Il2CppType type = null;

        public ResolvedParameter(Il2CppParameterDefinition paramDefinition)
        {
            paramDef = paramDefinition;
            type = il2cpp.types[paramDef.typeIndex];
            Name = MetadataReader.GetString(paramDef.nameIndex);
            //Name = MetadataReader.GetTypeString(type);
        }

        public bool isOut
        {
            get
            {
                return (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) != 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) == 0;
            }
        }

        public bool isIn
        {
            get
            {
                return (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) == 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) != 0;
            }
        }

        public bool isRef
        {
            get
            {
                return (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) != 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) != 0;
            }
        }

        public string ToCode()
        {
            string code = "";

            if (isIn)
                code += "_In_ ";
            else if (isOut)
                code += "_Out_ ";
            else if (isRef)
                code += "_Inout_ ";

            code += $"{MetadataReader.GetTypeString(type)} {Name}";

            return code;
        }
    }
}
