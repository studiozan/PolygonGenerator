using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonGenerator
{
	public class TriangleData
	{
		public void SetPoint( Vector3 point0, Vector3 point1, Vector3 point2)
		{
			Point = new Vector3[ 3];
			Point[ 0] = point0;
			Point[ 1] = point1;
			Point[ 2] = point2;

			var x0 = Point[ 0].x;
			var z0 = Point[ 0].z;
			var x1 = Point[ 1].x;
			var z1 = Point[ 1].z;
			var x2 = Point[ 2].x;
			var z2 = Point[ 2].z;
			var a = x1 * x1 - x0 * x0 + z1 * z1 - z0 * z0;
			var b = x2 * x2 - x0 * x0 + z2 * z2 - z0 * z0;
			var c = 2f * (( x1 - x0) * ( z2 - z0) - ( z1 - z0) * ( x2 - x0));

			if( c == 0f)
			{
				c = 1f;
			}

			var x = (( z2 - z0) * a + ( z0 - z1) * b) / c;
			var z = (( x0 - x2) * a + ( x1 - x0) * b) / c;
			
			CirclePoint = new Vector3( x, 0f, z);
			var dist = CirclePoint - Point[ 0];
			CircleRadius = dist.x * dist.x + dist.z * dist.z;
		}

		public Vector3[] Point
		{
			get;
			private set;
		}

		public Vector3 CirclePoint
		{
			get;
			private set;
		}

		public float CircleRadius
		{
			get;
			private set;
		}
		
		public static bool EqualCheck( TriangleData point1, TriangleData point2)
		{
			bool ret = false;

			if( point1.Point[ 1] == point2.Point[ 2] && point1.Point[ 2] == point2.Point[ 1])
			{
				ret = true;
			}

			return ret;
		}
	}
}
