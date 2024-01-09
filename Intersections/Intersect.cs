using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Intersections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Building
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Touch_Plugin10 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Document ovDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ОВ")).FirstOrDefault();
            Document vkDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ВК")).FirstOrDefault();
            if (ovDoc == null || vkDoc == null)
            {
                TaskDialog.Show("Ошибка", "Не найден ОВ/ВК файл");
                return Result.Cancelled;
            }

            List<Element> elem = new List<Element>();

            elem.AddRange(ElementUtils.GetFloors(commandData));
            elem.AddRange(ElementUtils.GetWalls(commandData));

            List<Element> linkedElements = new List<Element>();
            linkedElements.AddRange(ElementUtils.GetOVDucts(commandData));
            linkedElements.AddRange(ElementUtils.GetOVPipes(commandData));
            linkedElements.AddRange(ElementUtils.GetVKPipes(commandData));

            using (Transaction ts = new Transaction(doc, "Расстановка отверстий"))
            {
                ts.Start();

                if (!ElementUtils.GetFamilyInstancesWall(commandData).IsActive && !ElementUtils.GetFamilyInstancesFloor(commandData).IsActive)
                {
                    ElementUtils.GetFamilyInstancesWall(commandData).Activate();
                    ElementUtils.GetFamilyInstancesFloor(commandData).Activate();
                }
                ts.Commit();
            }

            ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));
            ElementClassFilter floorFilter = new ElementClassFilter(typeof(Floor));

            // Combine the two filters using a LogicalOrFilter
            LogicalOrFilter combinedFilter = new LogicalOrFilter(wallFilter, floorFilter);

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(combinedFilter, FindReferenceTarget.Element, ElementUtils.Get3DView(commandData));

            var dataList = new List<Data>();
            double volumeOriginalCubicMeters = 0;

            string filePath1 = @"C:\Users\vocma\Desktop\Data.json";
            string filePath2 = @"C:\Users\vocma\Desktop\NewData.json";
            string filePath3 = @"C:\Users\vocma\Desktop\CombinedData.json";

            foreach (Element element in elem)
            {
                Parameter volumeParam = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                if (volumeParam != null)
                {
                    double volume = volumeParam.AsDouble();
                    volumeOriginalCubicMeters = UnitUtils.ConvertFromInternalUnits(volume, DisplayUnitType.DUT_CUBIC_METERS);
                }
                dataList.Add(new Data
                {
                    Id = element.Id.ToString(),
                    Volume = volumeOriginalCubicMeters,
                    Category = element.Category.Name,
                    Level = (doc.GetElement(element.LevelId) as Level).Name.ToString(),
                });
            }
            string json1 = JsonConvert.SerializeObject(dataList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath1, json1);

            using (Transaction ts = new Transaction(doc, "Расстановка отверстий"))
            {
                ts.Start();

                foreach (Element el in linkedElements)
                {
                    HoleCreation.CreateHole(doc, ElementUtils.GetFamilyInstancesWall(commandData), ElementUtils.GetFamilyInstancesFloor(commandData), referenceIntersector, el);
                }
                ts.Commit();
            }

            var newDataList = new List<NewData>();
            double volumeNewCubicMeters = 0;

            foreach (Element element in elem)
            {
                Parameter volumeParam = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                if (volumeParam != null)
                {
                    double volume = volumeParam.AsDouble();
                    volumeNewCubicMeters = UnitUtils.ConvertFromInternalUnits(volume, DisplayUnitType.DUT_CUBIC_METERS);
                }

                newDataList.Add(new NewData
                {
                    Id = element.Id.ToString(),
                    NewVolume = volumeNewCubicMeters,
                    Category = element.Category.Name,
                    Level = (doc.GetElement(element.LevelId) as Level).Name.ToString(),
                });
            }
            string json2 = JsonConvert.SerializeObject(newDataList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath2, json2);

            CombinedJson combinedJson = new CombinedJson();
            combinedJson.CreateCombinedJson(json1, json2, filePath1, filePath2, filePath3);

            return Result.Succeeded;
        }                
    }
}
