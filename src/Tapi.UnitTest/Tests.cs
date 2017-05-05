using System;
using System.IO;
using System.Linq;
using System.Text;
using tapi;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            var f = tapi.Script.Compile<Func<int,int>>("return 9*9;",new string[]{"x"}) ;
            Assert.Equal(f(9),81);
        }

      //  [Fact]
        public void InegrationTest()
        {
            Logger.LogLevel = LogLevel.Silent;

            var model = Model.Load("../../../src/tapi.unittest/reports/config.xml");
            var loadData = new LoadData
            {
                Model = model,
                Dir = "../../../sample/data",
                StartDate = model.From,
                EndDate = model.To
            };
            loadData.ExecuteAsync().Wait();

            foreach(var r in model.Reports)
            {
                var report = new CalculateReport();
                var accutal = report.Execute(r,loadData.Events,"");
                
                switch (r.Name)
                {
                    case "minutes_in_column" :
                        var nbrOfColmns = Encoding.UTF8.GetString(accutal.file)
                            .Split(new string[] {Environment.NewLine}, StringSplitOptions.None)
                            .First()
                            .Split('\t')
                            .Count();
                        Assert.Equal(24*60 + 1,  nbrOfColmns);
                        break;
                    default:
                        var expected = File.ReadAllBytes(Path.Combine("../../../src/tapi.unittest/reports/",r.FileName));
                        Assert.Equal(expected,accutal.file);
                        break;
                }
            } 

        }
    }
}