using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.TestClass
{
    public class TestClass
    {
        private readonly IConfiguration configuration;

        public TestClass(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void TestMethod()
        {
            var dataFromJsonFile = configuration.GetSection("Name").Value;
            Console.WriteLine(dataFromJsonFile);
        }

    }
}
