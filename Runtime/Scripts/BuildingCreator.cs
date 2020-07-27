using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class BuildingCreator
	{
		public void Initialize(MeshCreator meshCreator)
		{
			random = new System.Random();
			this.meshCreator = meshCreator;
		}

		public void SetHeightRange(float min, float max)
		{
			minHeight = min;
			maxHeight = max;
		}

		public IEnumerator CreateBuildingMesh(List<SurroundedArea> areas, float areaSize, float sideRatio)
		{
			var parameters = new List<BuildingParameter>();
			var types = new BuildingParameter.BuildingType[]
			{
				BuildingParameter.BuildingType.kBuildingA,
				BuildingParameter.BuildingType.kBuildingB,
				BuildingParameter.BuildingType.kBuildingC,
			};
			for (int i0 = 0; i0 < areas.Count; ++i0)
			{
				List<Vector3> points = areas[i0].AreaPoints;
				if (points.Count == 3)
				{
					points.Add(points[2]);
				}

				if (CanBuildBuilding(points, areaSize, sideRatio) != false)
				{
					var param = new BuildingParameter(points);
					param.SetBuildingType(types[random.Next(types.Length)], random.Next(4));
					param.SetBuildingHeight(Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble()));
					parameters.Add(param);
				}
			}

			meshCreator.BuildingPolygonCreate(parameters);

			yield break;
		}

		bool CanBuildBuilding(List<Vector3> points, float areaSize, float sideRatio)
		{
			bool canBuild = true;

			Vector3 v1 = points[1] - points[0];
			Vector3 v2 = points[2] - points[1];
			Vector3 v3 = points[3] - points[2];
			Vector3 v4 = points[0] - points[3];

			float s = (Mathf.Abs(Vector3.Cross(v1, v2).y) + Mathf.Abs(Vector3.Cross(v3, v4).y)) * 0.5f;
			if (s < areaSize || (CalcRatio(v1, v3) >= sideRatio && CalcRatio(v2, v4) >= sideRatio))
			{
				canBuild = false;
			}

			return canBuild;
		}

		float CalcRatio(Vector3 v1, Vector3 v2)
		{
			float ratio = Mathf.Infinity;

			float m1 = v1.magnitude;
			float m2 = v2.magnitude;

			if (Mathf.Approximately(m1, 0) == false && Mathf.Approximately(m2, 0) == false)
			{
				ratio = m1 > m2 ? m1 / m2 : m2 / m1;
			}

			return ratio;
		}

		System.Random random;
		MeshCreator meshCreator;
		float minHeight;
		float maxHeight;
	}
}
