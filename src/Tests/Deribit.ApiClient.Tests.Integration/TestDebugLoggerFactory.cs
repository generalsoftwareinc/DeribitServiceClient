using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deribit.ApiClient.Tests.Integration
{
    internal class TestDebugLoggerFactory
    {
        public ILogger<T> CreateLogger<T>(LogLevel logLevel)
        {
            return new TestDebugLogger<T>(logLevel);
        }
    }
}
