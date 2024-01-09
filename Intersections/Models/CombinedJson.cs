using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersections
{
    public class CombinedJson
    {
        public void CreateCombinedJson (string json1, string json2, string filePath1, string filePath2, string filePath3)
        {
            json1 = File.ReadAllText(filePath1);
            json2 = File.ReadAllText(filePath2);

            List<Data> data1 = JsonConvert.DeserializeObject<List<Data>>(json1);
            List<NewData> data2 = JsonConvert.DeserializeObject<List<NewData>>(json2);

            List<CombinedData> combinedDataList = new List<CombinedData>();

            foreach (var item1 in data1)
            {
                var item2 = data2.FirstOrDefault(d => d.Id == item1.Id && d.Category == item1.Category && d.Level == item1.Level);
                if (item2 != null && (item1.Volume / item2.NewVolume != 1))
                {
                    combinedDataList.Add(new CombinedData
                    {
                        Id = item1.Id,
                        Category = item1.Category,
                        Level = item1.Level,
                        Volume = item1.Volume,
                        NewVolume = item2.NewVolume,
                        Ratio = item1.Volume / item2.NewVolume
                    });
                }
            }
            string combinedJson = JsonConvert.SerializeObject(combinedDataList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath3, combinedJson);
        }
    }
}
