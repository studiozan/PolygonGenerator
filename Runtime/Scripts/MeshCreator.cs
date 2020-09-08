using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonGenerator
{
	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshFilter))]
	public class MeshCreator : MonoBehaviour
	{
		/**
		 * 渡された頂点情報からポリゴンを生成する
		 *
		 * @param vec_list	頂点座標のリスト
		 * @param uv_list	UV座標のリスト
		 */
		public void PolygonCreate( List<Vector3> vec_list, List<Vector2> uv_list)
		{
			int i0;
			var tri_list = new List<int>();
			var color_list = new List<Color32>();
			var tmp_color = new Color32(255,255,255,0);;

			/*! 頂点情報 */
			for( i0 = 0; i0 < vec_list.Count; ++i0)
			{
				tri_list.Add( i0);
			}

			for( i0 = 0; i0 < vec_list.Count; ++i0)
			{
				color_list.Add(tmp_color);
			}

			PolygonCreate( vec_list, tri_list, uv_list, color_list);
		}
		
		public void PolygonCreate( List<Vector3> vec_list)
		{
			int i0;
			var tri_list = new List<int>();
			var uv_list = new List<Vector2>();
			Vector2 tmp_uv = Vector2.zero;
			var color_list = new List<Color32>();
			var tmp_color = new Color32(255,255,255,255);

			/*! 頂点情報 */
			for( i0 = 0; i0 < vec_list.Count; ++i0)
			{
				tri_list.Add( i0);
			}

			/*! UV情報 */
			for( i0 = 0; i0 < vec_list.Count; ++i0)
			{
				uv_list.Add( tmp_uv);
			}

			for( i0 = 0; i0 < vec_list.Count; ++i0)
			{
				color_list.Add( tmp_color);
			}
			
			PolygonCreate( vec_list, tri_list, uv_list, color_list);
		}

		public void PolygonCreate( List<Vector3> vec_list, List<int> tri_list, List<Vector2> uv_list, List<Color32> color_list)
		{
			var mesh = new Mesh();

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;	/*! 頂点数の制限をなくした状態 */
			mesh.SetVertices( vec_list);
			mesh.SetTriangles( tri_list, 0);
			mesh.SetUVs( 0, uv_list);
			mesh.SetColors( color_list);

			mesh.RecalculateNormals();
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = mesh;
		}

		/**
		 * 渡されたビルのパラメータリストを元にビルのポリゴンをまとめて生成する
		 */
		public void BuildingPolygonCreate( List<BuildingParameter> building_list, float buildingInterval = 100f)
		{
			var vec_list = new List<Vector3>();
			var tri_list = new List<int>();
			var uv_list = new List<Vector2>();
			var color_list = new List<Color32>();
			List<Vector2> tmp_v2_list;
			var tmp_v3_list = new List<Vector3>();
			BuildingParameter tmp_buil;
			Vector3 tmp_vec;
			var tmp_color = new Color32(255,255,255,0);
			int i0, i1, i2, tmp_i, tri_count;
			float height, height_min;
			tri_count = 0;

			for( i0 = 0; i0 < building_list.Count; ++i0)
			{
				tmp_buil = building_list[ i0];

				height = tmp_buil.BuildingHeight;
				tmp_v2_list = tmp_buil.GetRoofTopUV();
				/*! 屋上のポリゴンを設定する */
				for( i1 = 0; i1 < tmp_buil.PositionList.Count; ++i1)
				{
					tmp_vec = tmp_buil.PositionList[ i1];
					tmp_vec.y = height;
					vec_list.Add( tmp_vec);
					uv_list.Add( tmp_v2_list[ i1]);
					color_list.Add( tmp_color);
				}
				tri_list.Add( tri_count);	tri_list.Add( tri_count + 1);	tri_list.Add( tri_count + 2);
				tri_list.Add( tri_count + 2);	tri_list.Add( tri_count + 3);	tri_list.Add( tri_count);
				tri_count += 4;

				/*! 側面のポリゴンを作る */
				tmp_v2_list = tmp_buil.GetSideUV();
				while( height > 0f)
				{
					height_min = height - buildingInterval;
					if( height_min < 0f)
					{
						height_min = 0f;
					}
					for( i1 = 0; i1 < 4; ++i1)
					{
						tmp_i = (i1 + 1) % 4;
						tmp_v3_list.Clear();
						for( i2 = 0; i2 < 4; ++i2)
						{
							switch( i2)
							{
								case 0:
								tmp_vec = tmp_buil.PositionList[ tmp_i];
								tmp_vec.y = height;
								break;
								case 1:
								tmp_vec = tmp_buil.PositionList[ i1];
								tmp_vec.y = height;
								break;
								case 2:
								tmp_vec = tmp_v3_list[ 1];
								tmp_vec.y = height_min;
								break;
								case 3:
								tmp_vec = tmp_v3_list[ 0];
								tmp_vec.y = height_min;
								break;
								default:
								tmp_vec = tmp_buil.PositionList[ tmp_i];
								break;
							}
							tmp_v3_list.Add( tmp_vec);
							uv_list.Add( tmp_v2_list[ i2]);
							color_list.Add( tmp_color);
						}
						vec_list.AddRange( tmp_v3_list);
						tri_list.Add( tri_count);	tri_list.Add( tri_count + 1);	tri_list.Add( tri_count + 2);
						tri_list.Add( tri_count + 2);	tri_list.Add( tri_count + 3);	tri_list.Add( tri_count);
						tri_count += 4;
					}
					height -= buildingInterval;
				}
			}
			
			PolygonCreate( vec_list, tri_list, uv_list, color_list);
		}
	}
}
