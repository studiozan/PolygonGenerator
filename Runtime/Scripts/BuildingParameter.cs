using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonGenerator
{
	public class BuildingParameter
	{
		/*! コンストラクタ時に座標リストを設定している */
		public BuildingParameter( List<Vector3> posList)
		{
			SetPositionList( posList);
		}

		/*! 座標リストの設定 */
		public void SetPositionList( List<Vector3> posList)
		{
			/*! この時に座標の順番が時計回りになるように入れ替えた方が良いかも */
			PositionList = posList;

			Vector3 tmp_vec;
			float tmp_f;
			bool flg = true;
			int i0, count = 2;

			while( count > 0)
			{
				flg = false;
				for( i0 = 0; i0 < 2; i0++)
				{
					tmp_f = MapGroundPolygonCreator.CrossY( PositionList[ 0], PositionList[ 1 + i0], PositionList[ 2 + i0]);
					if( tmp_f < 0f)
					{
						tmp_vec = PositionList[ 1 + i0];
						PositionList[ 1 + i0] = PositionList[ 2 + i0];
						PositionList[ 2 + i0] = tmp_vec;
						flg = true;
					}
				}
				count--;
				if( flg == false)
				{
					count = 0;
				}
			}
		}

		/*! ビルのタイプの設定 */
		public void SetBuildingType( BuildingType type, int rooftop = 0)
		{
			TextureType = type;
			RoofTopType = rooftop;
		}

		/*! ビルの高さを設定する */
		public void SetBuildingHeight( float height)
		{
			BuildingHeight = height;
		}

		/*! 建物の屋上部分のUV座標をテクスチャの左上から時計回りで渡す */
		public List<Vector2> GetRoofTopUV()
		{
			List<Vector2> ret_list = new List<Vector2>();
			Vector2[] vec_tbl = new Vector2[ 2];
			float uv_x, uv_y;

			switch( RoofTopType)
			{
			case 0:
				uv_x = 0f;		uv_y = 0.125f;
				break;
			case 1:
				uv_x = 0.125f;	uv_y = 0.125f;
				break;
			case 2:
				uv_x = 0f;		uv_y = 0f;
				break;
			case 3:
				uv_x = 0.125f;	uv_y = 0f;
				break;
			default:
				uv_x = 0f;		uv_y = 0.125f;
				break;
			}

			switch( TextureType)
			{
			case BuildingType.kBuildingA:
				vec_tbl[ 0] = new Vector2( uv_x, uv_y);
				vec_tbl[ 1] = new Vector2( uv_x + 0.125f, uv_y + 0.125f);
				break;
			case BuildingType.kBuildingB:
				vec_tbl[ 0] = new Vector2( uv_x + 0.75f, uv_y);
				vec_tbl[ 1] = new Vector2( uv_x + 0.875f, uv_y + 0.125f);
				break;
			case BuildingType.kBuildingC:
				vec_tbl[ 0] = new Vector2( uv_x + 0.5f, uv_y);
				vec_tbl[ 1] = new Vector2( uv_x + 0.625f, uv_y + 0.125f);
				break;
			case BuildingType.kBuildingD:
				/*! 屋上のタイプが1つしかないので、uv_yは使わない */
				vec_tbl[ 0] = new Vector2( uv_x + 0.25f, 0f);
				vec_tbl[ 1] = new Vector2( uv_x + 0.5f, 0.25f);
				break;
			default:
				vec_tbl[ 0] = new Vector2( uv_x, uv_y);
				vec_tbl[ 1] = new Vector2( uv_x + 0.125f, uv_y + 0.125f);
				break;
			}

			ret_list.Add(new Vector2( vec_tbl[ 0].x, vec_tbl[ 1].y));
			ret_list.Add(new Vector2( vec_tbl[ 1].x, vec_tbl[ 1].y));
			ret_list.Add(new Vector2( vec_tbl[ 1].x, vec_tbl[ 0].y));
			ret_list.Add(new Vector2( vec_tbl[ 0].x, vec_tbl[ 0].y));

			return ret_list;
		}

		/*! 建物の側面部分のUV座標をテクスチャの左上から時計回りで渡す */
		public List<Vector2> GetSideUV()
		{
			List<Vector2> ret_list = new List<Vector2>();
			Vector2 tmp_vec = Vector2.zero;

			switch( TextureType)
			{
			case BuildingType.kBuildingA:
				tmp_vec.x = 0.75f;		tmp_vec.y = 1f;
				break;
			case BuildingType.kBuildingB:
				tmp_vec.x = 0.5f;		tmp_vec.y = 0.75f;
				break;
			case BuildingType.kBuildingC:
				tmp_vec.x = 0.25f;		tmp_vec.y = 0.5f;
				break;
			case BuildingType.kBuildingD:
				tmp_vec.x = 0.5f;		tmp_vec.y = 0.75f;
				break;
			default:
				tmp_vec.x = 0.75f;		tmp_vec.y = 1f;
				break;
			}

			ret_list.Add(new Vector2( 0f, tmp_vec.y));
			ret_list.Add(new Vector2( 0.25f, tmp_vec.y));
			ret_list.Add(new Vector2( 0.25f, tmp_vec.x));
			ret_list.Add(new Vector2( 0f, tmp_vec.x));

			return ret_list;
		}

		/*! 座標リスト */
		public List<Vector3> PositionList
		{
			get;
			private set;
		}

		/*! 屋上のタイプ（0～3）*/
		public int RoofTopType
		{
			get;
			private set;
		}

		/*! 表示するテクスチャのタイプ（これでUVが決まる） */
		public BuildingType TextureType
		{
			get;
			private set;
		}

		public enum BuildingType
		{
			kBuildingA = 0,
			kBuildingB,
			kBuildingC,
			kBuildingD,
		}

		/*! ビルの高さ */
		public float BuildingHeight
		{
			get;
			private set;
		}
	}
}
