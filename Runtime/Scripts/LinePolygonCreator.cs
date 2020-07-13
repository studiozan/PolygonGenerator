using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class LinePolygonCreator
	{
		public void SetObject(GameObject gameObject)
		{
			this.gameObject = gameObject;
			meshFilter = gameObject?.GetComponent<MeshFilter>();
		}

		public void CreatePolygon(List<FieldConnectPoint> points, float width)
		{
			if (meshFilter != null)
			{
				SetPointsIndex(points);
				CreateVerticesAndIndices(points, width);
				meshFilter.sharedMesh = CreateMesh();
			}
		}

		Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.SetVertices(vertices);
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

		void CreateVerticesAndIndices(List<FieldConnectPoint> points, float width)
		{
			vertices.Clear();
			indices.Clear();
			connectedMap.Clear();
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				FieldConnectPoint point = points[i0];
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				for (int i1 = 0; i1 < connectPoints.Count; ++i1)
				{
					FieldConnectPoint nextPoint = connectPoints[i1];
					if (IsConnected(point.Index, nextPoint.Index) == false)
					{
						RegisterConnectedMap(point.Index, nextPoint.Index);
						Vector3 dir = nextPoint.Position - point.Position;
						float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
						Quaternion rotation = Quaternion.Euler(0, angle, 0);
						Vector3 leftBase = rotation * new Vector3(-width / 2, 0, 0);
						Vector3 rightBase = rotation * new Vector3(width / 2, 0, 0);
						int indexBase = vertices.Count;
						vertices.Add(leftBase + point.Position);
						vertices.Add(rightBase + point.Position);
						vertices.Add(leftBase + nextPoint.Position);
						vertices.Add(rightBase + nextPoint.Position);

						indices.Add(indexBase);
						indices.Add(indexBase + 2);
						indices.Add(indexBase + 1);
						indices.Add(indexBase + 1);
						indices.Add(indexBase + 2);
						indices.Add(indexBase + 3);
					}
				}
			}
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


		GameObject gameObject;
		MeshFilter meshFilter;

		List<Vector3> vertices = new List<Vector3>();
		List<int> indices = new List<int>();
		Dictionary<int, HashSet<int>> connectedMap = new Dictionary<int, HashSet<int>>();
	}
}
