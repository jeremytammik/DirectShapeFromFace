#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
      var dialog = new TaskDialog("Create direct shape")
          {
              MainInstruction = "Select the way, you want to create shape"
          };
      dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Initial shape builder");
      dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Jeremy's shape builder");
      switch (dialog.Show())
      {
          case TaskDialogResult.CommandLink1:
              CreateDirectShapeInitial.Execute1(commandData);
              break;
          case TaskDialogResult.CommandLink2:
              CreateDirectShape.Execute(commandData);
              break;
      }

      return Result.Succeeded;
    }
  }
}
