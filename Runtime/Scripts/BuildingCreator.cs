using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	public class BuildingCreator : MonoBehaviour
	{
		void Awake()
		{
			meshCreator = GetComponent<MeshCreator>();
			townGenerator.OnGenerate += CreateBuildingMesh;

			random = new System.Random(0);
		}

		void CreateBuildingMesh(TownGenerator generator)
		{
			List<SurroundedArea> areas = generator.SurroundedAreas;
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
		}

		[SerializeField]
		TownGenerator townGenerator = default;

		[SerializeField]
		float minHeight = 10;
		[SerializeField]
		float maxHeight = 50;

		MeshCreator meshCreator;

		System.Random random;
	}
}
