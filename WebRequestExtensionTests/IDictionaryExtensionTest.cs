using System.Dynamic;
using Xunit;
using WebRequestExtension.Extensions;
using System;
using Newtonsoft.Json;

namespace WebRequestExtensionTests
{
    public class IDictionaryExtensionTest
    {
        [Fact]
        public void ToPostJsonString()
        {
            var foo = new Foo { A = 1, B = "hello" };

            dynamic obj = new ExpandoObject();
            obj.A = foo.A;
            obj.B = foo.B;

            var json = IDictionaryExtension.ToPostString(obj, "application/json");

            Assert.Equal(json, JsonConvert.SerializeObject(foo));
        }

        [Fact]
        public void ToPostFormString()
        {
            var foo = new Foo { A = 1, B = "hello" };

            dynamic obj = new ExpandoObject();
            obj.A = foo.A;
            obj.B = foo.B;

            var formData = IDictionaryExtension.ToPostString(obj, null);

            Assert.Equal(formData, $"A={foo.A}&B={foo.B}");
        }

        class Foo
        {
            public int A { get; set; }

            public string B { get; set; }
        }
    }
}
