
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
	private static List<OCScript> m_Scripts = new List<OCScript>();

	/// <summary>
	/// Whenever we have scanned for scripts.
	/// </summary>
	private static bool m_IsInitialized = false;

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
		get {return m_Scripts;}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public static void Init()
	{
		if( !m_IsInitialized )
		{
			m_IsInitialized = true;

			ScanAll();
		}
	}

	/// <summary>
	/// Scan all of the scripts in resources (not editor scripts)
	/// </summary>
	public static void ScanAll()
	{
		//Get all of the scripts
		m_Scripts = Resources.LoadAll("", typeof( MonoScript ) )
			//Make this a collection of MonoBehaviours
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

//			foreach(OCScript script in m_Scripts)
//			{
//				//if(script.Script.name == "Test")
//				{
//					Debug.Log("Script: " + script.Script.name);
//					foreach(var keyAndValue in script.Properties)
//					{
//						Debug.Log("-----PropertyField: " + keyAndValue.Key + ", " + keyAndValue.Value.PublicName);
//					}
//				}
//			}

	}

	/// <summary>
	/// Gets the name to property dictionary.
	/// </summary>
	/// <returns>
	/// The name to property dictionary.
	/// </returns>
	/// <param name='script'>
	/// Object.
	/// </param>
	public static Dictionary<string, OCPropertyField>
		GetNameToPropertyDictionary(MonoScript script)
	{
		List<OCPropertyField> allPropertiesAndFields = new List<OCPropertyField>();

		System.Type currentType = script.GetClass();

//		Debug.Log("Step 1");

		if(currentType.IsSubclassOf(typeof(MonoBehaviour)))
		{
//			Object[] objects = GameObject.FindObjectsOfTypeIncludingAssets(currentType);

//			Debug.Log("Step 2");

//			if(objects.Length != 0)
			{

//				Debug.Log("Step 3");
				bool success
					= OCPropertyField
					. GetAllPropertiesAndFields
					(
						ref allPropertiesAndFields
					, null
					, currentType
					, null
					)
				;
			}
		}

		return allPropertiesAndFields.ToDictionary( p => p.PublicName );
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



