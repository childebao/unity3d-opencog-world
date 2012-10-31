using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Embodiment
{
    /// <summary>
    /// Since protobuf-net requires a data contract declaration on fields of 
    /// class that are to be serialized, we need to wrap the built-in types of
    /// Unity3D with manual data contract declaration.
    /// </summary>
    [ProtoContract]
    public class Vector3Wrapper
    {
        [ProtoMember(1, IsRequired=true)]
        public float x;
        [ProtoMember(2, IsRequired=true)]
        public float y;
        [ProtoMember(3, IsRequired=true)]
        public float z;

        // Constructor from a Unity3D Vector3
        public Vector3Wrapper(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        // Dummy constructor
        public Vector3Wrapper()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    ///
    /// The coordinate system between OpenCog and Unity are different, one should 
    /// convert the coordinate first before sending its mapinfo to OpenCog.
    /// 
    /// OpenCog coordinate:
    ///            z |
    ///              | _ _ _ _ x
    ///             /   -- length --
    ///           /  /
    ///       y /  width
    ///           /
    /// 
    /// Unity coordinate:
    ///       y |  
    ///         |    / z
    ///         |  /  
    ///         |/_ _ _ _ x 
    ///

    /// A class to wrap infomation of game objects in unity.
    [ProtoContract]
    public class OCObjectMapInfo
    {
        #region Constants
        public static readonly float POSITION_DISTANCE_THRESHOLD = 0.05f;
        public static readonly float ROTATION_DELTA = 0.0001f;
    
        public static readonly float DEFAULT_AVATAR_LENGTH = 1f;
        public static readonly float DEFAULT_AVATAR_WIDTH = 1f;
        public static readonly float DEFAULT_AVATAR_HEIGHT = 3f;

        public enum VISIBLE_STATUS { VISIBLE = 0, INVISIBLE = 1, UNKNOWN = 2 };
        #endregion

        #region Private variables
        private string id;
        private string name;
        private string type;
        
        // Position of object
        private Vector3 position;
        private Vector3Wrapper positionWrapper;

        // Rotation of object
        private Rotation rotation;

        // Velocity of an object, if it is moving.
        private Vector3 velocity;
        public Vector3Wrapper velocityWrapper;
        
        // Size of an object.
        private float length, width, height;
		
		// weight of an object
		private float weight;
		
		// the lastest time start to move position
		public Vector3 startMovePos;
        
        private List<OCProperty> properties = new List<OCProperty>();

        // Set the visibility of an object to visible by default.
        private VISIBLE_STATUS visibility = VISIBLE_STATUS.VISIBLE;
        #endregion

        #region Constructors
        // Construct map info from a game object
        public OCObjectMapInfo(GameObject go)
        {
            // Get id of a game object
            this.id = go.GetInstanceID().ToString();
            // Get name
            this.name = go.name;
            // TODO: By default, we are using object type.
            this.type = EmbodimentXMLTags.ORDINARY_OBJECT_TYPE;

            // Convert from unity coordinate to OAC coordinate.
            this.Position = VectorUtil.ConvertToOpenCogCoord(go.transform.position);
            // Get rotation
            this.Rotation = new Rotation(go.transform.rotation);
            // Calculate the velocity later
            this.Velocity = Vector3.zero;

            // Get size
            if (go.collider != null)
            {
                // Get size information from collider.
                this.width = go.collider.bounds.size.z;
                this.height = go.collider.bounds.size.y;
                this.length = go.collider.bounds.size.x;
            }
            else
            {
				Debug.LogWarning("No collider for gameobject " + go.name + ", assuming a point.");
                // Set default value of the size.
                this.width = 0.1f;
                this.height = 0.1f;
                this.length = 0.1f;
            }

            if (go.tag == "OCA")
            {
                // This is an OC avatar, we will use the brain id instead of unity id.
                OCConnector connector = go.GetComponent<OCConnector>() as OCConnector;
                if (connector != null)
                    this.id = connector.BrainId;
                this.type = EmbodimentXMLTags.PET_OBJECT_TYPE;
                // The altitude of oc avatar is underneath the floor because of the 3D model problem, 
                // correct it by adding half of the avatar height.
                //this.Position = new Vector3(this.Position.x, 
                //                            this.Position.y, 
                //                            this.Position.z + this.height * 0.5f);
                this.width = 0.02f;
                this.length = 0.02f;
            } 
            else if (go.tag == "Player")
            {
                // This is a human player avatar.
                this.type = EmbodimentXMLTags.AVATAR_OBJECT_TYPE;
                this.length = OCObjectMapInfo.DEFAULT_AVATAR_LENGTH;
                this.width = OCObjectMapInfo.DEFAULT_AVATAR_WIDTH;
                this.height = OCObjectMapInfo.DEFAULT_AVATAR_HEIGHT;
            }

            // Get weight
            if (go.rigidbody != null)
            {
                this.weight = go.rigidbody.mass;
            }
            else
            {
                this.weight = 0.0f;
            }

            // Get a property manager instance
            OCPropertyManager manager = go.GetComponent<OCPropertyManager>() as OCPropertyManager;
            if (manager != null)
            {
                // Copy all OC properties from the manager, if any.
                foreach (OCProperty ocp in manager.propertyList)
                {
                    this.AddProperty(ocp.Key, ocp.value, ocp.valueType);
                }
            }

            this.AddProperty("visibility-status", "visible", PropertyType.STRING);
            this.AddProperty("detector", "true", PropertyType.BOOL);
			
			string goName = go.name;
			if (go.name.Contains("(Clone)"))
				goName = go.name.Remove(go.name.IndexOf('('));

			this.AddProperty("class", goName, PropertyType.STRING);
        }
		

        public static OCObjectMapInfo CreateTerrainMapInfo(Chunk chunk, uint x, uint y, uint z, uint blockHeight, BlockType blocktype)
        {
            // Construct a unique block name.
            string blockName = "CHUNK_" + chunk.ArrayX + "_" + chunk.ArrayY + "_" + chunk.ArrayZ +
                               "_BLOCK_" + x + "_" + y + "_" + z;
            OCObjectMapInfo mapinfo = new OCObjectMapInfo();
            mapinfo.Height = blockHeight;
            mapinfo.Width = 1;
            mapinfo.Length = 1;
            mapinfo.Type = EmbodimentXMLTags.STRUCTURE_OBJECT_TYPE;
            mapinfo.Id = blockName;
            mapinfo.Name = blockName;
            Vector3 pos = new Vector3(chunk.ArrayX * chunk.WorldData.ChunkBlockWidth + x, chunk.ArrayZ * chunk.WorldData.ChunkBlockDepth + z, chunk.ArrayY * chunk.WorldData.ChunkBlockHeight + y);
            pos = VectorUtil.ConvertToCentralCoord(pos, mapinfo.Size);
            mapinfo.Velocity = Vector3.zero;
            mapinfo.Position = pos;
            Rotation rot = new Rotation(0, 0, 0);
            mapinfo.Rotation = rot;

            // Add block properties
            mapinfo.AddProperty("class", "block", PropertyType.STRING);
            mapinfo.AddProperty("visibility-status", "visible", PropertyType.STRING);
            mapinfo.AddProperty("detector", "true", PropertyType.BOOL);
			mapinfo.AddProperty(EmbodimentXMLTags.MATERIAL_ATTRIBUTE, blocktype.ToString(), PropertyType.STRING);

            return mapinfo;
        }

        public OCObjectMapInfo()
        {
        }
        #endregion

        #region Handy functions
        public OCProperty CheckPropertyExist(string keyStr)
        {
            foreach (OCProperty ocp in properties)
            {
                if (ocp.key == keyStr)
                    return ocp;
            }
            return null;
        }

        public void AddProperty(string keyStr, string valueStr, PropertyType type)
        {
            // Check if property existing
            OCProperty ocp = CheckPropertyExist(keyStr);
            if (ocp != null)
            {
                properties.Remove(ocp);
            }
            properties.Add(new OCProperty(keyStr, valueStr, type));
        }

        public void RemoveProperty(string keyStr)
        {
            // Check if property existing
            OCProperty ocp = CheckPropertyExist(keyStr);
            if (ocp != null)
            {
                properties.Remove(ocp);
            }
        }

        public void UpdateProperty(string keyStr, string valueStr, PropertyType type)
        {
            AddProperty(keyStr, valueStr, type);
        }
        #endregion

        #region Accessors and Mutators
        [ProtoMember(1)]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [ProtoMember(2)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [ProtoMember(3)]
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        // Will be called implicitly when serialized by protobuf-net.
        // Do not invoke this explicitly.
        [ProtoMember(4)]
        public Vector3Wrapper PositionWrapper
        {
            get { return positionWrapper; }
            set { Position = value.ToVector3(); }
        }

        public Vector3 Position
        {
            get { return position; }
            set {
                position = value;
                positionWrapper = new Vector3Wrapper(position);
            }
        }

        // Will be called implicitly when serialized by protobuf-net.
        // Do not invoke this explicitly.
        [ProtoMember(5)]
        public Vector3Wrapper VelocityWrapper
        {
            get { return velocityWrapper; }
            set { Velocity = value.ToVector3();  }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set {
                velocity = value;
                velocityWrapper = new Vector3Wrapper(velocity);
            }
        }

        [ProtoMember(6)]
        public Rotation Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        [ProtoMember(7)]
        public float Length
        {
            get { return length; }
            set { length = value; }
        }

        [ProtoMember(8)]
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        [ProtoMember(9)]
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        //[ProtoMember(10)]
        public VISIBLE_STATUS Visibility
        {
            get { return this.visibility; }
            set { this.visibility = value; }
        }

        [ProtoMember(10)]
        public List<OCProperty> Properties
        {
            get { return properties; }
            set { properties = value; }
        }
		
		[ProtoMember(11)]
        public float Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public Vector3 Size
        {
            get { return new Vector3((float)this.length, (float)this.width, (float)this.height); }
        }
        #endregion
    }
}
