
/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using System.Collections;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using OpenCog.SerializationExtensions;
using OpenCog.AttributeExtensions;

namespace OpenCog
{

namespace AutomationExtensions
{

/// <summary>
/// The OpenCog Automated Script Scanner. Scans the project's non-Editor
/// scripts for use with the Automated Editor Builder and to synchronize
/// public properties for missing scripts in auto-generated Unity Editors.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[ExecuteInEditMode]
#endregion
public class OCAutomatedScriptScanner : MonoBehaviour
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// All of the candidate scripts.
	/// </summary>
	private static List<OCScript> m_scripts = new List<OCScript>();

	/// <summary>
	/// Whenever we have scanned for scripts.
	/// </summary>
	private static bool m_initialized = false;

	/// <summary>
	/// Are we setup to repaint on changes to the project window?
	/// </summary>
	private static bool m_willRepaint = false;

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Gets the scripts.
	/// </summary>
	/// <value>
	/// The scripts.
	/// </value>
	public static List<OCScript> Scripts
	{
		get {return m_scripts;}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Initialize the scanned scripts
	/// </summary>
	public static void Initialize()
	{
		if( !m_willRepaint )
		{
			EditorApplication.projectWindowChanged += () => {
				//@TODO: Repaint only in the Editor?
				//Repaint();
			};
			m_willRepaint = true;
		}
		if( !m_initialized )
		{
			m_initialized = true;

			ScanAll();
		}
	}

	/// <summary>
	/// Scan all of the scripts in resources (not editor scripts)
	/// </summary>
	public static void ScanAll()
	{
		//Get all of the scripts
		m_scripts = Resources.FindObjectsOfTypeAll( typeof( MonoScript ) )
			//Make this a collection of MonoScripts
      .Cast<MonoScript>()
			//Make sure that they aren't system scripts
      .Where( c => c.hideFlags == 0 )
			//Make sure that they are compiled and
			//can retrieve a class
      .Where( c => c.GetClass() != null )
			//Create a scanned script for each one
      .Select
			(
				c => new OCScript
				(
					c.GetInstanceID()
				, c
				//The properties need to be all public
				//and all private with [SerializeField] set
				, GetNameToPropertyDictionary(c)
				)
      )
      .ToList();
	}

	/// <summary>
	/// Gets the name to property dictionary.
	/// </summary>
	/// <returns>
	/// The name to property dictionary.
	/// </returns>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public static Dictionary<string, OCProperty>
		GetNameToPropertyDictionary(System.Object obj)
	{
		OCProperty[] readOnlyProperties;
		OCProperty[] readAndWriteProperties;
		bool success
			= OCExposePropertiesAttribute
			. GetProperties
			( obj
			, out readOnlyProperties
			, out readAndWriteProperties
			);

		OCProperty[] allProperties
			= new OCProperty
			[ readOnlyProperties.Length
			+ readAndWriteProperties.Length
			];

		readOnlyProperties.CopyTo(allProperties, 0);
		readAndWriteProperties.CopyTo(allProperties, readOnlyProperties.Length);

		return allProperties.ToDictionary( p => p.Name );
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

}// class OCAutomatedScriptScanner

}// namespace AutomationExtensions

}// namespace OpenCog



