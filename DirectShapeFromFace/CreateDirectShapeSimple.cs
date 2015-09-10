using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DirectShapeFromFace
{
  class CreateDirectShapeSimple
  {
    public static void Execute(
      ExternalCommandData commandData )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;
      Selection choices = uidoc.Selection;

      try
      {
        Reference reference = choices.PickObject(
          ObjectType.Face );

        Element el = doc.GetElement(
          reference.ElementId );

        Face face = el.GetGeometryObjectFromReference(
          reference ) as Face;

        Mesh mesh = face.Triangulate();

        var familyInstance = el as FamilyInstance;

        if( null != familyInstance )
        {
          var t = familyInstance
            .GetTotalTransform();

          mesh = mesh.get_Transformed( t );
        }

        using( Transaction trans = new Transaction( doc ) )
        {
          trans.Start( "Create DirectShape from Face" );

          TessellatedShapeBuilder builder
            = new TessellatedShapeBuilder();

          builder.OpenConnectedFaceSet( false );

          List<XYZ> args = new List<XYZ>( 3 );

          XYZ[] triangleCorners = new XYZ[3];

          for( int i = 0; i < mesh.NumTriangles; ++i )
          {
            MeshTriangle triangle = mesh.get_Triangle( i );

            triangleCorners[0] = triangle.get_Vertex( 0 );
            triangleCorners[1] = triangle.get_Vertex( 1 );
            triangleCorners[2] = triangle.get_Vertex( 2 );

            TessellatedFace tesseFace
              = new TessellatedFace( triangleCorners,
                ElementId.InvalidElementId );

            if( builder.DoesFaceHaveEnoughLoopsAndVertices(
              tesseFace ) )
            {
              builder.AddFace( tesseFace );
            }
          }

          builder.CloseConnectedFaceSet();

          TessellatedShapeBuilderResult result
            = builder.Build(
              TessellatedShapeBuilderTarget.AnyGeometry,
              TessellatedShapeBuilderFallback.Mesh,
              ElementId.InvalidElementId );

          ElementId categoryId = new ElementId(
            BuiltInCategory.OST_GenericModel );

          DirectShape ds = DirectShape.CreateElement(
            doc, categoryId,
            Assembly.GetExecutingAssembly().GetType()
              .GUID.ToString(), Guid.NewGuid().ToString() );

          ds.SetShape( result.GetGeometricalObjects() );

          ds.Name = "MyShape";

          trans.Commit();
        }
      }
      catch( Exception ex )
      {
        Debug.Print( ex.Message );
      }
    }
  }
}
