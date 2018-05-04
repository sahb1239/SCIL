using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class OutParametersTest
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
                    Sink   = "System.Console::WriteLine(System.Int32)",
                    Type   = "System.Int32"
                }
            };

            await Helper.AnalyzeTestProgram("OutParameters", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
