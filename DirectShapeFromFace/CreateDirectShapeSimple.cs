using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DirectShapeFromFace
{
    public class CreateDirectShapeSimple
    {
        public static void Execute(ExternalCommandData commandData)
        {
            Transaction trans = null;

            UIDocument uidoc = commandData.Application
                .ActiveUIDocument;

            Document doc = uidoc.Document;

            try
            {
                Selection choices = uidoc.Selection;

                Reference reference = choices.PickObject(
                    ObjectType.Face);

                Element el = doc.GetElement(
                    reference.ElementId);

                trans = new Transaction(doc, "Create elements");
                trans.Start();

                TessellatedShapeBuilder builder
                    = new TessellatedShapeBuilder();

                builder.OpenConnectedFaceSet(false);

                Face face = el.GetGeometryObjectFromReference(
                    reference) as Face;

                Mesh mesh = face.Triangulate();
                List<XYZ> args = new List<XYZ>(3);

                var familyInstance = el as FamilyInstance;
                var transform = familyInstance != null ? familyInstance.GetTotalTransform() : Transform.Identity;

                for (int i = 0; i < mesh.NumTriangles; i++)
                {
                    MeshTriangle triangle = mesh.get_Triangle(
                        i);

                    XYZ p1 = triangle.get_Vertex(0);
                    XYZ p2 = triangle.get_Vertex(1);
                    XYZ p3 = triangle.get_Vertex(2);

                    p1 = transform.OfPoint(p1);
                    p2 = transform.OfPoint(p2);
                    p3 = transform.OfPoint(p3);


                    args.Clear();
                    args.Add(p1);
                    args.Add(p2);
                    args.Add(p3);
                    TessellatedFace tesseFace
                        = new TessellatedFace(args,
                                              ElementId.InvalidElementId);

                    if (builder.DoesFaceHaveEnoughLoopsAndVertices(
                        tesseFace))
                    {
                        builder.AddFace(tesseFace);
                    }
                }

                builder.CloseConnectedFaceSet();

                TessellatedShapeBuilderResult result
                    = builder.Build(
                        TessellatedShapeBuilderTarget.AnyGeometry,
                        TessellatedShapeBuilderFallback.Mesh,
                        ElementId.InvalidElementId);

                ElementId categoryId = new ElementId(
                    BuiltInCategory.OST_GenericModel);

                DirectShape ds = DirectShape.CreateElement(
                    doc, categoryId,
                    Assembly.GetExecutingAssembly().GetType()
                        .GUID.ToString(), Guid.NewGuid().ToString());

                ds.SetShape(result.GetGeometricalObjects());

                ds.Name = "MyShape";

                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.RollBack();

                Debug.Print(ex.Message);
            }
        }
    }
}
