using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Building.Touch_Plugin10;

namespace Intersections
{
    public class HoleCreation
    {
        public static void CreateHole(Document doc, FamilySymbol familySymbolWall, FamilySymbol familySymbolFloor, ReferenceIntersector referenceIntersector, Element el)
        {
            Document elDoc = el.Document;
            Document ovDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ОВ")).FirstOrDefault();
            Document vkDoc = doc.Application.Documents.OfType<Document>().Where(x => x.Title.Contains("ВК")).FirstOrDefault();

            Pipe pipe = el as Pipe;
            Duct duct = el as Duct;

            Line curve = pipe == null ? (duct.Location as LocationCurve).Curve as Line : (pipe.Location as LocationCurve).Curve as Line;
            XYZ point = curve.GetEndPoint(0);
            XYZ direction = curve.Direction;

            List<ReferenceWithContext> intersections = referenceIntersector.Find(point, direction)
                .Where(x => x.Proximity <= curve.Length)
                .Distinct(new ReferenceWithContextElementEqualityComparer())
                .ToList();

            foreach (ReferenceWithContext refer in intersections)
            {
                double proximity = refer.Proximity;
                Reference reference = refer.GetReference();
                XYZ halfDuct = new XYZ();
                Element host = doc.GetElement(reference.ElementId);
                string family = el.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
                if (host is Wall)
                {
                    Level level = doc.GetElement(host.LevelId) as Level;

                    if (el is Duct)
                    {
                        if (!family.Contains("Round Duct"))
                        {
                            halfDuct = new XYZ(point.X, point.Y, (point.Z - duct.Height / 2 - 50 / 304.8));
                        }
                        else
                        {
                            halfDuct = new XYZ(point.X, point.Y, (point.Z - duct.Diameter / 2 - 50 / 304.8));
                        }
                    }
                    else
                    {
                        halfDuct = new XYZ(point.X, point.Y, (point.Z - pipe.Diameter / 2 - 50 / 304.8));
                    }

                    XYZ pointHole = new XYZ((point + (direction * proximity)).X, (point + (direction * proximity)).Y, halfDuct.Z);

                    FamilyInstance hole = doc.Create.NewFamilyInstance(pointHole, familySymbolWall, host, level, StructuralType.NonStructural);
                    
                    Parameter comment = hole.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);                    
                    if (elDoc.Equals(ovDoc))
                    {
                        comment.Set("ОВ");
                    }  
                    if (elDoc.Equals(vkDoc))
                    {
                        comment.Set("ВК");
                    }
                    
                    Parameter width = hole.LookupParameter("ADSK_Размер_Ширина");
                    Parameter height = hole.LookupParameter("ADSK_Размер_Высота");
                    if (duct != null)
                    {
                        if (!family.Contains("Round Duct"))
                        {
                            width.Set(duct.Width + 100 / 304.8);
                            height.Set(duct.Height + 100 / 304.8);
                        }
                        else
                        {
                            width.Set(duct.Diameter + 100 / 304.8);
                            height.Set(duct.Diameter + 100 / 304.8);
                        }
                    }
                    else if (pipe != null)
                    {
                        width.Set(pipe.Diameter + 100 / 304.8);
                        height.Set(pipe.Diameter + 100 / 304.8);
                    }
                }
                if (host is Floor)
                {
                    Level level = doc.GetElement(host.LevelId) as Level;

                    XYZ pointHole = point + (direction * proximity);

                    FamilyInstance hole = doc.Create.NewFamilyInstance(pointHole, familySymbolFloor, host, level, StructuralType.NonStructural);

                    Parameter comment = hole.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    if (elDoc.Equals(ovDoc))
                    {
                        comment.Set("ОВ");
                    }
                    if (elDoc.Equals(vkDoc))
                    {
                        comment.Set("ВК");
                    }

                    Parameter width = hole.LookupParameter("ADSK_Размер_Ширина");
                    Parameter height = hole.LookupParameter("ADSK_Размер_Высота");
                    if (duct != null)
                    {
                        if (!family.Contains("Round Duct"))
                        {
                            width.Set(duct.Width + 100 / 304.8);
                            height.Set(duct.Height + 100 / 304.8);
                        }
                        else
                        {
                            width.Set(duct.Diameter + 100 / 304.8);
                            height.Set(duct.Diameter + 100 / 304.8);
                        }
                    }
                    else if (pipe != null)
                    {
                        width.Set(pipe.Diameter + 100 / 304.8);
                        height.Set(pipe.Diameter + 100 / 304.8);
                    }
                }
            }
        }
    }
}
