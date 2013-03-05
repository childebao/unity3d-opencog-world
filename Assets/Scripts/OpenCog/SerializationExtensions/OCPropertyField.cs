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
using System.Reflection;
using UnityEditor;
using System;

namespace OpenCog
{

namespace SerializationExtensions
{

/// <summary>
/// The OpenCog Property Field.  Provides meta-data utility for storing and
/// retrieving arbitrarily typed properties or fields.  Used to expose
/// MonoBehavior script properties or fields (public or serialized) and to
/// synchronize properties for missing MonoBehavior script references in
/// auto-generated Unity Editors.  Basically an adapter between Unity's
/// serialization interface and C#'s serialization interface.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
#endregion
public class OCPropertyField
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Are we wrapping a property or a field?
	/// </summary>
	private bool m_IsProperty;

	/// <summary>
	/// The property field instance.
	/// </summary>
	private System.Object m_Instance;

	/// <summary>
	/// The property info (when we're wrapping a property).
	/// </summary>
	private PropertyInfo m_PropertyInfo;

	/// <summary>
	/// The field info (when we're wrapping a field).
	/// </summary>
	private FieldInfo m_FieldInfo;

	/// <summary>
	/// The property field's serialized type.
	/// </summary>
	private SerializedPropertyType m_Type;

	/// <summary>
	/// The property field's accessor info.
	/// </summary>
	private MethodInfo m_Getter;

	/// <summary>
	/// The property field's mutator info.
	/// </summary>
	private MethodInfo m_Setter;

	/// <summary>
	/// The unity serialized property reference.
	/// </summary>
	private SerializedProperty m_SerializedPropertyReference = null;

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Gets the type.
	/// </summary>
	/// <value>
	/// The type.
	/// </value>
	public SerializedPropertyType Type
	{
		get
		{
			return m_Type;
		}
	}

	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public String Name
	{
		get
		{
			if(m_IsProperty)
				return ObjectNames.NicifyVariableName(m_PropertyInfo.Name);
			else if(m_FieldInfo != null)
				return ObjectNames.NicifyVariableName(m_FieldInfo.Name);
			else
				return ObjectNames.NicifyVariableName(m_SerializedPropertyReference.name);
		}
	}

	public String UnNicifiedName
	{
		get
		{
			if(m_IsProperty)
				return m_PropertyInfo.Name;
			else if(m_FieldInfo != null)
				return m_FieldInfo.Name;
			else
				return m_SerializedPropertyReference.name;
		}
	}

	/// <summary>
	/// Gets the serialized property reference.
	/// </summary>
	/// <value>
	/// The serialized property reference.
	/// </value>
	public SerializedProperty SerializedPropertyReference
	{
		get
		{
			return m_SerializedPropertyReference;
		}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public OCPropertyField(System.Object instance, PropertyInfo info, SerializedPropertyType type)
	{ 
		m_IsProperty = true;
		m_Instance = instance;
		m_PropertyInfo = info;
		m_Type = type;
 
		if(m_PropertyInfo != null)
		{
			m_Getter = m_PropertyInfo.GetGetMethod();
			m_Setter = m_PropertyInfo.GetSetMethod();
		}
		else
		{
			m_Getter = null;
			m_Setter = null;
		}
	}

	public OCPropertyField(OCPropertyField propertyField)
	{
		m_IsProperty = propertyField.m_IsProperty;
		m_Instance = propertyField.m_Instance;
		m_PropertyInfo = propertyField.m_PropertyInfo;
		m_Type = propertyField.m_Type;

		if(m_PropertyInfo != null)
		{
			m_Getter = m_PropertyInfo.GetGetMethod();
			m_Setter = m_PropertyInfo.GetSetMethod();
		}
		else
		{
			m_Getter = null;
			m_Setter = null;
		}
	}

	public OCPropertyField(System.Object instance, SerializedProperty serializedProperty)
	{
		m_Instance = instance;

		m_PropertyInfo = m_Instance.GetType().GetProperty(serializedProperty.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		m_FieldInfo = m_Instance.GetType().GetField(serializedProperty.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		FieldInfo[] fieldInfos = m_Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

//		if(m_FieldInfo != null)
//			Debug.Log("Field Info: " +  m_FieldInfo.ToString());
//		else if(m_PropertyInfo != null)
//			Debug.Log("Property Info: " + m_PropertyInfo.ToString());
//		else
//		{
//			Debug.Log("No Field Info: " + serializedProperty.name + ", " + m_Instance.ToString());
//			foreach( FieldInfo info in fieldInfos)
//			{
//				Debug.Log( info.ToString() );
//			}
//		}

		m_Type = serializedProperty.propertyType;

		m_SerializedPropertyReference = serializedProperty;

		if(m_PropertyInfo != null)
		{
			m_IsProperty = true;
			m_Getter = m_PropertyInfo.GetGetMethod();
			m_Setter = m_PropertyInfo.GetSetMethod();
		}
		else
		{
			if(m_FieldInfo != null) m_IsProperty = false;
			m_Getter = null;
			m_Setter = null;
		}
	}
 
	public System.Object GetValue()
	{
		if(m_Getter != null)
			return m_Getter.Invoke(m_Instance, null);
		else if(m_FieldInfo != null)
		{
			return m_FieldInfo.GetValue(m_Instance);
		}
		else if (m_SerializedPropertyReference != null)
		{
			return m_SerializedPropertyReference.objectReferenceValue;
		}
		else return null;
	}
 
	public void SetValue(System.Object value)
	{
		if(m_Setter != null)
			m_Setter.Invoke(m_Instance, new System.Object[] { value });
		else if(m_FieldInfo != null)
			m_FieldInfo.SetValue(m_Instance, value);
		else if(m_SerializedPropertyReference != null)
			m_SerializedPropertyReference.objectReferenceValue = (UnityEngine.Object)value;
	}

	public bool IsNull()
	{
		return m_Instance == null;
	}
 
	public static bool GetPropertyType(MemberInfo info, out SerializedPropertyType propertyType)
	{
 
		propertyType = SerializedPropertyType.Generic;

		Type type = null;
		if(info.MemberType == MemberTypes.Field)
		{
			type = (info as FieldInfo).FieldType;
		}
		else
		{
			type = (info as PropertyInfo).PropertyType;
		}

//		Debug.Log("In OCPropertyField.GetPropertyType, member type:" + type.ToString());
 
		if(type == typeof(int))
		{
			propertyType = SerializedPropertyType.Integer;
			return true;
		}
 
		if(type == typeof(float))
		{
			propertyType = SerializedPropertyType.Float;
			return true;
		}
 
		if(type == typeof(bool))
		{
			propertyType = SerializedPropertyType.Boolean;
			return true;
		}
 
		if(type == typeof(string))
		{
			propertyType = SerializedPropertyType.String;
			return true;
		} 
 
		if(type == typeof(Vector2))
		{
			propertyType = SerializedPropertyType.Vector2;
			return true;
		}
 
		if(type == typeof(Vector3))
		{
			propertyType = SerializedPropertyType.Vector3;
			return true;
		}
 
		if(type.IsEnum)
		{
			propertyType = SerializedPropertyType.Enum;
			return true;
		}
 
		return false;
 
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

}// class OCPropertyField

}// namespace SerializationExtensions

}// namespace OpenCog



