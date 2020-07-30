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
		 * @param pointList	ポリゴン作成情報に使用する繋がりポイントのリスト
		 * @param ofsetY		ポリゴン作成時のYの高さ
		 */
		public IEnumerator GroundPolygonCreate( Transform parent, List<FieldConnectPoint> pointList, Vector3 min, Vector3 max, float ofsetY = -0.1f)
		{
			int i0, i1, i2, i3, count;
			FieldConnectPoint currentPoint;
			var polygonTriangleVector = new Vector3[ 3];
			Vector3 tmp_vec, subVector;
			var addUv = Vector2.zero;
			float alphaPower, size;
			var vectorList = new List<Vector3>();
			var uvList = new List<Vector2>();
			var triangleList = new List<int>();
			var colorList = new List<Color32>();
			var addColor = new Color32(255,255,255,0);
			size = 50f;
			size = size * size;
			
			for( i0 = 0; i0 < pointList.Count; ++i0)
			{
				currentPoint = pointList[ i0];
				count = currentPoint.ConnectionList.Count;
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
						polygonTriangleVector[ 0] = new Vector3( currentPoint.Position.x, currentPoint.Position.y, currentPoint.Position.z);
						tmp_vec = currentPoint.ConnectionList[ i1].Position;
						polygonTriangleVector[ 1] = new Vector3( tmp_vec.x, tmp_vec.y, tmp_vec.z);
						tmp_vec = currentPoint.ConnectionList[ (i2 % count)].Position;
						polygonTriangleVector[ 2] = new Vector3( tmp_vec.x, tmp_vec.y, tmp_vec.z);
						float clossY = CrossY( polygonTriangleVector[ 0], polygonTriangleVector[ 1], polygonTriangleVector[ 2]);
						if( clossY < 0)
						{
							tmp_vec = polygonTriangleVector[ 1];
							polygonTriangleVector[ 1] = polygonTriangleVector[ 2];
							polygonTriangleVector[ 2] = tmp_vec;
						}
						for( i3 = 0; i3 < polygonTriangleVector.Length; ++i3)
						{
							MinMaxCheck( ref polygonTriangleVector[ i3], min, max);
							polygonTriangleVector[ i3].y = ofsetY;
							vectorList.Add( polygonTriangleVector[ i3]);
							addUv.x = polygonTriangleVector[ i3].x * 0.01f;
							addUv.y = polygonTriangleVector[ i3].z * 0.01f;
							uvList.Add( addUv);
						}
						/*! 対面のテクスチャも作る設定 */
						subVector = polygonTriangleVector[ 1] - polygonTriangleVector[ 0];
						polygonTriangleVector[ 0] = polygonTriangleVector[ 2] + subVector;
						tmp_vec = polygonTriangleVector[ 1];
						polygonTriangleVector[ 1] = polygonTriangleVector[ 2];
						polygonTriangleVector[ 2] = tmp_vec;
						for( i3 = 0; i3 < polygonTriangleVector.Length; ++i3)
						{
							MinMaxCheck( ref polygonTriangleVector[ i3], min, max);
							polygonTriangleVector[ i3].y = ofsetY;
							vectorList.Add( polygonTriangleVector[ i3]);
							addUv.x = polygonTriangleVector[ i3].x * 0.01f;
							addUv.y = polygonTriangleVector[ i3].z * 0.01f;
							uvList.Add( addUv);
						}
					}
				}
			}

			/*! 重ねて表示するテクスチャの座標をランダムに出す */
			var SystemRandom = new System.Random();
			var overwritePoint = new Vector3(0,0,0);
			overwritePoint.x = (float)SystemRandom.NextDouble() * 300f + 100f;
			overwritePoint.z = (float)SystemRandom.NextDouble() * 300f + 300f;
			if( createObj != null)
			{
				GameObject obj;
				for( i0 = 0; i0 < vectorList.Count; ++i0)
				{
					triangleList.Add( i0);
				}
#if false
				/*! 特定の座標周りだけテクスチャを上乗せする処理。
					頂点カラーの設定が上手くいっていないので、重なってるポリゴンの部分で上乗せ具合が違っててチラつく
				 */
				byte tmp_b;
				Vector3 sub_vec;
				for( i0 = 0; i0 < vectorList.Count; ++i0)
				{
					sub_vec = overwritePoint - vectorList[ i0];
					alphaPower = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
					if( alphaPower > size)
					{
						alphaPower = 0;
					}
					else
					{
						alphaPower = (1f - (alphaPower / size)) * 255f;
					}
					tmp_b = (byte)alphaPower;
					addColor.a = tmp_b;
					colorList.Add( addColor);
				}
#endif
#if true
				byte tmp_b;
				for( i0 = 0; i0 < vectorList.Count; ++i0)
				{
					alphaPower = vectorList[ i0].z;
					if( alphaPower < overwritePoint.z)
					{
						tmp_b = 0;
					}
					else
					{
						alphaPower = (alphaPower - overwritePoint.z) * 0.01f;
						if( alphaPower > 1f)
						{
							alphaPower = 1f;
						}
						tmp_b = (byte)(alphaPower * 255.1f);
					}
					addColor.a = tmp_b;
					colorList.Add( addColor);
				}
#endif
#if false
				/*! 特に何もしない頂点カラーの設定 */
				addColor.a = 0;
				for( i0 = 0; i0 < vectorList.Count; ++i0)
				{
					colorList.Add( addColor);
				}
#endif
				/* マップサイズのポリゴンを生成する（地面に穴が開いているような状態で生成される事があるため、そこを塞ぐために生成する） */
				addColor = new Color32(255,255,255,0);
				int index = vectorList.Count;
				for( i0 = 0; i0 < 4; ++i0)
				{
					switch( i0)
					{
						case 0:
						tmp_vec = new Vector3(min.x, ofsetY - 0.1f, max.z);
						break;
						case 1:
						tmp_vec = new Vector3(max.x, ofsetY - 0.1f, max.z);
						break;
						case 2:
						tmp_vec = new Vector3(max.x, ofsetY - 0.1f, min.z);
						break;
						case 3:
						tmp_vec = new Vector3(min.x, ofsetY - 0.1f, min.z);
						break;
						default:
						tmp_vec = new Vector3(min.x, ofsetY - 0.1f, max.z);
						break;
					}
					vectorList.Add(tmp_vec);
					addUv.x = tmp_vec.x * 0.01f;
					addUv.y = tmp_vec.z * 0.01f;
					uvList.Add(addUv);
					colorList.Add( addColor);
				}
				triangleList.Add( index + 0);	triangleList.Add( index + 1);	triangleList.Add( index + 2);
				triangleList.Add( index + 2);	triangleList.Add( index + 3);	triangleList.Add( index + 0);
				obj = Object.Instantiate( createObj) as GameObject;
				obj.transform.parent = parent;
				var meshScript = obj.GetComponent<MeshCreator>();
				meshScript.PolygonCreate( vectorList, triangleList, uvList, colorList);
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
