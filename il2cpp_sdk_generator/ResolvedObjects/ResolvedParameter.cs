using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedParameter : ResolvedObject
    {
        public Int32 parameterIndex;
        public Il2CppParameterDefinition paramDef = null;
        public Il2CppType type = null;

        public ResolvedParameter(Il2CppParameterDefinition paramDefinition, Int32 parameterIdx)
        {
            parameterIndex = parameterIdx;
            paramDef = paramDefinition;
            type = il2cpp.types[paramDef.typeIndex];
            Name = MetadataReader.GetString(paramDef.nameIndex);
            //Name = MetadataReader.GetTypeString(type);
        }

        // TODO: [In] [Out] attributes
        public bool isOut
        {
            get
            {
                return type.byref == 1 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) != 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) == 0;
            }
        }

        public bool isIn
        {
            get
            {
                return type.byref == 1 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) == 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) != 0;
            }
        }

        public bool isRef
        {
            get
            {
                return type.byref == 1 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_OUT) == 0 && (type.attrs & il2cpp_Constants.PARAM_ATTRIBUTE_IN) == 0;
            }
        }

        public bool hasDefaultValue
        {
            get
            {
                if (Metadata.mapParameterDefValues.ContainsKey(parameterIndex))
                    return true;
                return false;
            }
        }

        bool m_isValueType_Checked = false;
        bool m_isValueType;
        public bool isValueType
        {
            get
            {
                if (!m_isValueType_Checked)
                {
                    if (type.type == Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_R4 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_R8 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_U1 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_U2 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_U4 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_U8 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_I1 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_I2 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_I4 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_I8 ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_CHAR ||
                        type.type == Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF)
                    {
                        m_isValueType = true;
                    }
                    else if(type.type == Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST)
                    {
                        Resolvedil2CppGenericClass genericClass = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
                        //Il2CppGenericClass generic_class = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
                        if (Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex] is ResolvedStruct)
                            m_isValueType = true;
                    }
                    else
                    {
                        m_isValueType = false;
                    }
                    m_isValueType_Checked = true;
                }

                return m_isValueType;
            }
        }

        public async Task ToCppCode(StreamWriter sw)
        {
            if (isIn)
                await sw.WriteAsync("_In_ ");
            else if (isOut)
                await sw.WriteAsync("_Out_ ");
            else if (isRef)
                await sw.WriteAsync("_Inout_ ");

            await sw.WriteAsync($"{MetadataReader.GetTypeString(type)}");
            if (isValueType)
            {
                if (isOut)
                    await sw.WriteAsync("*");
                else if (isIn || isRef)
                    await sw.WriteAsync("&");
            }

            await sw.WriteAsync($" {Name}");
        }

        public string ToCppCode()
        {
            string code = "";

            if (isIn)
                code += "_In_ ";
            else if (isOut)
                code += "_Out_ ";
            else if (isRef)
                code += "_Inout_ ";

            code += $"{MetadataReader.GetTypeString(type)}";
            if (isValueType)
            {
                if(isOut)
                    code += "*";
                else if( isIn || isRef)
                    code += "&";
            }
            
            code += $" {Name}";

            return code;
        }

        public string ToHeaderCode()
        {
            string code = "";

            if (isIn)
                code += "_In_ ";
            else if (isOut)
                code += "_Out_ ";
            else if (isRef)
                code += "_Inout_ ";

            code += $"{MetadataReader.GetTypeString(type)}";
            if (isValueType)
            {
                if (isOut)
                    code += "*";
                else if (isIn || isRef)
                    code += "&";
            }

            code += $" {Name}";

            if (hasDefaultValue)
            {
                if (this.Name == "heading")
                {

                }

                if (!MetadataReader.GetDefaultParameterValue(parameterIndex, out var val))
                {
                    //Console.WriteLine("ResolvedParameter::ToCode() has no value @_@");
                }

                

                bool isValueType = type.type == Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE;

                if (type.type == Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST)
                {
                    Resolvedil2CppGenericClass genericClass = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
                    if (Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex] is ResolvedStruct)
                        isValueType = true;
                }

                switch(type.type)
                {
                    case Il2CppTypeEnum.IL2CPP_TYPE_I1:
                    case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                    case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                    case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                    case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                    case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                    case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                    case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                    {
                        if (val == null)
                            code += $" = 0";
                        else
                            code += $" = {val}";
                        break;
                    }
                    case Il2CppTypeEnum.IL2CPP_TYPE_R4:
                    {
                        if (val == null)
                            code += $" = 0.0f";
                        else if (val is float && (float)val == float.PositiveInfinity)
                        {
                            code += $" = std::numeric_limits<float>::infinity()";
                            //code += $" = {val.ToString()}";
                        }
                        else
                            code += $" = (float){(float)val}";
                        break;
                    }
                    case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                    {
                        if (val == null)
                            code += $" = false";
                        else
                            code += $" = {val.ToString().ToLower()}";
                        break;
                    }
                    case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    {
                        ResolvedType resolvedType = Metadata.resolvedTypes[type.data.klassIndex];
                        if(resolvedType is ResolvedEnum)
                        {
                            if (val == null)
                                code += $" = ({MetadataReader.GetTypeString(type)})0";
                            else if (val != null)
                                code += $" = ({MetadataReader.GetTypeString(type)}){val}";
                        }
                        //
                        break;
                    }
                    case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                    {
                        if (val == null)
                            code += $" = il2cpp::make_string(L\"\")";
                        else if (val != null)
                            code += $" = il2cpp::make_string(L\"{val}\")";
                        break;
                    }
                    case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
                    {
                        if (val == null)
                            code += $" = '\0'";
                        else if (val != null)
                            code += $" = '{val}'";
                        break;
                    }
                    default:
                    {
                        if (val == null && !isValueType)
                            code += " = nullptr";
                        else if (val != null)
                            code += $" = ({MetadataReader.GetTypeString(type)}){val}";
                        break;
                    }
                }

                
                
            }
            
            return code;
        }
    }
}
