using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
  class TestClass : RuleBaseClass
  {
    public TestClass() : base()
    {
      this.Name = "1111";
      Console.WriteLine(this.Name);
    }

    void method_Awake()
    {

    }
  }
}
