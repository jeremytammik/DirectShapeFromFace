using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "DirectShapeFromFace" )]
[assembly: AssemblyDescription( "Revit Add-In Description for DirectShapeFromFace" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Autodesk Inc." )]
[assembly: AssemblyProduct( "DirectShapeFromFace Revit Add-In" )]
[assembly: AssemblyCopyright( "Copyright 2015-2018 © Jeremy Tammik Autodesk Inc." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "321044f7-b0b2-4b1c-af18-e71a19252be0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//
// 2015-08-31 2016.0.0.0 initial commit using offset works for some columns
// 2015-09-01 2016.0.0.1 display original geometry location using model lines and existing sketch planes
// 2015-09-01 2016.0.0.2 started exploring geometry instance transformations
// 2015-09-01 2016.0.0.2 implemented GetTransformStackForObject to no avail
// 2015-09-01 2016.0.0.2 applied FamilyInstance.GetTransform
// 2015-09-02 2016.0.0.3 applied FamilyInstance.GetTotalTransform
// 2015-09-02 2016.0.0.4 reimplemented GetTransformStackForObject using stable_representation
// 2015-09-02 2016.0.0.5 use GetSymbolGeometry instead of GetInstanceGeometry to compare stable_representation
// 2015-09-03 2016.0.0.6 tested sketch plane reuse; it never happens, because the name always remains '<not associated>'
// 2015-09-03 2016.0.0.7 reuse of '<not associated>' sketch plane works fine
// 2015-09-04 2016.0.0.8 added better check for face or edge searching geometric target element
// 2015-09-08 2016.0.0.9 added debug log of sketch plane counter and removed commented code
// 2015-09-10 2016.0.0.10 merged alex' pull request #1
// 2015-09-10 2016.0.0.11 integrated alex' simple shape builder pull request #1
// 2015-09-10 2016.0.0.12 encapsulate transaction in 'using' statement
// 2015-09-10 2016.0.0.13 further simplification, transform entire mesh instead of individual vertex
// 2018-01-09 2018.0.0.0 flat migration to Revit 2018
//
[assembly: AssemblyVersion( "2018.0.0.0" )]
[assembly: AssemblyFileVersion( "2018.0.0.0" )]
