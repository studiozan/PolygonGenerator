using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class LinePolygonParameter
	{
		[SerializeField]
		public WeightedValue[] widthCandidates = default;
		[SerializeField]
		public float uvY1 = 0;
		[SerializeField]
		public float uvY2 = 1;
		[SerializeField]
		public float decalSize = 1;
	}
}
