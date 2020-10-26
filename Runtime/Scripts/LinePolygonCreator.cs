using System.Collections;
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

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, WeightedValue[] widthCandidates, float uvY1, float uvY2, float disconnectionProb = 0, float decalSize = 1)
		{
			if (meshFilter != null)
			{
				lastInterruptionTime = System.DateTime.Now;
				this.widthCandidates = widthCandidates;
				Prepare(points);
				yield return CoroutineUtility.CoroutineCycle( CreateMeshParameter(points, uvY1, uvY2, decalSize));
				meshFilter.sharedMesh = CreateMesh();
			}
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, LinePolygonParameter parameter)
		{
			yield return CoroutineUtility.CoroutineCycle(CreatePolygon(points, parameter.widthCandidates, parameter.uvY1, parameter.uvY2, 0, parameter.decalSize));
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width, float uvY1, float uvY2)
		{
			var candidates = new WeightedValue[] { new WeightedValue { value = width, weight = 1 } };
			yield return CoroutineUtility.CoroutineCycle(CreatePolygon(points, candidates, uvY1, uvY2));
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, WeightedValue[] widthCandidates)
		{
			yield return CoroutineUtility.CoroutineCycle( CreatePolygon(points, widthCandidates, 0, 1, 0));
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width)
		{
			yield return CoroutineUtility.CoroutineCycle( CreatePolygon(points, width, 0, 1));
		}

		Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetUVs(1, decalUvs);
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

			maxWidth = 0;
			for (int i0 = 0; i0 < widthCandidates.Length; ++i0)
			{
				float width = widthCandidates[i0].value;
				if (width > maxWidth)
				{
					maxWidth = width;
				}
			}

			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point= points[i0];
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				List<float> widthList = point.WidthList;
				for (int i1 = connectPoints.Count - 1; i1 >= 0; --i1)
				{
					FieldConnectPoint connectPoint = connectPoints[i1];
					var key1 = new Vector2Int(point.Index, connectPoint.Index);
					var key2 = new Vector2Int(connectPoint.Index, point.Index);
					float width;
					if (widthMap.TryGetValue(key1, out width) == false)
					{
						width = DetectWeightedValue(widthCandidates);
						widthMap.Add(key1, width);
						widthMap.Add(key2, width);
					}

					if (Mathf.Approximately(width, 0) != false)
					{
						connectPoints.RemoveAt(i1);
					}
					else
					{
						widthList.Add(width);
					}
				}
				widthList.Reverse();
			}
		}

		IEnumerator CreateMeshParameter(List<FieldConnectPoint> points, float uvY1, float uvY2, float decalSize)
		{
			vertices.Clear();
			uvs.Clear();
			decalUvs.Clear();
			indices.Clear();
			indicesMap.Clear();
			judgedCombinationSet.Clear();
			disconnectCombinationSet.Clear();

			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point = points[i0];
				List<float> widthList = point.WidthList;
				var connectPoints = new List<FieldConnectPoint>(point.ConnectionList);
				if (connectPoints.Count != 0)
				{
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
						decalUvs.Add(new Vector2(left.x / decalSize, left.z / decalSize));
						decalUvs.Add(new Vector2(right.x / decalSize, right.z / decalSize));

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
								Vector3 vertL = vertices[indexLR[1]];
								Vector3 vertR = vertices[indexLR[0]];
								vertices.Add(vertL);
								vertices.Add(vertR);

								uvs[leftIndex] = new Vector2(0, uvY1);
								uvs[rightIndex] = new Vector2(0, uvY2);
								uvs.Add(new Vector2(1, uvY1));
								uvs.Add(new Vector2(1, uvY2));

								decalUvs.Add(new Vector2(vertL.x / decalSize, vertL.z / decalSize));
								decalUvs.Add(new Vector2(vertR.x / decalSize, vertR.z / decalSize));

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
							decalUvs.Add(new Vector2(origin.x / decalSize, origin.z / decalSize));
						}
						for (int i1 = 0; i1 < clockwiseIndices.Count; ++i1)
						{
							int index1 = clockwiseIndices[i1];
							int index2 = clockwiseIndices[(i1 + 1) % clockwiseIndices.Count];
							Vector3 pos1 = connectPoints[index1].Position;
							Vector3 pos2 = connectPoints[index2].Position;
							Vector3 dir1 = pos1 - origin;
							dir1.Normalize();
							Vector3 dir2 = pos2 - origin;
							dir2.Normalize();

							float width1 = widthList[index1];
							float width2 = widthList[index2];
							float halfWidth1 = width1 * 0.5f;
							float halfWidth2 = width2 * 0.5f;
							var rightBase = new Vector3(dir1.z, 0, -dir1.x) * halfWidth1;
							var leftBase = new Vector3(-dir2.z, 0, dir2.x) * halfWidth2;

							Vector3 posR1 = rightBase + origin;
							Vector3 posR2 = rightBase + pos1;
							Vector3 posL1 = leftBase + origin;
							Vector3 posL2 = leftBase + pos2;

							var intersection = new Vector3();
							if (TryGetIntersection(posR1, posR2, posL1, posL2, out intersection) == false)
							{
								intersection = posR1;
							}

							Vector3 dist = intersection - point.Position;
							float maxDist = maxWidth * 3;
							if (dist.sqrMagnitude > maxDist * maxDist)
							{
								intersection = (posR1 + posL1) * 0.5f;
							}

							vertices.Add(intersection);
							uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));
							vertices.Add(intersection);
							uvs.Add(new Vector2(0.5f, (uvY1 + uvY2) * 0.5f));
							decalUvs.Add(new Vector2(intersection.x / decalSize, intersection.z / decalSize));
							decalUvs.Add(new Vector2(intersection.x / decalSize, intersection.z / decalSize));

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
					Vector3 dir = points[i0].Position - origin.Position;
					dir.Normalize();
					float cross1 = Vector3.Cross(baseVec, dir).y;
					//右
					if (cross1 >= 0)
					{
						float cross2 = Vector3.Cross(right, dir).y;
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
						float cross2 = Vector3.Cross(left, dir).y;
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

		bool TryGetIntersection(Vector3 startPos1, Vector3 endPos1, Vector3 startPos2, Vector3 endPos2, out Vector3 intersection)
		{
			bool isIntersecting = false;
			intersection = Vector3.zero;

			Vector3 dir1 = endPos1 - startPos1;
			Vector3 dir2 = startPos2 - startPos1;
			Vector3 dir3 = startPos1 - endPos2;
			Vector3 dir4 = endPos2 - startPos2;

			float area1 = Vector3.Cross(dir1, dir2).y * 0.5f;
			float area2 = Vector3.Cross(dir1, dir3).y * 0.5f;
			float area = area1 + area2;

			if (Mathf.Approximately(area, 0) == false)
			{
				isIntersecting = true;
				intersection = startPos2 + dir4 * area1 / area;
			}

			return isIntersecting;
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

		float DetectWeightedValue(WeightedValue[] candidates)
		{
			float value = 0;

			float totalWeight = 0;
			for (int i0 = 0; i0 < candidates.Length; ++i0)
			{
				totalWeight += candidates[i0].weight;
			}

			float border = totalWeight * (float)random.NextDouble();

			for (int i0 = 0; i0 < candidates.Length; ++i0)
			{
				float weight = candidates[i0].weight;
				if (Mathf.Approximately(weight, 0) == false)
				{
					if (border <= weight)
					{
						value = candidates[i0].value;
						break;
					}
				}

				border -= weight;
			}

			return value;
		}


		public static readonly float kElapsedTimeToInterrupt = 16.7f;

		System.Random random = new System.Random(0);
		System.DateTime lastInterruptionTime;

		GameObject gameObject;
		MeshFilter meshFilter;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector2> decalUvs = new List<Vector2>();
		List<int> indices = new List<int>();
		Dictionary<int, Dictionary<int, int[]>> indicesMap = new Dictionary<int, Dictionary<int, int[]>>();
		HashSet<Vector2Int> judgedCombinationSet = new HashSet<Vector2Int>();
		HashSet<Vector2Int> disconnectCombinationSet = new HashSet<Vector2Int>();
		Dictionary<int, int> connectCountMap = new Dictionary<int, int>();
		Dictionary<Vector2Int, float> widthMap = new Dictionary<Vector2Int, float>();

		WeightedValue[] widthCandidates;
		float maxWidth;
	}
}
