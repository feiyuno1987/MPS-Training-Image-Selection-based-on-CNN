/*
 * SpatialIndex
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2021.1
 * 
 */

using System;

namespace JAM.Algorithm.Geometry
{
    [Serializable]
    public class SpatialIndex
    {
        public DimensionEnum Dimension { get; internal set; } = DimensionEnum._1D;

        public int I { get; internal set; }
        public int J { get; internal set; }
        public int K { get; internal set; }
        /// <summary>
        /// 唯一标识码
        /// </summary>
        public string UniCode { get; internal set; }

        /// <summary>
        /// SpatialIndex附带其他信息
        /// </summary>
        public object Tag
        {
            get; set;
        } = null;

        /// <summary>
        /// SpatialIndex的值
        /// </summary>
        public double Value
        {
            get; set;
        } = double.NaN;

        #region 构造函数

        /// <summary>
        /// SpatialIndex构造函数，没有赋值，不能直接用
        /// </summary>
        public SpatialIndex()
        {
        }
        public SpatialIndex(int I, int J)
        {
            this.I = I;
            this.J = J;
            this.Dimension = DimensionEnum._2D;
            UniCode = string.Format("_2D_{0}_{1}", I, J);
        }
        public SpatialIndex(int I, int J, int K)
        {
            this.I = I;
            this.J = J;
            this.K = K;
            this.Dimension = DimensionEnum._3D;
            UniCode = string.Format("_3D_{0}_{1}_{2}", I, J, K);
        }

        #endregion

        #region 实例函数

        #region offset偏移计算

        /// <summary>
        /// 偏移(2D & 3D)
        /// </summary>
        /// <param name="Delta"></param>
        /// <returns></returns>
        public SpatialIndex Offset(SpatialIndex Delta)
        {
            if (Delta.Dimension == DimensionEnum._2D)
            {
                return Offset(Delta.I, Delta.J);
            }
            if (Delta.Dimension == DimensionEnum._3D)
            {
                return Offset(Delta.I, Delta.J, Delta.K);
            }
            return null;
        }
        /// <summary>
        /// 偏移(2D)
        /// </summary>
        /// <param name="DeltaI"></param>
        /// <param name="DeltaJ"></param>
        /// <returns></returns>
        public SpatialIndex Offset(int DeltaI, int DeltaJ)
        {
            return new SpatialIndex(I + DeltaI, J + DeltaJ);
        }
        /// <summary>
        /// 偏移(3D)
        /// </summary>
        /// <param name="DeltaI"></param>
        /// <param name="DeltaJ"></param>
        /// <param name="DeltaK"></param>
        /// <returns></returns>
        public SpatialIndex Offset(int DeltaI, int DeltaJ, int DeltaK)
        {
            return new SpatialIndex(I + DeltaI, J + DeltaJ, K + DeltaK);
        }

        #endregion

        /// <summary>
        /// Convert to String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Dimension == DimensionEnum._2D)
                return string.Format("I={0}, J={1},K={2}", I, J, "NAN");
            if (Dimension == DimensionEnum._3D)
                return string.Format("I={0}, J={1}, K={2}", I, J, K);
            else
                return null;
        }
        /// <summary>
        /// 转换为极简字符串
        /// </summary>
        /// <returns></returns>
        public string ToShortString()
        {
            if (Dimension == DimensionEnum._2D)
                return I + ":" + J + ":" + "NAN";
            if (Dimension == DimensionEnum._3D)
                return I + ":" + J + ":" + K;
            return null;
        }
        /// <summary>
        /// 由极简字符串构建实例
        /// </summary>
        /// <param name="IndexShortString"></param>
        public void FromShortString(string IndexShortString)
        {
            string[] tempArray = IndexShortString.Split(':');
            this.I = int.Parse(tempArray[0]);
            this.J = int.Parse(tempArray[1]);
            if (tempArray[2] != "NAN")
                this.K = int.Parse(tempArray[2]);
        }

        public bool Equals(SpatialIndex Index)
        {
            if (Index == null) return false;
            if (Index.Dimension != Dimension)
                throw new Exception("异常:维度问题");

            if (Dimension == DimensionEnum._2D)
                return (I == Index.I && J == Index.J);
            else
                return (I == Index.I && J == Index.J && K == Index.K);
        }


        //重载方法
        public override bool Equals(object o)
        {
            if (!(o is SpatialIndex))
            {
                return false;
            }

            return I == ((SpatialIndex)o).I &&
                   J == ((SpatialIndex)o).J &&
                   K == ((SpatialIndex)o).K;
        }
        public override int GetHashCode()
        {
            return I.GetHashCode() ^ J.GetHashCode() ^ K.GetHashCode();
        }

        #endregion

        #region 静态函数

        public static SpatialIndex operator +(SpatialIndex Index1, SpatialIndex Index2)
        {
            if (Index1.Dimension != Index2.Dimension)
                throw new Exception("异常:维度问题");

            if (Index1.Dimension == DimensionEnum._2D)
                return new SpatialIndex(Index1.I + Index2.I, Index1.J + Index2.J);
            if (Index1.Dimension == DimensionEnum._3D)
                return new SpatialIndex(Index1.I + Index2.I, Index1.J + Index2.J, Index1.K + Index2.K);
            else
                return null;
        }
        public static SpatialIndex operator -(SpatialIndex Index1, SpatialIndex Index2)
        {
            if (Index1.Dimension != Index2.Dimension)
                throw new Exception("异常:维度问题");

            if (Index1.Dimension == DimensionEnum._2D)
                return new SpatialIndex(Index1.I - Index2.I, Index1.J - Index2.J);
            if (Index1.Dimension == DimensionEnum._3D)
                return new SpatialIndex(Index1.I - Index2.I, Index1.J - Index2.J, Index1.K - Index2.K);
            else
                return null;
        }
        public static bool operator ==(SpatialIndex Index1, SpatialIndex Index2)
        {
            return Equals(Index1, Index2);
        }
        public static bool operator !=(SpatialIndex Index1, SpatialIndex Index2)
        {
            return !Equals(Index1, Index2);
        }

        #endregion
    }
}
