using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	public class LinePolygonParameter
	{
		public WeightedValue[] WidthCandidates { get; set; }
		public float UvY1 { get; set; } = 0;
		public float UvY2 { get; set; } = 1;
		public float DecalSize { get; set; } = 1;
	}
}
