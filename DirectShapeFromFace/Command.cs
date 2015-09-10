#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace DirectShapeFromFace
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      var dialog = new TaskDialog( "Create DirectShape" )
      {
        MainInstruction = "Select the way you want to create shape"
      };

      dialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink1, "Initial shape builder" );
      dialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink2, "Jeremy's shape builder" );
      dialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink3, "Simple shape builder" );
      
      switch( dialog.Show() )
      {
        case TaskDialogResult.CommandLink1:
          CreateDirectShapeInitial.Execute1( commandData );
          break;
      
        case TaskDialogResult.CommandLink2:
          CreateDirectShape.Execute( commandData );
          break;

        case TaskDialogResult.CommandLink3:
          CreateDirectShapeSimple.Execute( commandData );
          break;
      }
      return Result.Succeeded;
    }
  }
}
