using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace DirectShapeFromFace
{
  class CreateDirectShape
  {
    const string _sketch_plane_name_prefix
      = "The Building Coder";

    const string _sketch_plane_name_prefix2
      = "<not associated>";

    #region Geometrical Comparison
    const double _eps = 1.0e-9;

    public static bool IsAlmostZero(
      double a,
      double tolerance )
    {
      return tolerance > Math.Abs( a );
    }

    public static bool IsAlmostZero( double a )
    {
      return IsAlmostZero( a, _eps );
    }

    public static bool IsAlmostEqual( double a, double b )
    {
      return IsAlmostZero( b - a );
    }
    #endregion // Geometrical Comparison

    /// <summary>
    /// Return the normal of a plane  
    /// spanned by the two given vectors.
    /// </summary>
    static XYZ GetNormal( XYZ v1, XYZ v2 )
    {
      return v1
        .CrossProduct( v2 )
        .Normalize();
    }

    /// <summary>
    /// Return the normal of a plane spanned by the
    /// three given triangle corner points.
    /// </summary>
    static XYZ GetNormal( XYZ[] triangleCorners )
    {
      return GetNormal(
        triangleCorners[1] - triangleCorners[0],
        triangleCorners[2] - triangleCorners[0] );
    }

    /// <summary>
    /// Return signed distance from plane to a given point.
    /// </summary>
    public static double SignedDistanceTo(
      Plane plane,
      XYZ p )
    {
      Debug.Assert(
        IsAlmostEqual( plane.Normal.GetLength(), 1 ),
          "expected normalised plane normal" );

      XYZ v = p - plane.Origin;

      return plane.Normal.DotProduct( v );
    }

    /// <summary>
    /// Return true if the sketch plane belongs to us
    /// and its origin and normal vector match the 
    /// given targets.
    /// Nope, we are unable to set the sketch plane 
    /// name. However, Revit throws an exception if 
    /// we try to draw on the skatch plane named
    /// 'Level 1', so lets ensure we use '<not 
    /// associated>'.
    /// </summary>
    static bool SketchPlaneMatches(
      SketchPlane sketchPlane,
      XYZ origin,
      XYZ normal )
    {
      //bool rc = sketchPlane.Name.StartsWith(
      //  _sketch_plane_name_prefix );

      bool rc = sketchPlane.Name.Equals(
        _sketch_plane_name_prefix2 );

      if( rc )
      {
        Plane plane = sketchPlane.GetPlane();

        rc = plane.Normal.IsAlmostEqualTo( normal )
          && IsAlmostZero( SignedDistanceTo(
            plane, origin ) );
      }
      return rc;
    }

    static int _sketch_plane_creation_counter = 0;

    /// <summary>
    /// Return a sketch plane through the given origin
    /// point with the given normal, either by creating
    /// a new one or reusing an existing one.
    /// </summary>
    static SketchPlane GetSketchPlane(
      Document doc,
      XYZ origin,
      XYZ normal )
    {
      string s = "reusing";

      // If we could reliably set the sketch plane Name
      // property or find some other relaible marker 
      // that is reflected in a parameter, we could 
      // replace the sketchPlane.Name.Equals check in
      // SketchPlaneMatches by a parameter filter in
      // the filtered element collector framework
      // to move the test into native Revit code 
      // instead of post-processing in .NET, which
      // would give a 50% performance enhancement.

      SketchPlane sketchPlane
        = new FilteredElementCollector( doc )
          .OfClass( typeof( SketchPlane ) )
          .Cast<SketchPlane>()
          .FirstOrDefault<SketchPlane>( x =>
            SketchPlaneMatches( x, origin, normal ) );

      if( null == sketchPlane )
      {
        Plane plane = new Plane( normal, origin );

        sketchPlane = SketchPlane.Create( doc, plane );

        //sketchPlane.Name = string.Format(
        //  "{0} {1}", _sketch_plane_name_prefix,
        //  _sketch_plane_creation_counter++ );

        ++_sketch_plane_creation_counter;

        s = "created";
      }
      Debug.Print( "GetSketchPlane: {0} '{1}' ({2})",
        s, sketchPlane.Name,
        _sketch_plane_creation_counter );

      return sketchPlane;
    }

    /// <summary>
    /// Create model lines representing a closed 
    /// planar loop in the given sketch plane.
    /// </summary>
    static void DrawModelLineLoop(
      SketchPlane sketchPlane,
      XYZ[] corners )
    {
      Autodesk.Revit.Creation.Document factory
        = sketchPlane.Document.Create;

      int n = corners.GetLength( 0 );

      for( int i = 0; i < n; ++i )
      {
        int j = 0 == i ? n - 1 : i - 1;

        factory.NewModelCurve( Line.CreateBound(
          corners[j], corners[i] ), sketchPlane );
      }
    }

    /// <summary>
    /// Determine the stack of transforms to apply to 
    /// the given target geometry object to bring it 
    /// to the proper location in the project coordinates.
    /// Unfortunetely, we have not found any way at all 
    /// yet to identify the target object we are after.
    /// </summary>
    static bool GetTransformStackForObject(
      Stack<Transform> tstack,
      GeometryElement geo,
      Document doc,
      string stable_representation )
    {
      Debug.Print( "enter GetTransformStackForObject "
        + "with tstack count {0}", tstack.Count );

      bool found = false;

      foreach( GeometryObject obj in geo )
      {
        GeometryInstance gi = obj as GeometryInstance;

        if( null != gi )
        {
          tstack.Push( gi.Transform );

          found = GetTransformStackForObject( tstack,
            gi.GetSymbolGeometry(), doc,
            stable_representation );

          if( found ) { return found; }

          tstack.Pop();

          continue;
        }

        Solid solid = obj as Solid;

        if( null != solid )
        {
          string rep;

          bool isFace = stable_representation.EndsWith(
            "SURFACE" );

          bool isEdge = stable_representation.EndsWith(
            "LINEAR" );

          Debug.Assert( isFace || isEdge,
            "GetTransformStackForObject currently only supports faces and edges" );

          if( isFace && 0 < solid.Faces.Size )
          {
            foreach( Face face in solid.Faces )
            {
              rep = face.Reference
                .ConvertToStableRepresentation( doc );

              if( rep.Equals( stable_representation ) )
              {
                return true;
              }
            }
          }

          if( isEdge && 0 < solid.Edges.Size )
          {
            foreach( Edge edge in solid.Edges )
            {
              rep = edge.Reference
                .ConvertToStableRepresentation( doc );

              if( rep.Equals( stable_representation ) )
              {
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    public static void Execute(
      ExternalCommandData commandData )
    {
      Transaction trans = null;

      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      try
      {
        Selection choices = uidoc.Selection;

        Reference faceref = choices.PickObject(
          ObjectType.Face );

        string rep = faceref
          .ConvertToStableRepresentation( doc );

        Debug.Assert( rep.EndsWith( ":SURFACE" ),
          "expected stable representation to end with SURFACE" );

        Debug.Print( "Face reference picked: "
          + rep );

        Element el = doc.GetElement(
          faceref.ElementId );

        using( trans = new Transaction( doc ) )
        {
          trans.Start( "Create elements" );

          TessellatedShapeBuilder builder
            = new TessellatedShapeBuilder();

          builder.OpenConnectedFaceSet( false );

          // This may return a face in the family 
          // symbol definition with no family instance 
          // transform applied. Use the GeometryElement
          // GetTransformed method to retrieve the face 
          // with the instance transformation applied.

          Face face = el.GetGeometryObjectFromReference(
            faceref ) as Face;

          Debug.Print( "Face reference property: "
            + ( ( null == face.Reference )
              ? "<nil>"
              : face.Reference.ConvertToStableRepresentation( doc ) ) );

          Transform t = null;

          FamilyInstance fi = el as FamilyInstance;

          if( null != fi )
          {
            // Will this handle a face selected
            // in a nested family instance?
            // Some, yes, but not all.

            //t = fi.GetTransform();

            // This also works for some instances
            // but not all.

            //Transform t1 = fi.GetTotalTransform();

            Options opt = new Options();
            opt.ComputeReferences = true;

            GeometryElement geo = el.get_Geometry( opt );

            GeometryElement geo2 = geo.GetTransformed( 
              Transform.Identity );

            Stack<Transform> tstack 
              = new Stack<Transform>();

            if( GetTransformStackForObject( tstack, 
              geo, doc, rep ) && 0 < tstack.Count )
            {
              Debug.Print( "GetTransformStackForObject "
                + "returned true with tstack count {0}",
                tstack.Count );

              t = Transform.Identity;

              while( 0 < tstack.Count )
              {
                t *= tstack.Pop();
              }
            }
          }

          Mesh mesh = face.Triangulate();

          if( null != t )
          {
            mesh = mesh.get_Transformed( t );
          }

          XYZ[] triangleCorners = new XYZ[3];

          for( int i = 0; i < mesh.NumTriangles; i++ )
          {
            MeshTriangle triangle = mesh.get_Triangle( i );

            triangleCorners[0] = triangle.get_Vertex( 0 );
            triangleCorners[1] = triangle.get_Vertex( 1 );
            triangleCorners[2] = triangle.get_Vertex( 2 );

            XYZ normal = GetNormal( triangleCorners );

            SketchPlane sketchPlane = GetSketchPlane(
              doc, triangleCorners[0], normal );

            DrawModelLineLoop( sketchPlane, triangleCorners );

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
            Assembly.GetExecutingAssembly().GetType().GUID.ToString(),
            Guid.NewGuid().ToString() );

          ds.SetShape( result.GetGeometricalObjects() );

          ds.Name = "MyShape";

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
