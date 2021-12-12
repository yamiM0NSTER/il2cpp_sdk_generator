using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class ApplyAfter : Attribute
  {
    string _ruleName;

    public ApplyAfter(string ruleName)
    {
      this._ruleName = ruleName;
    }

    public virtual string RuleName
    {
      get => this._ruleName;
    }
  }
}
