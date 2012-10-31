using System.Collections;
using System;

namespace Embodiment
{
    public class ActionParamTypeCode
    {

        public const int BOOLEAN_CODE = 0;
        public const int INT_CODE = 1;
        public const int FLOAT_CODE = 2;
        public const int STRING_CODE = 3;
        public const int VECTOR_CODE = 4;
        public const int ROTATION_CODE = 5;
        public const int ENTITY_CODE = 6;
        // This must be at the end of the enumeration in order to count the number of action types.
        public const int NUMBER_OF_ACTION_PARAM_TYPES = 7;
    }

    public class ActionParamType
    {

        private static Hashtable nameMap = new Hashtable();
        private static Hashtable codeMap = new Hashtable();

        private static bool initialized = false;

        /**
         * 
         * @param name
         * @return
         */
        private static bool existName(string name)
        {
            return nameMap.ContainsKey(name);
        }

        /**
         * 
         * @param code
         * @return
         */
        private static bool existCode(int code)
        {
            return codeMap.ContainsKey(code);
        }

        /**
         * 
         */
        public static void init()
        {
            if (!initialized)
            {
                initialized = true;

                BOOLEAN();
                INT();
                FLOAT();
                STRING();
                VECTOR();
                ROTATION();
                ENTITY();
            }
        }

        /**
         * 
         */
        public static ActionParamType getFromName(string name)
        {
            init();
            if (!nameMap.ContainsKey(name))
            {
                // error "ActionParamType - Invalid/unknown ActionParam name: %s\n"
				return null;
            }
            return (ActionParamType)nameMap[name];
        }

        /**
         * 
         */
        public static ActionParamType getFromCode(int code)
        {
            init();
            if (!codeMap.ContainsKey(code))
            {
                // error "ActionParamType - Invalid/unknown ActionParam name: %s\n"
            }
            return (ActionParamType)codeMap[code];
        }

        /**
         * 
         * @return
         */
        public static ActionParamType BOOLEAN()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.BOOLEAN_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.BOOLEAN_CODE, "bool");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.BOOLEAN_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType INT()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.INT_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.INT_CODE, "int");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.INT_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType FLOAT()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.FLOAT_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.FLOAT_CODE, "float");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.FLOAT_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType STRING()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.STRING_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.STRING_CODE, "string");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.STRING_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType VECTOR()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.VECTOR_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.VECTOR_CODE, "vector");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.VECTOR_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType ROTATION()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.ROTATION_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.ROTATION_CODE, "rotation");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.ROTATION_CODE);
            }
            return result;
        }

        /**
         * 
         * @return
         */
        public static ActionParamType ENTITY()
        {
            ActionParamType result;

            if (!existCode(ActionParamTypeCode.ENTITY_CODE))
            {
                result = new ActionParamType(ActionParamTypeCode.ENTITY_CODE, "entity");
            }
            else
            {
                result = getFromCode(ActionParamTypeCode.ENTITY_CODE);
            }
            return result;
        }

        /**
         * The action parameter type code
         */
        private int code;

        /**
         * The action parameter type name
         */
        private string name;

        /**
         * Empty constructor (for serialization only)
         */
        public ActionParamType()
        {
        }

        /**
         * Constructor
         * 
         * @param code
         * @param name
         */
        private ActionParamType(int code, string name)
        {

            if (existCode(code))
            {
                // error "ActionParamType - Duplicate action parameter type code
            }
            if (existName(name))
            {
                // error "ActionParamType - Duplicate action parameter type name: %s"
            }

            this.code = code;
            this.name = name;

            nameMap.Add(name, this);
            codeMap.Add(code, this);
        }

        /**
         * 
         * @return
         */
        public string getName()
        {
            return this.name;
        }

        /**
         * 
         * @return
         */
        public int getCode()
        {
            return this.code;
        }

        /**
         * 
         */
        public override bool Equals(Object obj)
        {
            ActionParamType other = (ActionParamType)obj;
            return (this.code == other.getCode());
        }

        public override int GetHashCode()
        {
            return code;
        }
    }
}
