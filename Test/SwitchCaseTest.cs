using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class SwitchCaseTest
    {
        [Fact]
        public async Task Test1()
        {
            var logs = new List<string>();

            var expected = new List<Result>
            {
                new Result
                {
                    Source = "System.ConsoleKeyInfo::get_KeyChar()",
                    Sink   = "System.Console::WriteLine(System.Int32)",
                    Type   = "System.Int32"
                }
            };

            await Helper.AnalyzeTestProgram("SwitchCase", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
