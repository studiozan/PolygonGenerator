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

		public IEnumerator CreateBuildingMesh(List<SurroundedArea> areas, BuildingCondition condition, float generationRate)
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

			List<SurroundedArea> buildableAreas = DetectBuildableAreas(areas, condition);
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

		List<SurroundedArea> DetectBuildableAreas(List<SurroundedArea> areas, BuildingCondition condition)
		{
			var buildableArea = new List<SurroundedArea>();
			for (int i0 = 0; i0 < areas.Count; ++i0)
			{
				var areaPoints = new List<Vector3>(areas[i0].AreaPoints);
				if (areaPoints.Count == 3)
				{
					areaPoints.Add(areaPoints[2]);
				}

				if (CanBuildBuilding(areaPoints, condition) != false)
				{
					buildableArea.Add(new SurroundedArea { AreaPoints = areaPoints });
				}
			}

			return buildableArea;
		}

		bool CanBuildBuilding(List<Vector3> areaPoints, BuildingCondition condition)
		{
			bool canBuild = true;

			if (IsConvex(areaPoints) == false)
			{
				canBuild = false;
			}
			else
			{
				Vector3 v1 = areaPoints[1] - areaPoints[0];
				Vector3 v2 = areaPoints[2] - areaPoints[1];
				Vector3 v3 = areaPoints[3] - areaPoints[2];
				Vector3 v4 = areaPoints[0] - areaPoints[3];

				float s = (Mathf.Abs(Vector3.Cross(v1, v2).y) + Mathf.Abs(Vector3.Cross(v3, v4).y)) * 0.5f;
				float sideRatio = condition.sideRatio;
				if (s < condition.minAreaSize || (CalcRatio(v1, v3) >= sideRatio && CalcRatio(v2, v4) >= sideRatio))
				{
					canBuild = false;
				}
				else
				{
					for (int i0 = 0; i0 < areaPoints.Count; ++i0)
					{
						Vector3 p0 = areaPoints[i0];
						Vector3 p1 = areaPoints[(i0 + 1) % areaPoints.Count];
						Vector3 p2 = areaPoints[(i0 + areaPoints.Count - 1) % areaPoints.Count];

						Vector3 dir1 = p1 - p0;
						Vector3 dir2 = p2 - p0;

						float angle = -Vector2.SignedAngle(new Vector2(dir1.x, dir1.z), new Vector2(dir2.x, dir2.z));
						angle = angle < 0 ? 360 + angle : angle;
						if (angle < condition.minAngle || angle > condition.maxAngle)
						{
							canBuild = false;
							break;
						}
					}
				}
			}

			return canBuild;
		}

		bool IsConvex(List<Vector3> quadrangle)
		{
			bool isConvex = false;

			for (int i0 = 0; i0 < quadrangle.Count; ++i0)
			{
				Vector3 point = quadrangle[i0];
				var triangle = new List<Vector3>(quadrangle);
				triangle.RemoveAt(i0);
				int sign = 0;
				isConvex = false;
				for (int i1 = 0; i1 < triangle.Count; ++i1)
				{
					Vector3 v = triangle[(i1 + 1) % triangle.Count] - triangle[i1];
					Vector3 p = point - triangle[i1];

					float cross = Vector3.Cross(v, p).y;
					if (i1 == 0)
					{
						sign = cross < 0 ? -1 : 1;
					}
					else
					{
						if (cross * sign <= 0)
						{
							isConvex = true;
							break;
						}
					}
				}

				if (isConvex == false)
				{
					break;
				}
			}

			return isConvex;
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
