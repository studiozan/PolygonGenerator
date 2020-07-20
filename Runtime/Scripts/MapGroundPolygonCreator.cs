using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	[System.Serializable]
	public class MapGroundPolygonCreator
	{
		/**
		 * ポリゴン作成用の情報を生成
		 *
		 * @param point_list	ポリゴン作成情報に使用する繋がりポイントのリスト
		 * @param ofset_y		ポリゴン作成時のYの高さ
		 */
		public IEnumerator GroundPolygonCreate( Transform parent, List<FieldConnectPoint> point_list, Vector3 min, Vector3 max, float ofset_y = -0.1f)
		{
			int i0, i1, i2, i3, count;
			FieldConnectPoint tmp_point;
			var vec_tbl = new Vector3[ 3];
			Vector3 tmp_vec, center_vec;
			Vector2 tmp_uv = Vector2.zero;
			MeshCreator mesh_script;
			float tmp_f, size;
			var vec_list = new List<Vector3>();
			var uv_list = new List<Vector2>();
			var tri_list = new List<int>();
			var color_list = new List<Color32>();
			var tmp_color = new Color32(255,255,255,0);
			size = 50f;
			size = size * size;
			
			for( i0 = 0; i0 < point_list.Count; ++i0)
			{
				tmp_point = point_list[ i0];
				count = tmp_point.ConnectionList.Count;
				/*! 最低でも2点無いとポリゴンが生成出来ないので、繋がっている数が1以下なら処理しない */
				if( count <= 1)
				{
					continue;
				}
				for( i1 = 0; i1 < count; ++i1)
				{
					for( i2 = i1 + 1; i2 < count + 1; ++i2)
					{
						if( i1 == i2)
						{
							continue;
						}
						/*! 自分の座標と繋がっている座標の2点でポリゴンを生成する
						 *	これだとポリゴンが重なる部分が出てくるので、ポリゴンが重ならずに生成されるように別の処理に変更したい
						 */
						/*! 基準点と繋がっている2点間とのポリゴンを生成する */
						vec_tbl[ 0] = new Vector3( tmp_point.Position.x, tmp_point.Position.y, tmp_point.Position.z);
						tmp_vec = tmp_point.ConnectionList[ i1].Position;
						vec_tbl[ 1] = new Vector3( tmp_vec.x, tmp_vec.y, tmp_vec.z);
						tmp_vec = tmp_point.ConnectionList[ (i2 % count)].Position;
						vec_tbl[ 2] = new Vector3( tmp_vec.x, tmp_vec.y, tmp_vec.z);
						tmp_f = CrossY( vec_tbl[ 0], vec_tbl[ 1], vec_tbl[ 2]);
						if( tmp_f < 0)
						{
							tmp_vec = vec_tbl[ 1];
							vec_tbl[ 1] = vec_tbl[ 2];
							vec_tbl[ 2] = tmp_vec;
						}
						for( i3 = 0; i3 < vec_tbl.Length; ++i3)
						{
							MinMaxCheck( ref vec_tbl[ i3], min, max);
							vec_tbl[ i3].y = ofset_y;
							vec_list.Add( vec_tbl[ i3]);
							tmp_uv.x = vec_tbl[ i3].x * 0.01f;
							tmp_uv.y = vec_tbl[ i3].z * 0.01f;
							uv_list.Add( tmp_uv);
						}
						/*! 対面のテクスチャも作る設定 */
						tmp_vec = vec_tbl[ 1] - vec_tbl[ 0];
						vec_tbl[ 0] = vec_tbl[ 2] + tmp_vec;
						tmp_vec = vec_tbl[ 1];
						vec_tbl[ 1] = vec_tbl[ 2];
						vec_tbl[ 2] = tmp_vec;
						for( i3 = 0; i3 < vec_tbl.Length; ++i3)
						{
							MinMaxCheck( ref vec_tbl[ i3], min, max);
							vec_tbl[ i3].y = ofset_y;
							vec_list.Add( vec_tbl[ i3]);
							tmp_uv.x = vec_tbl[ i3].x * 0.01f;
							tmp_uv.y = vec_tbl[ i3].z * 0.01f;
							uv_list.Add( tmp_uv);
						}
					}
				}
			}

			/*! 重ねて表示するテクスチャの座標をランダムに出す */
			var SystemRandom = new System.Random();
			center_vec = new Vector3(0,0,0);
			center_vec.x = (float)SystemRandom.NextDouble() * 300f + 100f;
			center_vec.z = (float)SystemRandom.NextDouble() * 300f + 300f;
			if( createObj != null)
			{
				GameObject obj;
				for( i0 = 0; i0 < vec_list.Count; ++i0)
				{
					tri_list.Add( i0);
				}
#if false
				/*! 特定の座標周りだけテクスチャを上乗せする処理。
					頂点カラーの設定が上手くいっていないので、重なってるポリゴンの部分で上乗せ具合が違っててチラつく
				 */
				byte tmp_b;
				Vector3 sub_vec;
				for( i0 = 0; i0 < vec_list.Count; ++i0)
				{
					sub_vec = center_vec - vec_list[ i0];
					tmp_f = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
					if( tmp_f > size)
					{
						tmp_f = 0;
					}
					else
					{
						tmp_f = (1f - (tmp_f / size)) * 255f;
					}
					tmp_b = (byte)tmp_f;
					tmp_color.a = tmp_b;
					color_list.Add( tmp_color);
				}
#endif
#if true
				byte tmp_b;
				for( i0 = 0; i0 < vec_list.Count; ++i0)
				{
					tmp_f = vec_list[ i0].z;
					if( tmp_f < center_vec.z)
					{
						tmp_b = 0;
					}
					else
					{
						tmp_f = (tmp_f - center_vec.z) * 0.01f;
						if( tmp_f > 1f)
						{
							tmp_f = 1f;
						}
						tmp_b = (byte)(tmp_f * 255.1f);
					}
					tmp_color.a = tmp_b;
					color_list.Add( tmp_color);
				}
#endif
#if false
				/*! 特に何もしない頂点カラーの設定 */
				for( i0 = 0; i0 < vec_list.Count; ++i0)
				{
					color_list.Add( tmp_color);
				}
#endif
				obj = Object.Instantiate( createObj) as GameObject;
				obj.transform.parent = parent;
				mesh_script = obj.GetComponent<MeshCreator>();
				mesh_script.PolygonCreate( vec_list, tri_list, uv_list, color_list);
			}

			yield break;
		}

		/**
		 * 渡された座標が最低値、最大値を超えていないか調べて、超えている場合は補正する
		 */
		void MinMaxCheck( ref Vector3 vec, Vector3 min, Vector3 max)
		{
			if( min.x > vec.x)
			{
				vec.x = min.x;
			}
			else if( max.x < vec.x)
			{
				vec.x = max.x;
			}
			if( min.z > vec.z)
			{
				vec.z = min.z;
			}
			else if( max.z < vec.z)
			{
				vec.z = max.z;
			}
		}

		/**
		 * 外積を求める
		 *
		 * pos1からpos2のベクトルと、pos1からpos3のベクトルで外積を求めている
		 * Yの値しか必要ないので、Y値だけ計算して返すようにしている
		 */
		public static float CrossY( Vector3 pos1, Vector3 pos2, Vector3 pos3)
		{
			Vector3 vec1, vec2;
			float ret = 0f;

			vec1 = pos2 - pos1;
			vec2 = pos3 - pos1;

			//ret = vec1.y * vec2.z - vec1.z * vec2.y;
			ret = vec1.z * vec2.x - vec1.x * vec2.z;
			//ret = vec1.x * vec2.y - vec1.y * vec2.x;

			return ret;
		}

		public void SetObject( GameObject obj)
		{
			createObj = obj;
		}

		GameObject createObj;		/*! 生成するMeshCreatorが付いているオブジェクト */
	}
}
