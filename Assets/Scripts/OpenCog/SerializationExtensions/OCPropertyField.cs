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
/// The OpenCog Property.  Provides meta-data utility for storing and
/// retrieving arbitrarily typed properties.  Used to expose MonoBehavior
/// script properties (public or serialized) and to synchronize properties for
/// missing MonoBehavior script references in auto-generated Unity Editors.
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
			else
				return ObjectNames.NicifyVariableName(m_FieldInfo.Name);
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
 
		m_Getter = m_PropertyInfo.GetGetMethod();
		m_Setter = m_PropertyInfo.GetSetMethod();
	}

	public OCPropertyField(OCPropertyField propertyField)
	{
		m_IsProperty = propertyField.m_IsProperty;
		m_Instance = propertyField.m_Instance;
		m_PropertyInfo = propertyField.m_PropertyInfo;
		m_Type = propertyField.m_Type;

		if(m_PropertyInfo)
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

	public OCPropertyField(SerializedProperty serializedProperty)
	{
		m_Instance = serializedProperty.serializedObject.targetObject;

		m_PropertyInfo = m_Instance.GetType().GetProperty(serializedProperty.name);

		m_FieldInfo = m_Instance.GetType().GetField(serializedProperty.name);

		m_Type = serializedProperty.propertyType;

		m_SerializedPropertyReference = serializedProperty;

		if(m_PropertyInfo)
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
 
	public System.Object GetValue()
	{
		if(m_Getter != null)
			return m_Getter.Invoke(m_Instance, null);
		else return m_SerializedPropertyReference.objectReferenceValue;
	}
 
	public void SetValue(System.Object value)
	{
		if(m_Setter)
			m_Setter.Invoke(m_Instance, new System.Object[] { value });
		else m_SerializedPropertyReference.objectReferenceValue = value;
	}

	public bool IsNull()
	{
		return m_Instance == null;
	}
 
	public static bool GetPropertyType(MemberInfo info, out SerializedPropertyType propertyType)
	{
 
		propertyType = SerializedPropertyType.Generic;
 
		Type type = info.DeclaringType;
 
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



