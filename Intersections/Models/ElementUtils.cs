using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersections
{
    public class ElementUtils
    {
        public static List<Floor> GetFloors (ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
         
            List<Floor> floors = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .ToList();

            return floors;
        }
        public static List<Wall> GetWalls(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<Wall> walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .ToList();

            return walls;
        }

        public static FamilySymbol GetFamilyInstancesFloor(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FamilySymbol familySymbolFloor = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.FamilyName.Contains("Р1_КЖ_Отверстие_Прямоугольное_Перекрытие"))
                .FirstOrDefault();
            if (familySymbolFloor == null)
            {
                TaskDialog.Show("Ошибка", "Не найдено семейство \"Отверстие в перекрытии\"");
            }

            return familySymbolFloor;
        }
        public static FamilySymbol GetFamilyInstancesWall(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FamilySymbol familySymbolWall = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.FamilyName.Contains("Р1"))
                .FirstOrDefault();
            if (familySymbolWall == null)
            {
                TaskDialog.Show("Ошибка", "Не найдено семейство \"Отверстие в стене\"");
            }

            return familySymbolWall;
        }
        public static List<Element> GetOVDucts(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Document ovDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ОВ")).FirstOrDefault();
            
            List<Element> ovDucts = new FilteredElementCollector(ovDoc)
                .OfClass(typeof(Duct))
                .OfType<Duct>()
                .Select(x => x as Element)
                .ToList();

            return ovDucts;
        }
        public static List<Element> GetOVPipes(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Document ovDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ОВ")).FirstOrDefault();
            
            List<Element> ovPipes = new FilteredElementCollector(ovDoc)
                .OfClass(typeof(Pipe))
                .OfType<Pipe>()
                .Select(x => x as Element)
                .ToList();

            return ovPipes;
        }
        public static List<Element> GetVKPipes(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;          
            Document vkDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ВК")).FirstOrDefault();

            List<Element> vkPipes = new FilteredElementCollector(vkDoc)
                .OfClass(typeof(Pipe))
                .OfType<Pipe>()
                .Select(x => x as Element)
                .ToList();

            return vkPipes;
        }
        public static View3D Get3DView(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            View3D view3D = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .OfType<View3D>()
                .Where(x => !x.IsTemplate)
                .FirstOrDefault();
            if (view3D == null)
            {
                TaskDialog.Show("Ошибка", "Не найден 3D вид");
            }

            return view3D;
        }
    }
}
