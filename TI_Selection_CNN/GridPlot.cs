/*
 * Draw Grid
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2021.1
 * 
 */

using JAM.Algorithm.Visualization;
using JAM.Utilities;
using System;
using System.Drawing;

namespace JAM.Algorithm.Geometry
{
    /// <summary>
    /// Grid绘图
    /// </summary>
    public class GridPlot
    {
        public DoubleGrid Grid
        {
            get;
            set;
        }

        int m_ICount, m_JCount;
        ColorMapEnum m_ColorMapEnum = ColorMapEnum.Jet;//颜色映射类
        int m_PixelsOfCell = 1;//Cell的像素数目
        bool m_IsDrawGap = true;//是否绘制间隔
        bool m_IsRePic = false;//是否反色
        int m_Alpha = 255;//透明度
        int bWidth = 0;//有效图像的宽度
        int bHeight = 0;//有效图像的高度

        public Bitmap DrawGrid(int PixelsOfCell, ColorMapEnum ColorMap,
            bool IsDrawGap, bool IsRePic, int Alpha)
        {
            if (Grid.Dimension != DimensionEnum._2D)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            ColorMapEnum ColorMapEnum = ColorMap;
            m_PixelsOfCell = PixelsOfCell;
            m_ColorMapEnum = ColorMapEnum;
            m_IsDrawGap = IsDrawGap;
            m_IsRePic = IsRePic;
            m_Alpha = Alpha;

            if (m_IsDrawGap)
                return DrawGridWithGap();
            else
                return DrawGridWithoutGap();
        }

        //绘制Gap
        Bitmap DrawGridWithGap()
        {
            m_ICount = Grid.GridStructure.ICount;
            m_JCount = Grid.GridStructure.JCount;
            double min = Grid.Min().Value;
            double max = Grid.Max().Value;
            //根据Grid网格值的范围计算颜色表
            ColorMap mapper = new ColorMap(min, max, 16, 255, m_ColorMapEnum);
            //笛卡尔坐标系to屏幕坐标系（坐标转换）
            DoubleGrid reverse = Grid.ReverseUpDown2D().ConvertToDoubleGrid();

            #region Draw Plot

            bWidth = m_ICount * (m_PixelsOfCell + 1) + 1;
            bHeight = m_JCount * (m_PixelsOfCell + 1) + 1;
            Bitmap b = new Bitmap(bWidth, bHeight);
            using (Graphics g = Graphics.FromImage(b))
            {
                for (int j = 0; j < m_JCount; j++)//转换为图像
                {
                    for (int i = 0; i < m_ICount; i++)
                    {
                        SolidBrush brush = null;
                        double? cell = reverse.GetCell(new SpatialIndex(i, j));
                        if (cell == null)
                        {
                            brush = new SolidBrush(Color.FromArgb(m_Alpha, 250, 250, 250));
                        }
                        else
                        {
                            //颜色映射表获取对应值的颜色
                            Color color = mapper.MapValueToColor(cell.Value);
                            //颜色映射表获取对应值的颜色
                            brush = new SolidBrush(Color.FromArgb(m_Alpha, color.R, color.G, color.B));
                        }
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(i * (m_PixelsOfCell + 1) + 1, j * (m_PixelsOfCell + 1) + 1, m_PixelsOfCell, m_PixelsOfCell);
                        g.FillRectangle(brush, rect);
                        brush.Dispose();
                    }
                }
            }

            #endregion

            if (m_IsRePic)
                return ImageHelper.RePic(b, b.Width, b.Height);//反色处理
            else
                return b;

        }
        //不绘制Gap
        Bitmap DrawGridWithoutGap()
        {
            m_ICount = Grid.GridStructure.ICount;
            m_JCount = Grid.GridStructure.JCount;
            double min = Grid.Min().Value;
            double max = Grid.Max().Value;
            ColorMap ColorMap = new ColorMap(min, max, 16, 255, m_ColorMapEnum);//根据Grid网格值的范围计算颜色表
            DoubleGrid reverse = Grid.ReverseUpDown2D().ConvertToDoubleGrid();//笛卡尔坐标系to屏幕坐标系（坐标转换）

            #region Draw Plot

            bWidth = m_ICount * m_PixelsOfCell;
            bHeight = m_JCount * m_PixelsOfCell;
            Bitmap b = new Bitmap(bWidth, bHeight);

            using (Graphics g = Graphics.FromImage(b))
            {
                for (int j = 0; j < m_JCount; j++)//转换为图像
                {
                    for (int i = 0; i < m_ICount; i++)
                    {
                        SolidBrush brush = null;
                        var cell = reverse.GetCell(new SpatialIndex(i, j));
                        if (cell == null)
                        {
                            brush = new SolidBrush(Color.FromArgb(m_Alpha, 250, 250, 250));
                        }
                        else
                        {
                            //颜色映射表获取对应值的颜色
                            Color color = ColorMap.MapValueToColor(cell.Value);
                            brush = new SolidBrush(Color.FromArgb(m_Alpha, color.R, color.G, color.B));
                        }
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(i * m_PixelsOfCell, j * m_PixelsOfCell, m_PixelsOfCell, m_PixelsOfCell);
                        g.FillRectangle(brush, rect);
                        brush.Dispose();
                    }
                    
                }
            }

            #endregion

            if (m_IsRePic)
                return ImageHelper.RePic(b, b.Width, b.Height);//反色处理
            else
                return b;
        }
    }
}
