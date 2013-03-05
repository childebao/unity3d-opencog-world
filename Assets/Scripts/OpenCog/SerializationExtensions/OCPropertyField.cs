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
using System.Linq;
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
/// serialization interface and C#'s serialization interface.  For ease of
/// reference, the terms Unity Type and C# Type (aka CS Type) will be used
/// to differentiate which serialization interface we're dealing with.
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
	/// The property' or field's instance.  When we call the get or set method
	/// on this OCPropertyField, we access or mutate the corresponding property
	/// or field on this instance of an object.
	/// </summary>
	private System.Object m_Instance = null;

	/// <summary>
	/// The property or field info.  May be null when we're wrapping a
	/// SerializedProperty for which we don't know the MemberInfo
	/// (e.g., m_Script).
	/// </summary>
	private MemberInfo m_MemberInfo = null;

	/// <summary>
	/// The unity serialized property reference.
	/// </summary>
	private SerializedProperty m_UnityPropertyField = null;

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Gets the serialized Unity Type of the property or field.
	/// </summary>
	/// <value>
	/// The serialized Unity Type of the property or field.  Generic on error.
	/// </value>
	public SerializedPropertyType UnityType
	{
		get
		{
			if(m_UnityPropertyField != null)
			{
				return m_UnityPropertyField.propertyType;
			}
			else
			{
				return UnityTypeFromCSTypeObject(m_Instance);
			}
		}
	}

	/// <summary>
	/// Gets the C# Type of the property or field.
	/// </summary>
	/// <value>
	/// The C# Type of the property or field.  Null on error.
	/// </value>
	public Type CSType
	{
		get
		{
			if(m_MemberInfo != null)
			{
				if(m_MemberInfo.MemberType == MemberTypes.Field)
				{
					return (m_MemberInfo as FieldInfo).FieldType;
				}
				else
				if(m_MemberInfo.MemberType == MemberTypes.Property)
				{
					return (m_MemberInfo as PropertyInfo).PropertyType;
				}
				else
				{
					return null;
				}
			}
			else
			if(m_UnityPropertyField != null)
			{
				return CSTypeFromUnityTypePropertyField(m_UnityPropertyField);
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets the public name without the leading m, underscore, and/or lowercase
	/// character.  Also splits apart multiple words with spaces.
	/// </summary>
	/// <value>
	/// A string representing the public name.  Null on error.
	/// </value>
	public String PublicName
	{
		get
		{
			return ObjectNames.NicifyVariableName(this.PrivateName);
		}
	}

	/// <summary>
	/// Gets the private name with the leading m, underscore, and/or lowercase
	/// character.  Should be identical to the declared name in code.
	/// </summary>
	/// <value>
	/// A string representing the private name.  Null on error.
	/// </value>
	public String PrivateName
	{
		get
		{
			if(m_MemberInfo != null)
			{
				return m_MemberInfo.Name;
			}
			else
			if(m_UnityPropertyField != null)
			{
				return m_UnityPropertyField.name;
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets Unity's version of the serialized property.  Will be null for C#
	/// properties or fields that we've exposed separately.
	/// </summary>
	/// <value>
	/// The serialized unity property field.
	/// </value>
	public SerializedProperty UnityPropertyField
	{
		get
		{
			return m_UnityPropertyField;
		}
	}

	/// <summary>
	/// Gets the property or field member info.  Will be null for serialized
	/// unity properties for which we do not know the MemberInfo
	/// (e.g., m_Script).
	/// </summary>
	/// <value>
	/// The property or field member info.
	/// </value>
	public MemberInfo MemberInfo
	{
		get
		{
			return m_MemberInfo;
		}
	}

	/// <summary>
	/// Gets the standard OpenCog Binding Flags.  Defines the property fields
	/// that we will seek to access or mutate.
	/// </summary>
	/// <value>
	/// The standard binding flags.
	/// </value>
	public static BindingFlags OCBindingFlags
	{
		get
		{
			return
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
		}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="OpenCog.SerializationExtensions.OCPropertyField"/> class.
	/// </summary>
	/// <param name='instance'>
	/// The object instance accessed or mutated by this property field.
	/// </param>
	/// <param name='info'>
	/// The C# Type propery or field info.  Must be
	/// System.Reflection.PropertyInfo or System.Reflection.FieldInfo.
	/// </param>
	public OCPropertyField
	(
		System.Object instance
	, MemberInfo info
	)
	{
		m_Instance = instance;
		m_MemberInfo = info;
		m_UnityPropertyField = null;
	}

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="OpenCog.SerializationExtensions.OCPropertyField"/> class.
	/// </summary>
	/// <param name='instance'>
	/// The object instance accessed or mutated by this property field.
	/// </param>
	/// <param name='unityPropertyField'>
	/// The serialized Unity Type property field.
	/// </param>
	public OCPropertyField
	(
		System.Object instance
	, SerializedProperty unityPropertyField
	)
	{
		m_Instance = instance;
		m_UnityPropertyField = unityPropertyField;

		PropertyInfo propertyInfo =
			m_Instance
		. GetType()
		. GetProperty(unityPropertyField.name, OCBindingFlags);

		FieldInfo fieldInfo =
			m_Instance.GetType()
		. GetField(unityPropertyField.name, OCBindingFlags);

		if(fieldInfo != null)
		{
			m_MemberInfo = fieldInfo;
		}
		else
		if(propertyInfo != null)
		{
			m_MemberInfo = propertyInfo;
		}
	}

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="OpenCog.SerializationExtensions.OCPropertyField"/> class.
	/// </summary>
	/// <param name='propertyField'>
	/// The OpenCog Property field we'd like to copy.
	/// </param>
	public OCPropertyField(OCPropertyField propertyField)
	{
		m_Instance = propertyField.m_Instance;
		m_MemberInfo = propertyField.m_MemberInfo;
		m_UnityPropertyField = propertyField.m_UnityPropertyField;
	}

	/// <summary>
	/// Gets the property' or field's value.
	/// </summary>
	/// <returns>
	/// The property' or field's value.  Null on error.
	/// </returns>
	public System.Object GetValue()
	{
		if(m_MemberInfo != null)
		{
			if(m_MemberInfo.MemberType == MemberTypes.Field)
			{
				return (m_MemberInfo as FieldInfo).GetValue(m_Instance);
			}
			else
			if(m_MemberInfo.MemberType == MemberTypes.Property)
			{
				return (m_MemberInfo as PropertyInfo).GetValue(m_Instance, null);
			}
			else
			{
				return null;
			}
		}
		else
		if(m_UnityPropertyField != null)
		{
			return m_UnityPropertyField.objectReferenceValue;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Sets the property' or field's value.
	/// </summary>
	/// <param name='value'>
	/// The property' or field's value.  Throws on error.
	/// </param>
	public void SetValue(System.Object value)
	{
		if(m_MemberInfo != null)
		{
			if(m_MemberInfo.MemberType == MemberTypes.Field)
			{
				(m_MemberInfo as FieldInfo).SetValue(m_Instance, value);
			}
			else
			if(m_MemberInfo.MemberType == MemberTypes.Property)
			{
				(m_MemberInfo as PropertyInfo).SetValue(m_Instance, value, null);
			}
			else
			{
				//@TODO: Make an OCException class
				throw new Exception("In OCPropertyField.SetValue," +
					"member is neither Field nor Property");
			}
		}
		else
		if(m_UnityPropertyField != null)
		{
			m_UnityPropertyField.objectReferenceValue = (UnityEngine.Object)value;
		}
		else
		{
			//@TODO: Make an OCException class
			throw new Exception("In OCPropertyField.SetValue," +
				"no member info or Unity property field.");
		}
	}

	/// <summary>
	/// Translates from a C# Type object to a serialized Unity Type.
	/// </summary>
	/// <returns>
	/// True if there exists a serialized Unity Type that corresponds to the
	/// C# Type object.
	/// </returns>
	/// <param name='obj'>
	/// The C# Type object.
	/// </param>
	/// <param name='unityType'>
	/// If set to <c>true</c>, the serialized Unity Type that corresponds to the
	/// C# Type.
	/// </param>
	public static bool GetUnityTypeFromCSTypeObject
	(
		object obj
	, out SerializedPropertyType unityType
	)
	{
		if(obj == null)
		{
			unityType = SerializedPropertyType.Generic;
			return false;
		}

		unityType = UnityTypeFromCSTypeObject(obj);

		if(unityType == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Translates from a serialized Unity Type property field to a C# Type.
	/// </summary>
	/// <returns>
	/// True if there exists a C# Type that corresponds to the serialized
	/// Unity Type property field.
	/// </returns>
	/// <param name='unityPropertyField'>
	/// The Unity Type property field.
	/// </param>
	/// <param name='csType'>
	/// If set to <c>true</c>, the C# Type that corresponds to the serialized
	/// Unity Type property field.
	/// </param>
	public static bool GetCSTypeFromUnityTypePropertyField
	(
		SerializedProperty unityPropertyField
	, out Type csType
	)
	{
		if(unityPropertyField == null)
		{
			csType = null;
			return false;
		}

		csType = CSTypeFromUnityTypePropertyField(unityPropertyField);

		if(csType == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Translates from a C# Type object to a serialized Unity Type.
	/// </summary>
	/// <returns>
	/// The serialized Unity Type that corresponds to the C# Type object.
	/// Generic on error.
	/// </returns>
	/// <param name='object'>
	/// The C# Type object.
	/// </param>
	private static SerializedPropertyType UnityTypeFromCSTypeObject
	(
		object obj
	)
	{
		if(obj == null)
		{
			return SerializedPropertyType.Generic;
		}

		Type csType = obj.GetType();

		// classic switch statements are efficient for plain old types
		switch(Type.GetTypeCode(csType))
		{
		case TypeCode.Boolean:
			return SerializedPropertyType.Boolean;

		case TypeCode.Decimal:
		case TypeCode.Double:
		case TypeCode.Single:
			return SerializedPropertyType.Float;

		case TypeCode.Int16:
		case TypeCode.Int32:
		case TypeCode.Int64:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.UInt64:
			return SerializedPropertyType.Integer;

		case TypeCode.String:
			return SerializedPropertyType.String;

		case TypeCode.Char:
			return SerializedPropertyType.Character;

		case TypeCode.Object:
			{
				// use our own nested type switch here
				SerializedPropertyType unityType = SerializedPropertyType.Generic;

				OCTypeSwitch.Do
				(
					obj
				, OCTypeSwitch.Case<AnimationCurve>
					(
						() => unityType = SerializedPropertyType.AnimationCurve
					)
				, OCTypeSwitch.Case<Bounds>
					(
						() => unityType = SerializedPropertyType.Bounds
					)
				, OCTypeSwitch.Case<Color>
					(
						() => unityType = SerializedPropertyType.Color
					)
				, OCTypeSwitch.Case<Gradient>
					(
						() => unityType = SerializedPropertyType.Gradient
					)
				, OCTypeSwitch.Case<LayerMask>
					(
						() => unityType = SerializedPropertyType.LayerMask
					)
				, OCTypeSwitch.Case<Rect>
					(
						() => unityType = SerializedPropertyType.Rect
					)
				, OCTypeSwitch.Case<Vector2>
					(
						() => unityType = SerializedPropertyType.Vector2
					)
				, OCTypeSwitch.Case<Vector3>
					(
						() => unityType = SerializedPropertyType.Vector3
					)
				, OCTypeSwitch.Case<Enum>
					(
						() =>
				{
					if(csType.IsEnum)
					{
						unityType = SerializedPropertyType.Enum;
					}
				}
					)
				, OCTypeSwitch.Default
					(
						() => unityType = SerializedPropertyType.ObjectReference
					)
				);

				return unityType;
			}

		case TypeCode.Byte:
		case TypeCode.SByte:
		case TypeCode.DateTime:
		case TypeCode.DBNull:
		case TypeCode.Empty:
		default:
			return SerializedPropertyType.Generic;

		}

		return SerializedPropertyType.Generic;
	}

	/// <summary>
	/// Translates from a serialized Unity Type property field to a C# Type.
	/// </summary>
	/// <returns>
	/// The C# Type that corresponds to the serialized Unity Type property field.
	/// </returns>
	/// <param name='unityPropertyField'>
	/// The serialized Unity Type property field.
	/// </param>
	private static Type CSTypeFromUnityTypePropertyField
	(
		SerializedProperty unityPropertyField
	)
	{
		if(unityPropertyField == null)
		{
			return null;
		}

		SerializedPropertyType unityType = unityPropertyField.propertyType;

		switch(unityType)
		{
		case SerializedPropertyType.AnimationCurve:
			return typeof(AnimationCurve);

		case SerializedPropertyType.Boolean:
			return typeof(Boolean);

		case SerializedPropertyType.Bounds:
			return typeof(Bounds);

		case SerializedPropertyType.Character:
			return typeof(Char);

		case SerializedPropertyType.Color:
			return typeof(Color);

		case SerializedPropertyType.Enum:
			return typeof(Enum);

		case SerializedPropertyType.Float:
			return typeof(float);

		case SerializedPropertyType.Gradient:
			return typeof(Gradient);

		case SerializedPropertyType.Integer:
			return typeof(int);

		case SerializedPropertyType.LayerMask:
			return typeof(LayerMask);

		case SerializedPropertyType.ObjectReference:
			return typeof(UnityEngine.Object);

		case SerializedPropertyType.Rect:
			return typeof(Rect);

		case SerializedPropertyType.String:
			return typeof(String);

		case SerializedPropertyType.Vector2:
			return typeof(Vector2);

		case SerializedPropertyType.Vector3:
			return typeof(Vector3);

		default:
			return null;
		}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

}// class OCPropertyField

}// namespace SerializationExtensions

}// namespace OpenCog



