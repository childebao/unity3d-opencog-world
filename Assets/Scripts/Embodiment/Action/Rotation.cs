using UnityEngine;
using System;
using System.Text;
using System.Globalization;
using ProtoBuf;

namespace Embodiment
{
    [ProtoContract]
    public class Rotation
    {

        private float pitch;
        private float roll;
        private float yaw;

        public bool Equals(Rotation other)
        {
        	// Decrease the impact the errors from float
            return Math.Abs(pitch - other.Pitch) < 0.01f && 
                   Math.Abs(roll - other.Roll) < 0.01f && 
                   Math.Abs(yaw - other.Yaw) < 0.01f;
        }

        public override int GetHashCode()
        {
            return (int)(pitch + roll + yaw);
        }

        public override string ToString()
        {
            return "(pitch:"+pitch+", roll:"+roll+", yaw:"+yaw+")";
        }

        // Dummy Constructor
        public Rotation()
        {
            pitch = 0.0f;
            roll = 0.0f;
            yaw = 0.0f;
        }

        // Constructor
        public Rotation(float pitch, float roll, float yaw)
        {
            this.pitch = pitch;
            this.roll = roll;
            this.yaw = yaw;
        }

        public Rotation(Quaternion orientation)
        {
            Vector3 eulerAngle = orientation.eulerAngles;
            this.pitch = (float)Math.PI * (eulerAngle.z / 180);
            this.roll = (float)Math.PI * (eulerAngle.x / 180);
            
            // We make some rotation here for unity3d's default direction is facing towards 
            // Z-axis(equals to the Y-axis in OpenCog), while OpenCog's default 
            // direction is facing towards X-axis.
            this.yaw = (float)Math.PI * (0.5f - eulerAngle.y / 180);
            if (this.yaw > 2 * (float)Math.PI)
			{
                this.yaw -= 2 * (float)Math.PI;
			}
			else if (this.yaw < 0)
			{
				this.yaw += 2 * (float)Math.PI;
			}
        }

        [ProtoMember(1, IsRequired=true)]
        public float Pitch
        {
            get { return this.pitch; }
            set { this.pitch = value; }
        }

        [ProtoMember(2, IsRequired = true)]
        public float Roll
        {
            get { return this.roll; }
            set { this.roll = value; }
        }

        [ProtoMember(3, IsRequired = true)]
        public float Yaw
        {
            get { return this.yaw; }
            set { this.yaw = value; }
        }
    }
}