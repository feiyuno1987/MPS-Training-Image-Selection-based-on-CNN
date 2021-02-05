/*     
*     GridStructure
*
* Version: 1.0
* Author:  Siyu Yu(573315294@qq.com)
* Date:    2017.10
* 
*/

using System;

namespace JAM.Algorithm.Geometry
{
    /// <summary>
    /// 网格体结构
    /// 喻思羽 2017.10
    /// </summary>
    [Serializable]
    public class GridStructure
    {
        public DimensionEnum Dimension
        {
            get
            {
                if (this.KCount == 1)//二维
                {
                    return DimensionEnum._2D;
                }
                else//三维
                {
                    return DimensionEnum._3D;
                }
            }
        }

        //I,J,K方向上cell的数目
        public int ICount { get; internal set; } = 0;
        public int JCount { get; internal set; } = 0;
        public int KCount { get; internal set; } = 0;

        //I,J,K方向上cell的尺寸
        public double ISize { get; internal set; } = 1.0;
        public double JSize { get; internal set; } = 1.0;
        public double KSize { get; internal set; } = 1.0;

        //I方向上范围，其中m_IExtent=m_ICount*m_ISize
        public double IExtent { get; internal set; } = 0;
        //J方向上范围，其中m_JExtent=m_JCount*m_JSize
        public double JExtent { get; internal set; } = 0;
        //K方向上范围，其中m_KExtent=m_KCount*m_KSize
        public double KExtent { get; internal set; } = 0;

        //原点坐标X,Y,Z
        public double OriginCellX { get; internal set; } = 0.5;
        public double OriginCellY { get; internal set; } = 0.5;
        public double OriginCellZ { get; internal set; } = 0.5;

        #region 构造函数

        public GridStructure()
        {

        }

        public GridStructure(double ISize, double JSize,
            int ICount, int JCount,
            double OriginCellX, double OriginCellY)
        {
            this.ICount = ICount;
            this.ISize = ISize;
            this.IExtent = ICount * ISize;
            this.OriginCellX = OriginCellX;

            this.JCount = JCount;
            this.JSize = JSize;
            this.JExtent = JCount * JSize;
            this.OriginCellY = OriginCellY;

            //2维情况，给第3维直接赋值，保证计算后续正确计算
            this.KCount = 1;
            this.KSize = 1.0;
            this.KExtent = 1.0;
            this.OriginCellZ = 0.5;
        }

        public GridStructure(double ISize, double JSize,
            double IExtent, double JExtent,
            double OriginCellX, double OriginCellY)
        {
            this.ISize = ISize;
            this.ICount = (int)Math.Ceiling(IExtent / ISize);
            //根据对应的CellCount，更新Extent的数值
            this.IExtent = this.ICount * ISize;
            this.OriginCellX = OriginCellX;

            this.JSize = JSize;
            this.JCount = (int)Math.Ceiling(JExtent / JSize);
            //根据对应的CellCount，更新Extent的数值
            this.JExtent = this.JCount * JSize;
            this.OriginCellY = OriginCellY;

            //2维情况，给第3维直接赋值，保证计算后续正确计算
            this.KCount = 1;
            this.KSize = 1.0;
            this.KExtent = 1.0;
            this.OriginCellZ = 0.5;
        }

        public GridStructure(double ISize, double JSize, double KSize,
            int ICount, int JCount, int KCount,
            double OriginCellX, double OriginCellY, double OriginCellZ)
        {
            this.ICount = ICount;
            this.ISize = ISize;
            this.IExtent = ICount * ISize;
            this.OriginCellX = OriginCellX;

            this.JCount = JCount;
            this.JSize = JSize;
            this.JExtent = JCount * JSize;
            this.OriginCellY = OriginCellY;

            this.KCount = KCount;
            this.KSize = KSize;
            this.KExtent = KCount * KSize;
            this.OriginCellZ = OriginCellZ;
        }

        public GridStructure(double ISize, double JSize, double KSize,
            double IExtent, double JExtent, double KExtent,
            double OriginCellX, double OriginCellY, double OriginCellZ)
        {
            this.ISize = ISize;
            this.ICount = (int)Math.Ceiling(IExtent / ISize);
            //根据对应的CellCount，更新Extent的数值
            this.IExtent = this.ICount * ISize;
            this.OriginCellX = OriginCellX;

            this.JSize = JSize;
            this.JCount = (int)Math.Ceiling(JExtent / JSize);
            //根据对应的CellCount，更新Extent的数值
            this.JExtent = this.JCount * JSize;
            this.OriginCellY = OriginCellY;

            this.KSize = KSize;
            this.KCount = (int)Math.Ceiling(KExtent / KSize);
            //根据对应的CellCount，更新Extent的数值
            this.KExtent = this.KCount * KSize;
            this.OriginCellZ = OriginCellZ;
        }

        #endregion

        public static GridStructure CreateSimple(
            int ICount, int JCount)
        {
            return new GridStructure(1.0, 1.0, ICount, JCount, 0.5, 0.5);
        }

        public static GridStructure CreateSimple(
            int ICount, int JCount, int KCount)
        {
            return new GridStructure(1.0, 1.0, 1.0,
                ICount, JCount, KCount,
                0.5, 0.5, 0.5);
        }

        public bool Equals(GridStructure gs)
        {
            if (gs.Dimension != this.Dimension) return false;

            if (gs.ICount != this.ICount) return false;
            if (gs.JCount != this.JCount) return false;
            if (gs.KCount != this.KCount) return false;
            if (gs.ISize != this.ISize) return false;
            if (gs.JSize != this.JSize) return false;
            if (gs.KSize != this.KSize) return false;
            if (gs.OriginCellX != this.OriginCellX) return false;
            if (gs.OriginCellY != this.OriginCellY) return false;
            if (gs.OriginCellZ != this.OriginCellZ) return false;

            return true;
        }

        public string ToMyString()
        {
            string s = string.Format("{0} {1} {2},{3} {4} {5},{6} {7} {8},{9}",
                ICount, JCount, KCount, ISize, JSize, KSize, OriginCellX, OriginCellY, OriginCellZ,
                Dimension.ToString());
            return s;
        }

        public static GridStructure FromMyString(string str)
        {
            GridStructure gs = null;
            string[] s = str.Split(new char[] { ',' });
            string[] s1 = s[0].Split(new char[] { ' ' });//ICount, JCount, KCount
            string[] s2 = s[1].Split(new char[] { ' ' });//ISize, JSize, KSize
            string[] s3 = s[2].Split(new char[] { ' ' });//OriginCellX, OriginCellY, OriginCellZ
            string s4 = s[3];//Dimension
            int ICount1 = int.Parse(s1[0]);
            int JCount1 = int.Parse(s1[1]);
            int KCount1 = int.Parse(s1[2]);
            double ISize1 = int.Parse(s2[0]);
            double JSize1 = int.Parse(s2[1]);
            double KSize1 = int.Parse(s2[2]);
            double OriginCellX1 = int.Parse(s3[0]);
            double OriginCellY1 = int.Parse(s3[1]);
            double OriginCellZ1 = int.Parse(s3[2]);
            DimensionEnum Dimension = (DimensionEnum)Enum.Parse(typeof(DimensionEnum), s4, true);
            if (Dimension == DimensionEnum._2D)
                gs = new GridStructure(
                    ISize1, JSize1,
                    ICount1, JCount1,
                    OriginCellX1, OriginCellY1);
            if (Dimension == DimensionEnum._3D)
                gs = new GridStructure(
                    ISize1, JSize1, KSize1,
                    ICount1, JCount1, KCount1,
                    OriginCellX1, OriginCellY1, OriginCellZ1);
            return gs;
        }
    }

}
