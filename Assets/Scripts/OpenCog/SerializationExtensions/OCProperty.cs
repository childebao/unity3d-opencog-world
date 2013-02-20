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
public class OCProperty
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// The property field instance.
	/// </summary>
	private System.Object m_Instance;

	/// <summary>
	/// The property field's info.
	/// </summary>
	private PropertyInfo m_Info;

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
			return ObjectNames.NicifyVariableName(m_Info.Name);
		}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public OCProperty(System.Object instance, PropertyInfo info, SerializedPropertyType type)
	{ 
 
		m_Instance = instance;
		m_Info = info;
		m_Type = type;
 
		m_Getter = m_Info.GetGetMethod();
		m_Setter = m_Info.GetSetMethod();
	}
 
	public System.Object GetValue()
	{
		return m_Getter.Invoke(m_Instance, null);
	}
 
	public void SetValue(System.Object value)
	{
		m_Setter.Invoke(m_Instance, new System.Object[] { value });
	}
 
	public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType)
	{
 
		propertyType = SerializedPropertyType.Generic;
 
		Type type = info.PropertyType;
 
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



