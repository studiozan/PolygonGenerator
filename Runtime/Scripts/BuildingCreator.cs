using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class BuildingCreator
	{
		public void Initialize(MeshCreator meshCreator, int seed = 0)
		{
			random = new System.Random(seed);
			this.meshCreator = meshCreator;
		}

		public void SetHeightRange(float min, float max)
		{
			minHeight = min;
			maxHeight = max;
		}

		public IEnumerator CreateBuildingMesh(List<SurroundedArea> areas, float areaSize, float sideRatio, float generationRate = 1)
		{
			var parameters = new List<BuildingParameter>();
			var types = new BuildingParameter.BuildingType[]
			{
				BuildingParameter.BuildingType.kBuildingA01,	BuildingParameter.BuildingType.kBuildingA02,
				BuildingParameter.BuildingType.kBuildingA03,	BuildingParameter.BuildingType.kBuildingA04,
				BuildingParameter.BuildingType.kBuildingB01,	BuildingParameter.BuildingType.kBuildingB02,
				BuildingParameter.BuildingType.kBuildingB03,	BuildingParameter.BuildingType.kBuildingB04,
				BuildingParameter.BuildingType.kBuildingC01,	BuildingParameter.BuildingType.kBuildingC02,
				BuildingParameter.BuildingType.kBuildingC03,	BuildingParameter.BuildingType.kBuildingC04,
				BuildingParameter.BuildingType.kBuildingD01,	BuildingParameter.BuildingType.kBuildingD02,
				BuildingParameter.BuildingType.kBuildingD03,	BuildingParameter.BuildingType.kBuildingD04,
				BuildingParameter.BuildingType.kBuildingE01,	BuildingParameter.BuildingType.kBuildingE02,
				BuildingParameter.BuildingType.kBuildingE03,	BuildingParameter.BuildingType.kBuildingE04,
				BuildingParameter.BuildingType.kBuildingF01,	BuildingParameter.BuildingType.kBuildingF02,
				BuildingParameter.BuildingType.kBuildingF03,	BuildingParameter.BuildingType.kBuildingF04,
				BuildingParameter.BuildingType.kBuildingG01,	BuildingParameter.BuildingType.kBuildingG02,
				BuildingParameter.BuildingType.kBuildingG03,	BuildingParameter.BuildingType.kBuildingG04,
				BuildingParameter.BuildingType.kBuildingH01,	BuildingParameter.BuildingType.kBuildingH02,
				BuildingParameter.BuildingType.kBuildingH03,	BuildingParameter.BuildingType.kBuildingH04,
			};

			List<SurroundedArea> buildableAreas = DetectBuildableAreas(areas, areaSize, sideRatio);
			int count = buildableAreas.Count;
			int max = Mathf.RoundToInt((float)count * Mathf.Clamp01(generationRate));

			for (int i0 = 0; i0 < max; ++i0)
			{
				int randomIndex = random.Next(count);
				var param = new BuildingParameter(buildableAreas[randomIndex].AreaPoints);
				param.SetBuildingType(types[random.Next(types.Length)], random.Next(4));
				param.SetBuildingHeight(Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble()));
				parameters.Add(param);

				--count;
				buildableAreas[randomIndex] = buildableAreas[count];
				buildableAreas.RemoveAt(count);
			}

			meshCreator.BuildingPolygonCreate(parameters);

			yield break;
		}

		List<SurroundedArea> DetectBuildableAreas(List<SurroundedArea> areas, float areaSize, float sideRatio)
		{
			var buildableArea = new List<SurroundedArea>();
			for (int i0 = 0; i0 < areas.Count; ++i0)
			{
				var points = new List<Vector3>(areas[i0].AreaPoints);
				if (points.Count == 3)
				{
					points.Add(points[2]);
				}

				if (CanBuildBuilding(points, areaSize, sideRatio) != false)
				{
					buildableArea.Add(new SurroundedArea { AreaPoints = points });
				}
			}

			return buildableArea;
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
