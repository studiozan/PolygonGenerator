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
			List<Vector3> vert = new List<Vector3>();
			List<int> tri = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			List<Color32> color_list = new List<Color32>();
			Color32 tmp_color;

			/*! 頂点情報 */
			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				tri.Add( i0);
			}

			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				//tmp_color = new Color32(255,255,255,100);
				tmp_color = new Color32(255,255,255,0);
				color_list.Add(tmp_color);
			}

			PolygonCreate( vec_list, tri, uv_list, color_list);
		}
		
		public void PolygonCreate( List<Vector3> vec_list)
		{
			int i0;
			List<int> tri = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			Vector2 tmp_uv = Vector2.zero;
			List<Color32> color_list = new List<Color32>();
			Color32 tmp_color = new Color32(255,255,255,255);

			/*! 頂点情報 */
			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				tri.Add( i0);
			}

			/*! UV情報 */
			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				uvs.Add( tmp_uv);
			}

			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				color_list.Add( tmp_color);
			}
			
			PolygonCreate( vec_list, tri, uvs, color_list);
		}

		public void PolygonCreate( List<Vector3> vec_list, List<int> tri_list, List<Vector2> uv_list, List<Color32> col_list)
		{
			var mesh = new Mesh();

			mesh.SetVertices( vec_list);
			mesh.SetTriangles( tri_list, 0);
			mesh.SetUVs( 0, uv_list);
			mesh.SetColors( col_list);

			mesh.RecalculateNormals();
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = mesh;
		}
	}
}
