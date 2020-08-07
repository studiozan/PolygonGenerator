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

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width, float uvY1, float uvY2)
		{
			if (meshFilter != null)
			{
				lastInterruptionTime = System.DateTime.Now;

				SetPointsIndex(points);
				yield return CreateMeshParameter(points, width, uvY1, uvY2);
				meshFilter.sharedMesh = CreateMesh();
			}
		}

		public IEnumerator CreatePolygon(List<FieldConnectPoint> points, float width)
		{
			yield return CreatePolygon(points, width, 0, 1);
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

		void SetPointsIndex(List<FieldConnectPoint> connectPoints)
		{
			for (int i0 = 0; i0 < connectPoints.Count; ++i0)
			{
				connectPoints[i0].Index = i0;
			}
		}

		IEnumerator CreateMeshParameter(List<FieldConnectPoint> points, float width, float uvY1, float uvY2)
		{
			vertices.Clear();
			uvs.Clear();
			indices.Clear();
			connectedMap.Clear();
			indicesMap.Clear();
			float halfWidth = width * 0.5f;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point = points[i0];
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				if (connectPoints.Count != 0)
				{
					if (connectPoints.Count == 1)
					{
						FieldConnectPoint p = connectPoints[0];
						Vector3 dir = p.Position - point.Position;
						dir.Normalize();
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
							Vector3 p1 = connectPoints[clockwiseIndices[i1]].Position;
							Vector3 p2 = connectPoints[clockwiseIndices[(i1 + 1) % clockwiseIndices.Count]].Position;
							Vector3 v1 = p1 - origin;
							v1.Normalize();
							Vector3 v2 = p2 - origin;
							v2.Normalize();

							var rightBase = new Vector3(v1.z, 0, -v1.x) * halfWidth;
							var leftBase = new Vector3(-v2.z, 0, v2.x) * halfWidth;

							Vector3 r1 = rightBase + origin;
							Vector3 r2 = rightBase + p1;
							Vector3 l1 = leftBase + origin;
							Vector3 l2 = leftBase + p2;

							var intersection = new Vector3();
							if (TryGetIntersection(r1, r2, l1, l2, out intersection) == false)
							{
								intersection = r1;
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

		bool IsConnected(int index1, int index2)
		{
			if (connectedMap.TryGetValue(index1, out HashSet<int> connectedSet) != false)
			{
				if (connectedSet.Contains(index2) != false)
				{
					return true;
				}
			}

			return false;
		}

		void RegisterConnectedMap(int index1, int index2)
		{
			if (connectedMap.TryGetValue(index1, out HashSet<int> connectedSet1) != false)
			{
				connectedSet1.Add(index2);
			}
			else
			{
				connectedSet1 = new HashSet<int>();
				connectedSet1.Add(index2);
				connectedMap.Add(index1, connectedSet1);
			}

			if (connectedMap.TryGetValue(index2, out HashSet<int> connectedSet2) != false)
			{
				connectedSet2.Add(index1);
			}
			else
			{
				connectedSet2 = new HashSet<int>();
				connectedSet2.Add(index1);
				connectedMap.Add(index2, connectedSet2);
			}
		}


		public static readonly float kElapsedTimeToInterrupt = 16.7f;

		System.DateTime lastInterruptionTime;

		GameObject gameObject;
		MeshFilter meshFilter;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();
		Dictionary<int, HashSet<int>> connectedMap = new Dictionary<int, HashSet<int>>();

		Dictionary<int, Dictionary<int, int[]>> indicesMap = new Dictionary<int, Dictionary<int, int[]>>();
	}
}
