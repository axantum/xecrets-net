﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AxCrypt Mono Specific Library")]
[assembly: AssemblyCompany("Axantum Software AB")]
[assembly: AssemblyDescription("Beta")]
[assembly: AssemblyProduct("AxCrypt")]
[assembly: AssemblyCopyright("Copyright © 2012 Svante Seleborg")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("086325b4-e6aa-43b6-89da-3651d1419f6b")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

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
#if !AXANTUM
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyInformationalVersion("2.0.0.0")]
[assembly: AssemblyConfiguration("GPL")]
[assembly: InternalsVisibleTo("Axantum.AxCrypt.Mono.Test")]
#else
[assembly: InternalsVisibleTo("Axantum.AxCrypt.Mono.Test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f7cdec4989133e4654fa9741b22177f2404b463d1c821033dc73dfa47a5976e1cc69a8d78f4dd551bbf710e54300d7f035636a7502c1f88e0929596c848308e3250f927437f358d053d972744691c79ee6e4d3b151e63f56a331446a3097bf13e21f1feba2b84add6a05ebf2b3d9ca600d5ebf33d9c0ec3ae49956a9f3db3fc8")]
#endif

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Axantum.AxCrypt.Mono", Justification = "There are more types, but they are internal.")]