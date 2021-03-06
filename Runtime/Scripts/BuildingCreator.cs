﻿using System.Collections;
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
			this.Initialize(new[] { meshCreator }, seed);
		}

		public void Initialize(IReadOnlyList<MeshCreator> meshCreators, int seed = 0)
		{
			random = new System.Random(seed);
			this.meshCreators = meshCreators;
		}

		//第３引数使用していません
		public IEnumerator CreateBuildingMesh(List<SurroundedArea> areas, BuildingCondition condition, float buildingInterval = 100f)
		{
			lastInterruptionTime = System.DateTime.Now;

			var allBuildingParameters = new List<BuildingParameter>();
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

			var buildableAreas = new List<SurroundedArea>();
			yield return CoroutineUtility.CoroutineCycle( DetectBuildableAreas(areas, condition, buildableAreas));
			int max = Mathf.RoundToInt((float)buildableAreas.Count * Mathf.Clamp01(condition.generationRate));
			var randomAreas = new List<SurroundedArea>();
			for (int i0 = 0; i0 < max; ++i0)
			{
				int randomIndex = random.Next(buildableAreas.Count);
				randomAreas.Add(buildableAreas[randomIndex]);
				int lastIndex = buildableAreas.Count - 1;
				buildableAreas[randomIndex] = buildableAreas[lastIndex];
				buildableAreas.RemoveAt(lastIndex);
			}
			randomAreas.Sort((a, b) => a.GetCenter().x.CompareTo(b.GetCenter().x));
			randomAreas.Sort((a, b) => a.GetCenter().z.CompareTo(b.GetCenter().z));
			int sqrtBuildingCount = 3;
			float buildingRatio = 0.25f;
			float spacingRatio = (1.0f - buildingRatio * sqrtBuildingCount) / (float)(sqrtBuildingCount - 1);
			for (int i0 = 0; i0 < randomAreas.Count; ++i0)
			{
				IReadOnlyList<Vector3> areaPoints = randomAreas[i0].AreaPoints;

				float minHeight, maxHeight;
				DetectRange(condition.heightRanges, out minHeight, out maxHeight);
				int buildingCountInArea = 0;
				// var demolishCounts = new int[] { 0, 3, 5 };
				// int demolishCount = demolishCounts[ random.Next(demolishCounts.Length)];
				int demolishCount = 0;
				var buildingNumbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
				var demolishIndices = new HashSet<int>();
				for( int i1 = 0; i1 < demolishCount; ++i1)
				{
					int buildingRandomIndex = random.Next(buildingNumbers.Count);
					demolishIndices.Add(buildingNumbers[buildingRandomIndex]);
					buildingNumbers.RemoveAt(buildingRandomIndex);
				}
				for (int row = 0; row < sqrtBuildingCount; ++row)
				{
					Vector3 top1 = Vector3.Lerp(areaPoints[0], areaPoints[3], (buildingRatio + spacingRatio) * row);
					Vector3 top2 = Vector3.Lerp(areaPoints[1], areaPoints[2], (buildingRatio + spacingRatio) * row);
					Vector3 bottom1 = Vector3.Lerp(areaPoints[0], areaPoints[3], (buildingRatio + spacingRatio) * row + buildingRatio);
					Vector3 bottom2 = Vector3.Lerp(areaPoints[1], areaPoints[2], (buildingRatio + spacingRatio) * row + buildingRatio);

					for (int column = 0; column < sqrtBuildingCount; ++column)
					{
						++buildingCountInArea;

						if (demolishIndices.Contains(buildingCountInArea) == false)
						{
							Vector3 leftTop = Vector3.Lerp(top1, top2, (buildingRatio + spacingRatio) * column);
							Vector3 rightTop = Vector3.Lerp(top1, top2, (buildingRatio + spacingRatio) * column + buildingRatio);
							Vector3 rightBottom = Vector3.Lerp(bottom1, bottom2, (buildingRatio + spacingRatio) * column + buildingRatio);
							Vector3 leftBottom = Vector3.Lerp(bottom1, bottom2, (buildingRatio + spacingRatio) * column);

							var buildingPoints = new List<Vector3>() { leftTop, rightTop, rightBottom, leftBottom };

							var param = new BuildingParameter(buildingPoints);
							param.SetBuildingType(types[random.Next(types.Length)], random.Next(4));
							param.SetBuildingHeight(Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble()));
							allBuildingParameters.Add(param);
						}
					}
				}

				if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= LinePolygonCreator.kElapsedTimeToInterrupt)
				{
					yield return null;
					lastInterruptionTime = System.DateTime.Now;
				}
			}

			int totalBuildingCount = allBuildingParameters.Count;
			int buildingCountPerMesh = totalBuildingCount / meshCreators.Count;
			int surplus = totalBuildingCount % meshCreators.Count;
			for (int i0 = 0; i0 < meshCreators.Count; ++i0)
			{
				int startIndex = buildingCountPerMesh * i0;
				int buildingCount = buildingCountPerMesh + (i0 == meshCreators.Count - 1 ? surplus : 0);
				List<BuildingParameter> parameters = allBuildingParameters.GetRange(startIndex, buildingCount);
				if (condition.enabledSort != false)
				{
					parameters.Sort((a, b) => (int)a.TextureType - (int)b.TextureType);
				}
				meshCreators[i0].BuildingPolygonCreate(parameters, buildingInterval);
			}
		}

		IEnumerator DetectBuildableAreas(List<SurroundedArea> areas, BuildingCondition condition, List<SurroundedArea> output)
		{
			output.Clear();
			for (int i0 = 0; i0 < areas.Count; ++i0)
			{
				var areaPoints = new List<Vector3>(areas[i0].AreaPoints);
				if (areaPoints.Count == 3)
				{
					areaPoints.Add(areaPoints[2]);
				}

				if (CanBuildBuilding(areaPoints, condition) != false)
				{
					output.Add(new SurroundedArea { AreaPoints = areaPoints });
				}

				if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= LinePolygonCreator.kElapsedTimeToInterrupt)
				{
					yield return null;
					lastInterruptionTime = System.DateTime.Now;
				}
			}
		}

		void DetectRange(WeightedRange[] ranges, out float min, out float max)
		{
			min = 0;
			max = 0;

			float totalWeight = 0;
			for (int i0 = 0; i0 < ranges.Length; ++i0)
			{
				totalWeight += ranges[i0].weight;
			}

			float border = totalWeight * (float)random.NextDouble();

			for (int i0 = 0; i0 < ranges.Length; ++i0)
			{
				WeightedRange range = ranges[i0];
				float weight = range.weight;
				if (Mathf.Approximately(weight, 0) == false)
				{
					if (border <= weight)
					{
						min = range.min;
						max = range.max;
						break;
					}

					border -= weight;
				}
			}
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
				Vector3 side1 = areaPoints[1] - areaPoints[0];
				Vector3 side2 = areaPoints[2] - areaPoints[1];
				Vector3 side3 = areaPoints[3] - areaPoints[2];
				Vector3 side4 = areaPoints[0] - areaPoints[3];

				float area = (Mathf.Abs(Vector3.Cross(side1, side2).y) + Mathf.Abs(Vector3.Cross(side3, side4).y)) * 0.5f;
				float sideRatio = condition.sideRatio;
				if (area < condition.minAreaSize || (CalcRatio(side1, side3) >= sideRatio && CalcRatio(side2, side4) >= sideRatio))
				{
					canBuild = false;
				}
				else
				{
					for (int i0 = 0; i0 < areaPoints.Count; ++i0)
					{
						Vector3 pos0 = areaPoints[i0];
						Vector3 pos1 = areaPoints[(i0 + 1) % areaPoints.Count];
						Vector3 pos2 = areaPoints[(i0 + areaPoints.Count - 1) % areaPoints.Count];

						Vector3 dir1 = pos1 - pos0;
						Vector3 dir2 = pos2 - pos0;

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

			float length1 = v1.magnitude;
			float length2 = v2.magnitude;

			if (Mathf.Approximately(length1, 0) == false && Mathf.Approximately(length2, 0) == false)
			{
				ratio = length1 > length2 ? length1 / length2 : length2 / length1;
			}

			return ratio;
		}

		System.Random random;
		System.DateTime lastInterruptionTime;
		IReadOnlyList<MeshCreator> meshCreators;
	}
}
