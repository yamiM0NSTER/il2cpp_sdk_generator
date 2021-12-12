using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class MethodName : Attribute
  {
    string _name;
    public MethodName(string name)
    {
      this._name = name;
    }

    public string Name
    {
      get => this._name;
    }
  }
}
