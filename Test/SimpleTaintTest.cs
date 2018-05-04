using Microsoft.Extensions.DependencyInjection;
using SCIL;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class SimpleTaintTest
    {
        [Fact]
        public async Task Test1()
        {
            var logs = new List<string>();

            var expected = new List<Result>
            {
                new Result
                {
                    Source = "System.Console::ReadLine()",
                    Sink   = "System.Console::WriteLine(System.String)",
                    Type   = "System.String"
                }
            };

            await Helper.AnalyzeTestProgram("SimpleTaint", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}