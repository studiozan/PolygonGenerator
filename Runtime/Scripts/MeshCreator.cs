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
			var mesh = new Mesh();
			List<Vector3> vert = new List<Vector3>();
			List<int> tri = new List<int>();
			List<Vector2> uvs = new List<Vector2>();

			/*! 頂点座標 */
			mesh.SetVertices( vec_list);

			/*! 頂点情報 */
			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				tri.Add( i0);
			}
			mesh.SetTriangles( tri, 0);

			mesh.SetUVs( 0, uv_list);
			List<Color> color_list = new List<Color>();
			Color tmp_color;
			for( i0 = 0; i0 < vec_list.Count; i0++)
			{
				tmp_color = Color.red;
			}
			mesh.SetColors( color_list);

			mesh.RecalculateNormals();
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = mesh;
		 }
	}
}
