using System.Reflection;
using System.Runtime.InteropServices;
using Rhino.PlugIns;

// Rhino plugin attributes - REQUIRED for Rhino to recognize this as a plugin
[assembly: PlugInDescription(DescriptionType.Organization, "Bosch")]
[assembly: PlugInDescription(DescriptionType.Email, "")]
[assembly: PlugInDescription(DescriptionType.Phone, "")]
[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "Romania")]
[assembly: PlugInDescription(DescriptionType.WebSite, "")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "")]
[assembly: PlugInDescription(DescriptionType.Icon, "")]

// Assembly information
[assembly: AssemblyTitle("Bosch Media Browser")]
[assembly: AssemblyDescription("Media browser plugin for Rhino 8 - Browse and insert Bosch product models")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bosch")]
[assembly: AssemblyProduct("Bosch Media Browser")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// COM visibility
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("B05C43D0-ED1A-4B05-8E12-345678ABCDEF")]
