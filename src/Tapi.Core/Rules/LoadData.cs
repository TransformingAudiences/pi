using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tapi
{
    public class LoadData
    {
        public Model Model { get; set; }
        public string Dir { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<Event> Events { get; set; }
        public List<Consumer> Consumers { get; set; }


        public async Task ExecuteAsync()
        {
            await Task.Yield();
            Events = new List<Event>();

            var source = Model.DataSource;

            var consumers = new Dictionary<string, Consumer>();
            var consumed = new List<Consumed>();
            var dayconsumers = new List<DayConsumer>();

            foreach (var date in StartDate.To(EndDate))
            {

                var consumerFile = source.Files.Where(x => x.Type == "Individual").FirstOrDefault();
                if (consumerFile != null)
                {
                    var lines = ParseFile(consumerFile, Dir, date);
                    dayconsumers.AddRange(lines.Select(line =>
                   {
                       var id = line.Where(x => x.Field.VariableType == VariableType.IndividualId).Single().Value as string;
                       var weight = (double)line.Where(x => x.Field.VariableType == VariableType.Weight).Single().Value * 1000.0;
                       return new DayConsumer(id, date, weight, line);
                   }));
                }
            }

            consumers = dayconsumers.GroupBy(x => x.Id).Select(x => DayConsumer.Merge(x)).ToDictionary(x => x.Id);
            Consumers = consumers.Values.ToList();
            foreach (var date in StartDate.To(EndDate))
            {
                foreach(var consumedFile in source.Files.Where(x => x.Type == "Product"))
                {
                    var lines = ParseFile(consumedFile, Dir, date);
                    consumed.AddRange( lines.Select(line =>
                    {
                        var id = (int)line.Where(x => x.Field.VariableType ==  VariableType.ProductId).Single().Value;
                        var startTime =  (TimeSpan)line.Where(x => x.Field.VariableType == VariableType.StartTime).Single().Value;
                        var duration = (int)line.Where(x => x.Field.VariableType == VariableType.Duration).Single().Value;
                        return new Consumed(id,date,startTime,duration,consumedFile.Name, line);
                    }));
                }
            }
            var consumedMap = consumed.GroupBy(x=> new { x.Id,x.Date}).ToDictionary(x=>x.Key, x=> x.OrderBy(y=> y.StartTime).ToList());
            foreach (var date in StartDate.To(EndDate))
            {
                var events = new List<Event>();
                var eventFile = source.Files.Where(x => x.Type == "Events").FirstOrDefault();
                if (eventFile != null)
                {
                    var lines = ParseFile(eventFile, Dir, date);
                    events = lines.Select(line =>
                    {
                        var timestamp = (TimeSpan)line.Where(x => x.Field.VariableType == VariableType.StartTime).Single().Value;
                        var cid = line.Where(x => x.Field.VariableType == VariableType.IndividualId).Single().Value as string;
                        var consumer = consumers[cid];

                        var channel = ((OptionValue)line.Where(x => x.Field.VariableType == VariableType.ProductId).Single().Value).Value;
                        var endTime = (TimeSpan)line.Where(x => x.Field.VariableType == VariableType.EndTime).Single().Value;
                        var channelKey = new { Id = channel, Date = date };
                        var channels = consumedMap.ContainsKey(channelKey) ? consumedMap [channelKey] : new List<Consumed>();
                        var consumeds = MatchConsumed(timestamp,endTime, channels);

                        return new Event(date,timestamp, consumer, consumeds, line);
                    }).ToList();
                }
                Events.AddRange(events);

            }

        }
        private List<Consumed> MatchConsumed(TimeSpan startTime, TimeSpan endTime, List<Consumed> consumedChannel)
        {
            return consumedChannel.Where(x=> TimeHelpers.IsOverlapping(
                    x.Interval,
                    (startTime,endTime)
                )).ToList();
        }

        private IEnumerable<List<VariableValue>> ParseFile(DataFile file, string dir, DateTime date)
        {
            var filePath = System.IO.Path.Combine(dir, file.FileName.Replace("{Date:yyMMdd}", date.ToString("yyMMdd")));
            foreach (var line in System.IO.File.ReadAllLines(filePath))
            {
                var res = new List<VariableValue>();
                foreach (var field in file.Fields)
                {
                    var value = line.Substring(field.Position, field.Length);

                    if (field.ValueType is OptionType oc)
                    {
                        if (!oc.Options.ContainsKey(value))
                        {
                            value = oc.Options.Where(x => x.Value.Index.ToString() == value).Select(x => x.Value.Name).FirstOrDefault() ?? value;
                        }
                    }
                    if (field.ValueType is TimespanType tt)
                    {
                        if (tt.Format == ("HHmmss"))
                        {
                            try
                            {

                                var hours = int.Parse(value.Substring(0, 2));
                                var min = int.Parse(value.Substring(2, 2));
                                var sec = int.Parse(value.Substring(4, 2));
                                var ts = new TimeSpan(hours, min, sec);
                                value = ts.ToString();
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    res.Add(new VariableValue(field, value));
                }
                yield return res;
            }
        }
    }
   
   

}