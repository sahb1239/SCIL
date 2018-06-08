using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class HttpBasicAuthSimpleTest
    {
        [Fact]
        public async Task Test1()
        {
            var logs = new List<string>();

            var expected = new List<StringAnalysisResult>
            {
                new StringAnalysisResult
                {
                    Name = "List(st_HttpBasicAuthSimple.Program::Main(System.String[])_0_1)",
                    Charset = "Charset((ht/e:pas@url.dk,ht/e:pas@url.dk))"
                },
                new StringAnalysisResult
                {
                    Name = "List(loc_HttpBasicAuthSimple.Program::Main(System.String[])_0_0)",
                    Charset = "Charset((ht/e:pas@url.dk,ht/e:pas@url.dk))"
                },
                new StringAnalysisResult
                {
                    Name = "List(st_HttpBasicAuthSimple.Program::Main(System.String[])_0_0)",
                    Charset = "Charset((ht/e:pas@url.dk,ht/e:pas@url.dk))"
                }
            };

            await Helper.StringAnalysisOnTestProgram("HttpBasicAuthSimple", logs);
            var actual = Helper.ParseStringAnalysisResults(logs);

            Assert.Equal(expected, actual);
        }
    }
}
