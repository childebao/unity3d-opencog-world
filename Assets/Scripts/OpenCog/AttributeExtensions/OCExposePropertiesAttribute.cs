
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
using System;
using System.Collections;
using System.Reflection;
using ProtoBuf;
using UnityEditor;
using System.Collections.Generic;
using OpenCog.SerializationExtensions;

namespace OpenCog
{

namespace AttributeExtensions
{

/// <summary>
/// The OpenCog Expose Properties Attribute.  Attributed classes will expose
/// their publicly accessible properties in the custom inspector editor.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[AttributeUsage( AttributeTargets.Class )]
#endregion
public class OCExposePropertiesAttribute : Attribute
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	private static string[] excludedPropertyNames =
	{
		"Use GUILayout"
	,	"Enabled"
	,	"Active"
	, "Tag"
	,	"Name"
	,	"Hide Flags"
	};

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public static void Expose(List<OCPropertyField> properties)
	{
		if(properties == null)
		{
			return;
		}
 
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
 
		EditorGUILayout.BeginVertical(emptyOptions);

		foreach(OCPropertyField field in properties)//
		{
 
			EditorGUILayout.BeginHorizontal(emptyOptions);
 
			switch(field.UnityType)
			{
			case SerializedPropertyType.Integer:
				field.SetValue(EditorGUILayout.IntField(field.PublicName, (int)field.GetValue(), emptyOptions)); 
				break;
 
			case SerializedPropertyType.Float:
				field.SetValue(EditorGUILayout.FloatField(field.PublicName, (float)field.GetValue(), emptyOptions));
				break;
 
			case SerializedPropertyType.Boolean:
				field.SetValue(EditorGUILayout.Toggle(field.PublicName, (bool)field.GetValue(), emptyOptions));
				break;
 
			case SerializedPropertyType.String:
				field.SetValue(EditorGUILayout.TextField(field.PublicName, (String)field.GetValue(), emptyOptions));
				break;
 
			case SerializedPropertyType.Vector2:
				field.SetValue(EditorGUILayout.Vector2Field(field.PublicName, (Vector2)field.GetValue(), emptyOptions));
				break;
 
			case SerializedPropertyType.Vector3:
				field.SetValue(EditorGUILayout.Vector3Field(field.PublicName, (Vector3)field.GetValue(), emptyOptions));
				break;
 
 
 
			case SerializedPropertyType.Enum:
				field.SetValue(EditorGUILayout.EnumPopup(field.PublicName, (Enum)field.GetValue(), emptyOptions));
				break;
 
			default:
 
				break;
 
			}
 
			EditorGUILayout.EndHorizontal();
 
		}
 
		EditorGUILayout.EndVertical();

	}

	public static bool GetProperties(System.Object obj, out List<OCPropertyField> readOnlyFields, out List<OCPropertyField> readAndWriteFields)
	{
		List< OCPropertyField > readOnlyFieldsList = new List<OCPropertyField>();
		List< OCPropertyField > readAndWriteFieldsList = new List<OCPropertyField>();

		if(obj == null)
		{
			readOnlyFields = readOnlyFieldsList;
			readAndWriteFields = readAndWriteFieldsList;
			return false;
		}

		//@TODO: Only Expose Attributed Classes
//		System.Type exposePropertiesAttributeType = typeof(OpenCog.AttributeExtensions.OCExposePropertiesAttribute);
//
//		object[] attributes = exposePropertiesAttributeType != null ? obj.GetType().GetCustomAttributes(exposePropertiesAttributeType, true) : null;
//
//		if(attributes == null || attributes.Length == 0)
//		{
//			readOnlyFields = readOnlyFieldsList.ToArray();
//			readAndWriteFields = readAndWriteFieldsList.ToArray();
//			return false;
//		}

		PropertyInfo[] infos = obj.GetType().GetProperties(OCPropertyField.OCBindingFlags);
		FieldInfo[] fieldInfos = obj.GetType().GetFields(OCPropertyField.OCBindingFlags);

//		Debug.Log("In OCExposePropertiesAttribute.GetProperties(), Property Infos:");

		foreach(PropertyInfo info in infos)
		{
			string nicifiedName = ObjectNames.NicifyVariableName(info.Name);
			bool isExcluded = false;

			foreach(string excludedName in excludedPropertyNames)
			{
				if(nicifiedName == excludedName)
				{
					isExcluded = true;
					break;
				}
			}

			if(isExcluded)
			{
				continue;
			}

//			Debug.Log(nicifiedName);

			if(info.CanRead && !info.CanWrite)
			{
				OCPropertyField field = new OCPropertyField(obj, info);
				readOnlyFieldsList.Add(field);
			}
			else if(info.CanRead && info.CanWrite)
			{
				OCPropertyField field = new OCPropertyField(obj, info);
				readAndWriteFieldsList.Add(field);
			}
		}

		readOnlyFields = readOnlyFieldsList;
		readAndWriteFields = readAndWriteFieldsList;
		return true;

	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

}// class OCExposeProperties

}// namespace AttributeExtensions

}// namespace OpenCog



