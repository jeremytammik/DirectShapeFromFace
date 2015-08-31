using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DirectShapeFromFace
{
  class CreateDirectShape
  {
    public static void Execute( 
      ExternalCommandData commandData )
    {
      Transaction trans = null;

      UIDocument uidoc = commandData.Application.ActiveUIDocument;

      Document doc = uidoc.Document;

      try
      {
        Selection choices = uidoc.Selection;

        Reference reference = choices.PickObject( 
          ObjectType.Face );

        Element el = doc.GetElement( 
          reference.ElementId );

        using( trans = new Transaction( doc ) )
        {
          trans.Start( "Create elements" );

          TessellatedShapeBuilder builder 
            = new TessellatedShapeBuilder();

          builder.OpenConnectedFaceSet( false );

          Face face = el.GetGeometryObjectFromReference( 
            reference ) as Face;

          Mesh mesh = face.Triangulate();
          List<XYZ> args = new List<XYZ>( 3 );

          XYZ offset = new XYZ();
          if( el.Location is LocationPoint )
          {
            LocationPoint locationPoint = el.Location 
              as LocationPoint;

            offset = locationPoint.Point;
          }

          /*
          if (el.Location is LocationCurve)
          {
            LocationCurve locationCurve = el.Location as LocationCurve;
            offset = locationCurve.Curve.GetEndPoint(0);
          }
          */

          for( int i = 0; i < mesh.NumTriangles; i++ )
          {
            MeshTriangle triangle = mesh.get_Triangle( i );

            XYZ p1 = triangle.get_Vertex( 0 );
            XYZ p2 = triangle.get_Vertex( 1 );
            XYZ p3 = triangle.get_Vertex( 2 );

            p1 = p1.Add( offset );
            p2 = p2.Add( offset );
            p3 = p3.Add( offset );

            args.Clear();
            args.Add( p1 );
            args.Add( p2 );
            args.Add( p3 );

            TessellatedFace tesseFace 
              = new TessellatedFace( args, 
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

          DirectShape ds = null;
          ds = DirectShape.CreateElement( 
            doc, categoryId, 
            Assembly.GetExecutingAssembly().GetType().GUID.ToString(), 
            Guid.NewGuid().ToString() );

          ds.SetShape( result.GetGeometricalObjects() );

          ds.Name = "MyShape";
          //counter++;

          trans.Commit();
        }
      }
      catch( Exception ex )
      {
        TaskDialog.Show( "Error", ex.Message );
      }
    }
  }
}

