using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ZStudio.D365.DeploymentTool")]
[assembly: AssemblyDescription("Dynamics 365 CE Deployment Tool")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Zero.Studio")]
[assembly: AssemblyProduct("ZStudio.D365.DeploymentTool")]
[assembly: AssemblyCopyright("Copyright © Zero.Studio 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2af36237-ff23-4d4a-b750-4317b40eb595")]

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
[assembly: AssemblyVersion("1.1.0.15")]
[assembly: AssemblyFileVersion("1.1.0.15")]

//to use XRM early-bound
[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]