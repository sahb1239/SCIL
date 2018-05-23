using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class MethodOverloadingWithSinksTest
    {
        [Fact]
        public async Task Test1()
        {
            var logs = new List<string>();

            // Should not trigger any results
            var expected = new List<Result>();

            await Helper.AnalyzeTestProgram("MethodOverloadingWithSinks", logs);
            var actual = Helper.ParseResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
