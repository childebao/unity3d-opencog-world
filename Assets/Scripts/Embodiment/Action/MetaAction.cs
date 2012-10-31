using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace Embodiment
{
    [RequireComponent(typeof(Avatar))]
    /**
     * @class
     * 
     * MetaAction contains the original name of an action, together with its parameter list 
     * parsed from xml message.
     */
    public class MetaAction
    {
        /**
         * Parameter list of this action.
         */
        private ArrayList parameters = new ArrayList();
        /**
         * Name of this action.
         */
        private string name;
        private int seq;

        /** 
         * Get attribute value from an xml element.
         */
        private static string GetAttribute(XmlElement element, string atribute)
        {
            return element.GetAttribute(atribute);
        }

        
        private static XmlNodeList GetChildren(XmlElement element, string tag)
        {
            return element.GetElementsByTagName(tag);
        }

        public MetaAction(string name, ArrayList parameters, int sequence)
        {
            this.name = name;
            this.parameters = parameters;
            this.seq = sequence;
        }

        /**
         * Create a meta action instance from xml context.
         */
        public static MetaAction Factory(XmlElement element, bool adjustCoordinate = false)
        {
            string actionName = GetAttribute(element, EmbodimentXMLTags.NAME_ATTRIBUTE);

            int sequence = int.Parse(GetAttribute(element, EmbodimentXMLTags.SEQUENCE_ATTRIBUTE));
            ArrayList paramList = new ArrayList();

            XmlNodeList list = GetChildren(element, EmbodimentXMLTags.PARAMETER_ELEMENT);
            // Extract parameters from the xml element.
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement parameterElement = (XmlElement)list.Item(i);
                ActionParamType parameterType = ActionParamType.getFromName(GetAttribute(parameterElement, EmbodimentXMLTags.TYPE_ATTRIBUTE));

                switch (parameterType.getCode())
                {
                    case ActionParamTypeCode.VECTOR_CODE:
                        XmlElement vectorElement = ((XmlElement)(GetChildren(parameterElement, EmbodimentXMLTags.VECTOR_ELEMENT)).Item(0));
                        float x = float.Parse(GetAttribute(vectorElement, EmbodimentXMLTags.X_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(GetAttribute(vectorElement, EmbodimentXMLTags.Y_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(GetAttribute(vectorElement, EmbodimentXMLTags.Z_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
					
						if (adjustCoordinate)
						{
							x += 0.5f;
							y += 0.5f;
							z += 0.5f;
						}
						
                        // swap z and y
                        paramList.Add(new Vector3(x, z, y)); 
                        break;
                    case ActionParamTypeCode.BOOLEAN_CODE:
                        paramList.Add(Boolean.Parse(GetAttribute(parameterElement, EmbodimentXMLTags.VALUE_ATTRIBUTE)));
                        break;
                    case ActionParamTypeCode.INT_CODE:
                        paramList.Add(int.Parse(GetAttribute(parameterElement, EmbodimentXMLTags.VALUE_ATTRIBUTE)));
                        break;
                    case ActionParamTypeCode.FLOAT_CODE:
                        paramList.Add(float.Parse(GetAttribute(parameterElement, EmbodimentXMLTags.VALUE_ATTRIBUTE)));
                        break;
                    case ActionParamTypeCode.ROTATION_CODE:
                        //!! This is a hacky trick. For currently, we do not use rotation
                        // in rotate method, so just convert it to vector type. What's more,
                        // "RotateTo" needs an angle parameter.

                        // Trick... add an angle...
                        XmlElement rotationElement = ((XmlElement)(GetChildren(parameterElement, EmbodimentXMLTags.ROTATION_ELEMENT)).Item(0));
                        float pitch = float.Parse(GetAttribute(rotationElement, EmbodimentXMLTags.PITCH_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
                        float roll = float.Parse(GetAttribute(rotationElement, EmbodimentXMLTags.ROLL_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
                        float yaw = float.Parse(GetAttribute(rotationElement, EmbodimentXMLTags.YAW_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);

                        Rotation rot = new Rotation(pitch, roll, yaw);
                        Vector3 rot3 = new Vector3(rot.Pitch, rot.Roll, rot.Yaw);

                        paramList.Add(0.0f);
                        paramList.Add(rot3);
                        break;
                    case ActionParamTypeCode.ENTITY_CODE:
                        // This action is supposed to act on certain entity.
                        XmlElement entityElement = ((XmlElement)(GetChildren(parameterElement, EmbodimentXMLTags.ENTITY_ELEMENT)).Item(0));

                        int id = int.Parse(GetAttribute(entityElement, EmbodimentXMLTags.ID_ATTRIBUTE));
                        string type = GetAttribute(entityElement, EmbodimentXMLTags.TYPE_ATTRIBUTE);
                        ActionTarget target = new ActionTarget(id, type);

                        paramList.Add(target);
                        break;
                    default:
                        paramList.Add(GetAttribute(parameterElement, EmbodimentXMLTags.VALUE_ATTRIBUTE));
                        break;
                }
            }
            MetaAction action = new MetaAction(actionName, paramList, sequence); 
            return action;
        }

        public int Sequence
        {
            get { return this.seq; }
            set { this.seq = value; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public ArrayList Parameters
        {
            get { return this.parameters; }
        }
    }
}
