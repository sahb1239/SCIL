using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class HttpBasicAuthNegativeTest
    {
        [Fact]
        public async Task Test1()
        {
            var logs = new List<string>();

            // Nothing should be flagged
            var expected = new List<StringAnalysisResult>();

            await Helper.StringAnalysisOnTestProgram("HttpBasicAuthNegative", logs);
            var actual = Helper.ParseStringAnalysisResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
