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
			var result = new List<Vector2>();
			List<Vector2> typeUv = GetSideUV();
			Vector2[] vec_tbl = new Vector2[ 2];
			float uv_x, uv_y;

			var standardPoint = new Vector2( typeUv[ 0].x + 0.125f, typeUv[ 0].y);

			switch( RoofTopType)
			{
			case 0:
				uv_x = 0f;		uv_y = 0.0625f;
				break;
			case 1:
				uv_x = 0.0625f;	uv_y = 0.0625f;
				break;
			case 2:
				uv_x = 0f;		uv_y = 0f;
				break;
			case 3:
				uv_x = 0.0625f;	uv_y = 0f;
				break;
			default:
				uv_x = 0f;		uv_y = 0.0625f;
				break;
			}

			vec_tbl[ 0] = new Vector2( uv_x, uv_y);
			vec_tbl[ 1] = new Vector2( uv_x + 0.0625f, uv_y + 0.0625f);

			result.Add(new Vector2( standardPoint.x + vec_tbl[ 0].x, standardPoint.y + vec_tbl[ 1].y));
			result.Add(new Vector2( standardPoint.x + vec_tbl[ 1].x, standardPoint.y + vec_tbl[ 1].y));
			result.Add(new Vector2( standardPoint.x + vec_tbl[ 1].x, standardPoint.y + vec_tbl[ 0].y));
			result.Add(new Vector2( standardPoint.x + vec_tbl[ 0].x, standardPoint.y + vec_tbl[ 0].y));

			return result;
		}

		/*! 建物の側面部分のUV座標をテクスチャの左上から時計回りで渡す */
		public List<Vector2> GetSideUV()
		{
			var result = new List<Vector2>();
			var surplus = ((int)TextureType % 8);
			var vectorX = Vector2.zero;
			var vectorY = Vector2.zero;

			switch( surplus)
			{
			case 0:
				vectorY.x = 0.875f;		vectorY.y = 1f;
				break;
			case 1:
				vectorY.x = 0.75f;		vectorY.y = 0.875f;
				break;
			case 2:
				vectorY.x = 0.625f;		vectorY.y = 0.75f;
				break;
			case 3:
				vectorY.x = 0.5f;		vectorY.y = 0.625f;
				break;
			case 4:
				vectorY.x = 0.375f;		vectorY.y = 0.5f;
				break;
			case 5:
				vectorY.x = 0.25f;		vectorY.y = 0.375f;
				break;
			case 6:
				vectorY.x = 0.125f;		vectorY.y = 0.25f;
				break;
			case 7:
				vectorY.x = 0f;			vectorY.y = 0.125f;
				break;
			default:
				vectorY.x = 0.875f;		vectorY.y = 1f;
				break;
			}

			if( (int)BuildingType.kBuildingB04 >= (int)TextureType)
			{
				vectorX.x = 0f;		vectorX.y = 0.125f;
			}
			else if( (int)BuildingType.kBuildingD04 >= (int)TextureType)
			{
				vectorX.x = 0.25f;	vectorX.y = 0.375f;
			}
			else if( (int)BuildingType.kBuildingF04 >= (int)TextureType)
			{
				vectorX.x = 0.5f;	vectorX.y = 0.625f;
			}
			else if( (int)BuildingType.kBuildingH04 >= (int)TextureType)
			{
				vectorX.x = 0.75f;	vectorX.y = 0.875f;
			}

			result.Add(new Vector2( vectorX.x, vectorY.y));
			result.Add(new Vector2( vectorX.y, vectorY.y));
			result.Add(new Vector2( vectorX.y, vectorY.x));
			result.Add(new Vector2( vectorX.x, vectorY.x));

			return result;
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

			kBuildingA01 = 0,
			kBuildingA02,
			kBuildingA03,
			kBuildingA04,
			kBuildingB01,
			kBuildingB02,
			kBuildingB03,
			kBuildingB04,
			kBuildingC01,
			kBuildingC02,
			kBuildingC03,
			kBuildingC04,
			kBuildingD01,
			kBuildingD02,
			kBuildingD03,
			kBuildingD04,
			kBuildingE01,
			kBuildingE02,
			kBuildingE03,
			kBuildingE04,
			kBuildingF01,
			kBuildingF02,
			kBuildingF03,
			kBuildingF04,
			kBuildingG01,
			kBuildingG02,
			kBuildingG03,
			kBuildingG04,
			kBuildingH01,
			kBuildingH02,
			kBuildingH03,
			kBuildingH04,
		}

		/*! ビルの高さ */
		public float BuildingHeight
		{
			get;
			private set;
		}
	}
}
