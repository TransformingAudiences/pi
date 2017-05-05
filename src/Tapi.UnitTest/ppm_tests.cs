using System;
using System.IO;
using System.Linq;
using System.Text;
using tapi;
using Xunit;
using OfficeOpenXml;
using System.Collections.Generic;

namespace Tests
{
    public class SharpTests
    {
       [Fact]
        public void InegrationTest()
        {
            Logger.LogLevel = LogLevel.Silent;

            var model = Model.Load("../../../src/tapi.unittest/sharp_ppm_tests/config_sharp.xml");
            var loadData = new LoadData
            {
                Model = model,
                Dir = "../../../src/tapi.unittest/sharp_ppm_tests",
                StartDate = model.From,
                EndDate = model.To
            };
            loadData.ExecuteAsync().Wait();

            using (var package = new ExcelPackage(new FileInfo("../../../src/tapi.unittest/sharp_ppm_tests/Kontrollkörningar TAPI 3 - till Joakim 20170428_2.xlsx")))
            {

                foreach(var r in model.Reports)
                {
                    var report = new CalculateReport();
                    var (accutal,file) = report.Execute(r,loadData.Events,"");
                    
                    var sheet = package.Workbook.Worksheets.Where(x => x.Name == r.Name).FirstOrDefault();
                    Assert.True(sheet != null, $"Can't fin a sheet for {r.Name}");
                    var expected = SheetToDictionary(sheet);

                    foreach(var row in expected.OrderBy(x=>x.Key))
                    {
                        foreach(var col in row.Value.OrderBy(x=>x.Key))
                        {
                            var e = col.Value;
                            
                            Assert.True(accutal.ContainsKey(row.Key), "The Result does not contain the row " + row.Key);
                            Assert.True(accutal[row.Key].ContainsKey(col.Key), $"{r.Name} does not contain the col {col.Key} for row {row.Key}");

                            var a = accutal[row.Key][col.Key]; 

                            Assert.True(Math.Abs(a - e) < 0.001 , $"Error in cell {r.Name}=>{row.Key}=>{col.Key}, Expected: {e}, Actual: {a} ");
                            
                        }
                    }
                    
                }
            } 

        }

        public Dictionary<string,Dictionary<string,double>> SheetToDictionary(ExcelWorksheet sheet)
        {
            var res = new Dictionary<string,Dictionary<string,double>>();
            var row = 6;
            while (!string.IsNullOrEmpty( sheet.Cells[row,1].Value as string))
            {
                var r = new Dictionary<string,double>();
                var n = sheet.Cells[row,1].Value as string;
                res.Add(n,r);
                var col = 2;
                while (!string.IsNullOrEmpty( sheet.Cells[4,col].Value as string))
                {
                    var cn = sheet.Cells[4,col].Value as string;
                    var v = (double)sheet.Cells[row,col].Value ;
                    r.Add(cn,v);
                    col++;
                }
                row++;
            }

            return res;
        }
    }
}