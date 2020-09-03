using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class BuildingCondition
	{
		[SerializeField]
		public float minAreaSize = 120;
		[SerializeField]
		public float sideRatio = 3;
		[SerializeField]
		public float minAngle = 0;
		[SerializeField]
		public float maxAngle = 360;
		[SerializeField]
		public WeightedRange[] heightRanges = default;
	}
}
