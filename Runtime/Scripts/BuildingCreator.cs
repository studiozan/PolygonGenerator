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

		public IEnumerator CreateBuildingMesh(List<SurroundedArea> areas)
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

				var param = new BuildingParameter(points);
				param.SetBuildingType(types[random.Next(types.Length)], random.Next(4));
				param.SetBuildingHeight(Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble()));
				parameters.Add(param);
			}

			meshCreator.BuildingPolygonCreate(parameters);

			yield break;
		}

		System.Random random;
		MeshCreator meshCreator;
		float minHeight;
		float maxHeight;
	}
}
