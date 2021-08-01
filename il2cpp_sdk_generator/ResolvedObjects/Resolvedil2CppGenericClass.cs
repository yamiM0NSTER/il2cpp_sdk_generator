using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class Resolvedil2CppGenericClass
    {
        public Il2CppGenericClass genericClass;
        public List<Il2CppType> classParameters = null;
        public List<Il2CppType> methodParameters = null;

        public Resolvedil2CppGenericClass(Il2CppGenericClass genClass)
        {
            genericClass = genClass;

            if(genericClass.context.class_instPtr > 0)
            {
                classParameters = new List<Il2CppType>();

                Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(genericClass.context.class_instPtr);
                ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
                for (int i = 0; i < pointers.Length; i++)
                {
                    classParameters.Add(il2cppReader.GetIl2CppType(pointers[i]));
                }
            }

            if (genericClass.context.method_instPtr > 0)
            {
                methodParameters = new List<Il2CppType>();

                Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(genericClass.context.method_instPtr);
                ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
                for (int i = 0; i < pointers.Length; i++)
                {
                    methodParameters.Add(il2cppReader.GetIl2CppType(pointers[i]));
                }
            }
        }



    }
}
