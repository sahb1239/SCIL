using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class BranchingTest
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

            await Helper.AnalyzeTestProgram("Branching", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
