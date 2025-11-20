/*
 * -----------------------------------------------------------------------------
 *  File: AssemblyInfo.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows;
using System.Windows.Markup;

// Tell WPF where to find the generic theme dictionary
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, // where theme-specific resource dictionaries are located
    ResourceDictionaryLocation.SourceAssembly // where the generic resource dictionary is located
)]

// XML namespace mappings
[assembly: XmlnsDefinition("http://uxcore/icons", "UX.Icons")]
[assembly: XmlnsPrefix("http://uxcore/icons", "icons")]

