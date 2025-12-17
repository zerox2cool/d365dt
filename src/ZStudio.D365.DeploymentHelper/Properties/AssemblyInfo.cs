using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ZStudio.D365.DeploymentHelper")]
[assembly: AssemblyDescription("Dynamics 365 CE Deployment Helper")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Zero.Studio")]
[assembly: AssemblyProduct("ZStudio.D365.DeploymentHelper")]
[assembly: AssemblyCopyright("Copyright © Zero.Studio 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4f95f890-3fe0-49d1-ba1e-df3fd5ad9fe4")]

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
[assembly: AssemblyVersion("0.0.0.10")]
[assembly: AssemblyFileVersion("0.0.0.10")]

//to use XRM early-bound
[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]