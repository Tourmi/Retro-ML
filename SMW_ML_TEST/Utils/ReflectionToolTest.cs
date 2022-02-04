using NUnit.Framework;
using SMW_ML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML_TEST.Utils
{
    [TestFixture]
    internal class ReflectionToolTest
    {
        [Test]
        public void GetFieldTest()
        {
            ReflectionTool.GetField<string>("", "");
        }
    }
}
