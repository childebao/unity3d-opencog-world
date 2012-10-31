using System;
using UnityEngine;

namespace Embodiment
{
	public class VectorUtil
	{
		/// <summary>
		/// Normalize an arbitrary angle to the range (-PI, PI) 
		/// </summary>
		/// <param name="angle">
		/// angle to be normalized
		/// </param>
		/// <returns>
		/// the normalized value of the angle
		/// </returns>
		public static double NormalizeAngle(double angle)
		{
			while(angle > Math.PI)
			{
				angle -= (Math.PI*2);
			}
			while(angle < -Math.PI)
			{
				angle += (Math.PI*2);
			}
			return angle;
		}
		
		/// <summary>
		/// Converts any absolute angle value from OAC-based coordinates
		/// to Unity-based ones.
		/// </summary>
		/// <param name="angle">
		/// angle to be converted
		/// </param>
		/// <returns>
		/// the converted absolute angle value
		/// </returns>
		public static double OAC2UnityConv(double angle)
		{
			double result = VectorUtil.NormalizeAngle(angle) + (Math.PI/2);
			return VectorUtil.NormalizeAngle(result);
		}
		
		/// <summary>
		/// Converts any absolute angle value from Unity-based coordinates
		/// to OAC-based ones.
		/// </summary>
		/// <param name="angle">
		/// angle to be converted
		/// </param>
		/// <returns>
		/// the converted absolute angle value
		/// </returns>
		public static double Unity2OACConv(double angle)
		{
			double result = VectorUtil.NormalizeAngle(angle) - (Math.PI/2);
			return VectorUtil.NormalizeAngle(result);
		}

        /// <summary>
        /// Convert a position vector from Unity coordinate to OpenCog coordinate.
        /// </summary>
        public static Vector3 ConvertToOpenCogCoord(Vector3 unityCoord)
        {
            return new Vector3(unityCoord.x, unityCoord.z, unityCoord.y);
        }

        /// <summary>
        /// OpenCog takes the center point of one object as its physical coordinates.
        /// However, sometimes we get the bottom left point vector from unity. In that situation,
        /// we need to calculate its central point manually by its size.
        /// </summary>
		public static Vector3 ConvertToCentralCoord(Vector3 unityCoord, Vector3 size)
		{
            Vector3 ocVec = ConvertToOpenCogCoord(unityCoord);
            return ocVec + 0.5f * size;
		}
		
	}
}

