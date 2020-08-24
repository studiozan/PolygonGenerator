﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class LinePolygonCreator
	{
		public void SetObject(GameObject gameObject)
		{
			this.gameObject = gameObject;
			meshFilter = gameObject?.GetComponent<MeshFilter>();
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width, float uvY1, float uvY2, float disconnectionProb = 0)
		{
			if (meshFilter != null)
			{
				lastInterruptionTime = System.DateTime.Now;

				this.width = width;
				Prepare(points);
				yield return CoroutineUtility.CoroutineCycle( CreateMeshParameter(points, width, uvY1, uvY2, disconnectionProb));
				meshFilter.sharedMesh = CreateMesh();
			}
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width)
		{
			yield return CoroutineUtility.CoroutineCycle( CreatePolygon(points, width, 0, 1, 0));
		}

		Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(indices, 0);

			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.RecalculateBounds();

			return mesh;
		}

		void Prepare(List<FieldConnectPoint> points)
		{
			connectCountMap.Clear();
			widthMap.Clear();
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point = points[i0];
				point.Index = i0;
				if (point.Type == PointType.kGridRoad)
				{
					List<FieldConnectPoint> connectPoints = point.ConnectionList;
					int count = connectPoints.Count;
					for (int i1 = 0; i1 < connectPoints.Count; ++i1)
					{
						if (connectPoints[i1].Type != PointType.kGridRoad)
						{
							--count;
						}
					}
					connectCountMap.Add(i0, count);
				}
			}

			/* 道路の幅が狭い方を多く生成させたかったので、ちょっと改造してあります（玉城） */
			float[] widthCand = { width, width * 0.75f, width * 0.75f, width * 0.5f, width * 0.5f, width * 0.5f, width * 0.5f, width * 0.25f, width * 0.25f, width * 0.25f };
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point= points[i0];
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				List<float> widthList = point.WidthList;
				for (int i1 = 0; i1 < connectPoints.Count; ++i1)
				{
					FieldConnectPoint connectPoint = connectPoints[i1];
					var key1 = new Vector2Int(point.Index, connectPoint.Index);
					var key2 = new Vector2Int(connectPoint.Index, point.Index);
					float width;
					if (widthMap.TryGetValue(key1, out width) == false)
					{
						width = widthCand[(point.Type != PointType.kRiver ? random.Next(10) : 0)];
						widthMap.Add(key1, width);
						widthMap.Add(key2, width);
					}
					widthList.Add(width);
				}
			}
		}

		IEnumerator CreateMeshParameter(List<FieldConnectPoint> points, float width, float uvY1, float uvY2, float disconnectionProb)
		{
			vertices.Clear();
			uvs.Clear();
			indices.Clear();
			indicesMap.Clear();
			judgedCombinationSet.Clear();
			disconnectCombinationSet.Clear();
			// float halfWidth = width * 0.5f;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point = points[i0];
				List<float> widthList = point.WidthList;
				var connectPoints = new List<FieldConnectPoint>(point.ConnectionList);
				if (connectPoints.Count != 0)
				{
					if (point.Type == PointType.kGridRoad)
					{
						var candidates = new List<FieldConnectPoint>();
						for (int i1 = connectPoints.Count - 1; i1 >= 0; --i1)
						{
							FieldConnectPoint connectPoint = connectPoints[i1];
							var item = new Vector2Int(point.Index, connectPoint.Index);
							if (connectPoint.Type == PointType.kGridRoad)
							{
								if (connectCountMap[connectPoint.Index] >= 2 && judgedCombinationSet.Contains(item) == false)
								{
									candidates.Add(connectPoint);
								}
								else if (disconnectCombinationSet.Contains(item) != false)
								{
									connectPoints.RemoveAt(i1);
								}
							}
						}

						if (candidates.Count >= 2)
						{
							int index = random.Next(candidates.Count);
							FieldConnectPoint candidate = candidates[index];
							var item1 = new Vector2Int(point.Index, candidate.Index);
							var item2 = new Vector2Int(candidate.Index, point.Index);
							if (DetectFromPercent(disconnectionProb) != false)
							{
								connectPoints.Remove(candidate);
								disconnectCombinationSet.Add(item1);
								disconnectCombinationSet.Add(item2);
								--connectCountMap[point.Index];
								--connectCountMap[candidate.Index];
							}

							judgedCombinationSet.Add(item1);
							judgedCombinationSet.Add(item2);
						}
					}

					if (connectPoints.Count == 1)
					{
						FieldConnectPoint p = connectPoints[0];
						Vector3 dir = p.Position - point.Position;
						dir.Normalize();
						float halfWidth = widthList[0] * 0.5f;
						var left = new Vector3(-dir.z, 0, dir.x) * halfWidth + point.Position;
						var right = new Vector3(dir.z, 0, -dir.x) * halfWidth + point.Position;

						int leftIndex = vertices.Count;
						int rightIndex = leftIndex + 1;
						vertices.Add(left);
						uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));
						vertices.Add(right);
						uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));

						Dictionary<int, int[]> map;
						if (indicesMap.TryGetValue(point.Index, out map) == false)
						{
							map = new Dictionary<int, int[]>();
							indicesMap.Add(point.Index, map);
						}
						map.Add(p.Index, new int[] { leftIndex, rightIndex });

						if (indicesMap.TryGetValue(p.Index, out map) != false)
						{
							if (map.TryGetValue(point.Index, out int[] indexLR) != false)
							{
								vertices.Add(vertices[indexLR[1]]);
								vertices.Add(vertices[indexLR[0]]);

								uvs[leftIndex] = new Vector2(0, uvY1);
								uvs[rightIndex] = new Vector2(0, uvY2);
								uvs.Add(new Vector2(1, uvY1));
								uvs.Add(new Vector2(1, uvY2));

								indices.Add(leftIndex);
								indices.Add(leftIndex + 2);
								indices.Add(rightIndex);
								indices.Add(rightIndex);
								indices.Add(leftIndex + 2);
								indices.Add(rightIndex + 2);
							}
						}
					}
					else
					{
						List<int> clockwiseIndices = GetClockwiseIndices(point, connectPoints, 0);
						int originIndex = vertices.Count;
						Vector3 origin = point.Position;
						if (connectPoints.Count >= 3)
						{
							vertices.Add(origin);
							uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));
						}
						for (int i1 = 0; i1 < clockwiseIndices.Count; ++i1)
						{
							int index1 = clockwiseIndices[i1];
							int index2 = clockwiseIndices[(i1 + 1) % clockwiseIndices.Count];
							Vector3 p1 = connectPoints[index1].Position;
							Vector3 p2 = connectPoints[index2].Position;
							Vector3 v1 = p1 - origin;
							v1.Normalize();
							Vector3 v2 = p2 - origin;
							v2.Normalize();

							float width1 = widthList[index1];
							float width2 = widthList[index2];
							float halfWidth1 = width1 * 0.5f;
							float halfWidth2 = width2 * 0.5f;
							var rightBase = new Vector3(v1.z, 0, -v1.x) * halfWidth1;
							var leftBase = new Vector3(-v2.z, 0, v2.x) * halfWidth2;

							Vector3 r1 = rightBase + origin;
							Vector3 r2 = rightBase + p1;
							Vector3 l1 = leftBase + origin;
							Vector3 l2 = leftBase + p2;

							var intersection = new Vector3();
							if (TryGetIntersection(r1, r2, l1, l2, out intersection) == false)
							{
								intersection = r1;
							}

							Vector3 dist = intersection - point.Position;
							float maxDist = width * 3;
							if (dist.sqrMagnitude > maxDist * maxDist)
							{
								intersection = (r1 + l1) * 0.5f;
							}

							vertices.Add(intersection);
							uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));
							vertices.Add(intersection);
							uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));

							if (connectPoints.Count >= 3)
							{
								indices.Add(originIndex);
								int offsetL = (i1 + 1) * 2;
								int offsetR = (offsetL + 1) % (clockwiseIndices.Count * 2);
								indices.Add(originIndex + offsetL);
								indices.Add(originIndex + offsetR);
							}
						}

						for (int i1 = 0; i1 < clockwiseIndices.Count; ++i1)
						{
							FieldConnectPoint p = connectPoints[clockwiseIndices[i1]];

							int offsetL = ((i1 + clockwiseIndices.Count - 1) % clockwiseIndices.Count + 1) * 2;
							int offsetR = (offsetL + 1) % (clockwiseIndices.Count * 2);
							int leftIndex = originIndex + offsetL;
							int rightIndex = originIndex + offsetR;

							if (connectPoints.Count < 3)
							{
								--leftIndex;
								--rightIndex;
							}

							Dictionary<int, int[]> map;
							if (indicesMap.TryGetValue(point.Index, out map) == false)
							{
								map = new Dictionary<int, int[]>();
								indicesMap.Add(point.Index, map);
							}
							map.Add(p.Index, new int[] { leftIndex, rightIndex });

							if (indicesMap.TryGetValue(p.Index, out map) != false)
							{
								if (map.TryGetValue(point.Index, out int[] indexLR) != false)
								{
									uvs[leftIndex] = new Vector2(1, uvY1);
									uvs[rightIndex] = new Vector2(1, uvY2);
									uvs[indexLR[1]] = new Vector2(0, uvY1);
									uvs[indexLR[0]] = new Vector2(0, uvY2);

									indices.Add(leftIndex);
									indices.Add(indexLR[1]);
									indices.Add(rightIndex);
									indices.Add(rightIndex);
									indices.Add(indexLR[1]);
									indices.Add(indexLR[0]);
								}
							}
						}
					}
				}

				if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= kElapsedTimeToInterrupt)
				{
					yield return null;
					lastInterruptionTime = System.DateTime.Now;
				}
			}
		}

		List<int> GetClockwiseIndices(FieldConnectPoint origin, List<FieldConnectPoint> points, int baseIndex)
		{
			var clockwise = new List<int>();

			var rightUp = new List<KeyValuePair<int, float>>();
			var rightDown = new List<KeyValuePair<int, float>>();
			var leftDown = new List<KeyValuePair<int, float>>();
			var leftUp = new List<KeyValuePair<int, float>>();

			Vector3 baseVec = points[baseIndex].Position - origin.Position;
			baseVec.Normalize();
			var right = new Vector3(baseVec.z, 0, -baseVec.x);
			var left = new Vector3(-baseVec.z, 0, baseVec.x);
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					Vector3 v = points[i0].Position - origin.Position;
					v.Normalize();
					float cross1 = Vector3.Cross(baseVec, v).y;
					//右
					if (cross1 >= 0)
					{
						float cross2 = Vector3.Cross(right, v).y;
						if (cross2 <= 0)
						{
							rightUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							rightDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
					//左
					else
					{
						float cross2 = Vector3.Cross(left, v).y;
						if (cross2 <= 0)
						{
							leftDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							leftUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
				}
			}

			rightUp.Sort((a, b) => a.Value.CompareTo(b.Value));
			rightDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftUp.Sort((a, b) => a.Value.CompareTo(b.Value));

			clockwise.Add(baseIndex);
			clockwise.AddRange(rightUp.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(rightDown.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(leftDown.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(leftUp.ConvertAll<int>(pair => pair.Key));

			return clockwise;
		}

		bool TryGetIntersection(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2, out Vector3 intersection)
		{
			bool isIntersecting = false;
			intersection = Vector3.zero;

			Vector3 v1 = e1 - s1;
			Vector3 v2 = s2 - s1;
			Vector3 v3 = s1 - e2;
			Vector3 v4 = e2 - s2;

			float area1 = Vector3.Cross(v1, v2).y * 0.5f;
			float area2 = Vector3.Cross(v1, v3).y * 0.5f;
			float area = area1 + area2;

			if (Mathf.Approximately(area, 0) == false)
			{
				isIntersecting = true;
				intersection = s2 + v4 * area1 / area;
			}

			return isIntersecting;
		}

		void AddJudgedCombination(int index1, int index2)
		{
			judgedCombinationSet.Add(new Vector2Int(index1, index2));
			judgedCombinationSet.Add(new Vector2Int(index2, index1));
		}

		bool DetectFromPercent(float percent)
		{
			int numDigit = 0;
			string percentString = percent.ToString();
			if (percentString.IndexOf(".") > 0)
			{
				numDigit = percentString.Split('.')[1].Length;
			}

			int rate = (int)Mathf.Pow(10, numDigit);
			int maxValue = 100 * rate;
			int border = (int)(percent * rate);

			return random.Next(0, maxValue) < border;
		}


		public static readonly float kElapsedTimeToInterrupt = 16.7f;

		System.Random random = new System.Random(0);
		System.DateTime lastInterruptionTime;

		GameObject gameObject;
		MeshFilter meshFilter;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();
		Dictionary<int, Dictionary<int, int[]>> indicesMap = new Dictionary<int, Dictionary<int, int[]>>();
		HashSet<Vector2Int> judgedCombinationSet = new HashSet<Vector2Int>();
		HashSet<Vector2Int> disconnectCombinationSet = new HashSet<Vector2Int>();
		Dictionary<int, int> connectCountMap = new Dictionary<int, int>();
		Dictionary<Vector2Int, float> widthMap = new Dictionary<Vector2Int, float>();

		float width;
	}
}
