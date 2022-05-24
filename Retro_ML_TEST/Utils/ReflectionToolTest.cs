using NUnit.Framework;
using Retro_ML.Utils;

namespace Retro_ML_TEST.Utils
{
    [TestFixture]
    internal class ReflectionToolTest
    {
        private string? testField1;
        private int? testField2;
        private bool[,]? testField3;

        [Test]
        public void GetFieldTest()
        {
            testField1 = "hello";
            testField2 = 37;
            testField3 = new bool[,] {
                { true, true, false },
                { true, false, true },
                { false, true, true },
            };


            Assert.AreEqual(testField1, ReflectionTool.GetField<string>(this, nameof(testField1)));
            Assert.AreEqual(testField2, ReflectionTool.GetField<int>(this, nameof(testField2)));
            Assert.AreEqual(testField3, ReflectionTool.GetField<bool[,]>(this, nameof(testField3)));
        }
    }
}
