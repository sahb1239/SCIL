using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Test
{
    public class RefParametersTest
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

            await Helper.AnalyzeTestProgram("RefParameters1", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Currently not supported")]
        public async Task Test2()
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

            await Helper.AnalyzeTestProgram("RefParameters2", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
