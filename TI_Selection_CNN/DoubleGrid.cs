/*
 * Grid class
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2017
 * 
 */

using System;
using System.Drawing;
using System.Linq;

namespace JAM.Algorithm.Geometry
{
    [Serializable]
    public class DoubleGrid : baseGrid<double?>
    {
        #region 统计量

        /// <summary>
        /// 最大值
        /// </summary>
        public override double? Max()
        {
            return m_buffer.Max();
        }

        /// <summary>
        /// 最小值
        /// </summary>
        public override double? Min()
        {
            return m_buffer.Min();
        }

        /// <summary>
        /// 均值
        /// </summary>
        public double Average
        {
            get
            {
                return m_buffer.Average(a => a.Value);
            }
        }

        /// <summary>
        /// 数据范围 = 最大值 - 最小值
        /// </summary>
        public double Range
        {
            get
            {
                return Max().Value - Min().Value;
            }
        }

        #endregion

        public DoubleGrid(GridStructure gs) : base(gs)
        {

        }

        /// <summary>
        /// 名称：双线性插值方法(2D重采样)
        /// 作用：对于一个目的像素，设置坐标通过反向变换得到的浮点坐标为(i+u,j+v)，其中i、j均为非负整数，
        /// u、v为[0,1)区间的浮点数，则这个像素得值 f(i+u,j+v) 可由原图像中坐标为 (i,j)、(i+1,j)、(i,j+1)、
        /// (i+1,j+1)所对应的周围四个像素的值决定，即：f(i+u, j+v) = (1-u)(1-v)f(i, j) + (1-u)vf(i, j+1) + 
        /// u(1-v)f(i+1, j) + uvf(i+1, j+1)。 其中f(i, j)表示源图像(i, j)处的的像素值，以此类推，
        /// 这就是双线性内插值法。双线性内插值法计算量大，但缩放后图像质量高，不会出现像素值不连续的的情况。
        /// 由于双线性插值具有低通滤波器的性质，使高频分量受损，所以可能会使图像轮廓在一定程度上变得模糊
        /// 作者：喻思羽
        /// 编写时间：2015-11-12
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DoubleGrid BilinearInterpolationResample(int desICount, int desJCount)
        {
            //注意的是preISize需要根据情况修改
            DoubleGrid desGrid = SimpleGrid(desICount, desJCount) as DoubleGrid;

            int preI, preJ;
            //destination(目的) ——> previous(原始) 的反向坐标映射
            for (int desJ = 0; desJ < desJCount; desJ++)
            {
                for (int desI = 0; desI < desICount; desI++)
                {
                    preI = (int)(desI * ICount / desICount);
                    preJ = (int)(desJ * JCount / desJCount);
                    double newCell = BilinearInterpolation(preI, preJ);
                    desGrid.SetCell(desI, desJ, newCell);//取样赋值
                }
            }

            return desGrid;
        }
        //参考资料——数字图像处理与机器视觉P120
        double BilinearInterpolation(int I, int J)
        {
            int I1, I2, J1, J2;//4个最邻近像素的坐标(i1,j1),(i2,j1),(i1,j2),(i2,j2)
            double f1, f2, f3, f4;//4个最邻近像素值
            double f12, f34;//2个插值中间值

            double epsilon = 0.0001;
            //计算四个最邻近像素的坐标
            I1 = I;
            I2 = I1 + 1;
            J1 = J;
            J2 = J1 + 1;

            int nHeight = ICount;
            int nWidth = JCount;
            if (!IsIndexValid(I, J)) return -1;
            else
            {
                //要计算的点在图像右边缘上
                if (Math.Abs(I - nWidth + 1) <= epsilon)
                {
                    //要计算的点正好是图像最右下角的那一个像素，直接返回该点像素值
                    if (Math.Abs(J - nHeight + 1) <= epsilon)
                    {
                        f1 = GetCell(I, J).Value;
                        return f1;
                    }
                    //在图像右边缘上且不是最后一点，直接一次插值即可
                    else
                    {
                        f1 = GetCell(I1, J1).Value;
                        f3 = GetCell(I1, J2).Value;
                        return f1 + (J - J1) * (f3 - f1);
                    }
                }
                //要计算的点在图像下边缘上且不是最后一点，直接一次插值即可
                else if (Math.Abs(J - nHeight + 1) <= epsilon)
                {
                    f1 = GetCell(I1, J1).Value;
                    f2 = GetCell(I2, J1).Value;
                    return f1 + (I - I1) * (f2 - f1);
                }
                else
                {
                    f1 = GetCell(I1, J1).Value;
                    f2 = GetCell(I2, J1).Value;
                    f3 = GetCell(I1, J2).Value;
                    f4 = GetCell(I2, J2).Value;
                    f12 = f1 + (I - I1) * (f2 - f1);
                    f34 = f3 + (I - I1) * (f4 - f3);
                    return f12 + (J - J1) * (f34 - f12);
                }
            }
        }

        public override double Distance(baseGrid<double?> other, int distType)
        {
            //如果Vector1和Vector2的元素数量不同，提示异常
            if (Count != other.Count)
                throw new Exception("长度不同!");
            //距离值
            double _d = 0;

            if (distType == 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Buffer[i].HasValue && other.Buffer[i].HasValue)
                    {
                        _d += Math.Abs(Buffer[i].Value - other.Buffer[i].Value);
                    }
                }
                return _d;
            }
            if (distType == 1)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Buffer[i].HasValue && other.Buffer[i].HasValue)
                    {
                        _d += Math.Pow(Buffer[i].Value - other.Buffer[i].Value, 2);
                    }
                }
                return Math.Sqrt(_d);
            }
            return -1;
        }

        /// <summary>
        /// 根据索引的界限提取区域部分网格(2D)
        /// </summary>
        /// <param name="I1"></param>
        /// <param name="I2"></param>
        /// <param name="J1"></param>
        /// <param name="J2"></param>
        /// <returns></returns>
        public new DoubleGrid GetRegionByRange(int I1, int I2, int J1, int J2)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I1 >= I2)
                throw new Exception(MyExceptions.Geometry_IndexException);
            if (J1 >= J2)
                throw new Exception(MyExceptions.Geometry_IndexException);
            if (I1 >= ICount || I2 <= -1)
                throw new Exception(MyExceptions.Geometry_IndexException);
            if (J1 >= JCount || J2 <= -1)
                throw new Exception(MyExceptions.Geometry_IndexException);

            int newICount = I2 - I1 + 1;
            int newJCount = J2 - J1 + 1;

            //创建一个新网格对象
            GridStructure gs = new GridStructure(ISize, JSize, newICount, newJCount, 0, 0);
            DoubleGrid newGrid = new DoubleGrid(gs);

            for (int j = J1; j <= J2; j++)
            {
                for (int i = I1; i <= I2; i++)
                {
                    var Cell = GetCell(i, j);

                    int ii = i - I1;
                    int jj = j - J1;

                    newGrid.SetCell(ii, jj, Cell);
                }
            }

            return newGrid;
        }
        /// <summary>
        /// 根据索引的界限提取区域部分网格(3D)
        /// </summary>
        /// <param name="I1"></param>
        /// <param name="I2"></param>
        /// <param name="J1"></param>
        /// <param name="J2"></param>
        /// <param name="K1"></param>
        /// <param name="K2"></param>
        /// <returns></returns>
        public new DoubleGrid GetRegionByRange(int I1, int I2, int J1, int J2, int K1, int K2)
        {
            if (DimensionEnum._2D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I1 >= I2) throw new Exception(MyExceptions.Geometry_IndexException);
            if (J1 >= J2) throw new Exception(MyExceptions.Geometry_IndexException);
            if (K1 == 0 && K2 == 0)//二维情形
            {
            }
            else//三维情形
            {
                if (K1 >= K2) throw new Exception(MyExceptions.Geometry_IndexException);
            }
            if (I1 >= ICount || I2 <= -1)
                throw new Exception(MyExceptions.Geometry_IndexException);
            if (J1 >= JCount || J2 <= -1)
                throw new Exception(MyExceptions.Geometry_IndexException);
            if (K1 >= ICount || K2 <= -1)
                throw new Exception(MyExceptions.Geometry_IndexException);

            //计算有效区间（有效区间可能不等于输入区间）
            I1 = Math.Max(I1, 0);
            I2 = Math.Min(I2, ICount - 1);
            J1 = Math.Max(J1, 0);
            J2 = Math.Min(J2, JCount - 1);
            K1 = Math.Max(K1, 0);
            K2 = Math.Min(K2, KCount - 1);

            int newICount = I2 - I1 + 1;
            int newJCount = J2 - J1 + 1;
            int newKCount = K2 - K1 + 1;
            //创建一个新网格对象
            GridStructure gs = new GridStructure(ISize, JSize, KSize, newICount, newJCount, newKCount, 0, 0, 0);
            DoubleGrid newGrid = new DoubleGrid(gs);

            for (int k = K1; k <= K2; k++)
            {
                for (int j = J1; j <= J2; j++)
                {
                    for (int i = I1; i <= I2; i++)
                    {
                        var Cell = GetCell(i, j, k);

                        int ii = i - I1;
                        int jj = j - J1;
                        int kk = k - K1;
                        newGrid.SetCell(ii, jj, kk, Cell);
                    }
                }
            }

            return newGrid;
        }
        /// <summary>
        /// 根据中心索引和半径提取区域分布网格(2D)
        /// </summary>
        /// <param name="I"></param>
        /// <param name="IRadius"></param>
        /// <param name="J"></param>
        /// <param name="JRadius"></param>
        /// <returns></returns>
        public new DoubleGrid GetRegionByCenter(int I, int IRadius, int J, int JRadius)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);
            int I1 = I - IRadius;
            int I2 = I + IRadius;
            int J1 = J - JRadius;
            int J2 = J + JRadius;
            return GetRegionByRange(I1, I2, J1, J2);
        }
        /// <summary>
        /// 根据中心索引和半径提取区域分布网格(3D)
        /// </summary>
        /// <param name="I"></param>
        /// <param name="IRadius"></param>
        /// <param name="J"></param>
        /// <param name="JRadius"></param>
        /// <param name="K"></param>
        /// <param name="KRadius"></param>
        /// <returns></returns>
        public new DoubleGrid GetRegionByCenter(int I, int IRadius, int J, int JRadius, int K, int KRadius)
        {
            if (DimensionEnum._2D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            int I1 = 0, I2 = 0;
            int J1 = 0, J2 = 0;
            int K1 = 0, K2 = 0;

            I1 = I - IRadius;
            I2 = I + IRadius;
            J1 = J - JRadius;
            J2 = J + JRadius;

            K1 = K - KRadius;
            K2 = K + KRadius;

            return GetRegionByRange(I1, I2, J1, J2, K1, K2);
        }

        #region 2D专属函数

        public static DoubleGrid ReadFromArray(double?[,] array)
        {
            /*
            double?[,] arr3 = { 
                                { 1, 2, 3 }, 
                                { 4, 5, 6 },
                                { 7, 8, 9 }
                                };
             */
            DoubleGrid grid = new DoubleGrid(
                GridStructure.CreateSimple(array.GetLength(0), array.GetLength(1)));
            for (int j = 0; j < grid.JCount; j++)
            {
                for (int i = 0; i < grid.ICount; i++)
                {
                    grid.SetCell(i, j, array[i, j]);
                }
            }

            return grid;
        }

        public static double?[,] WriteToArray(DoubleGrid grid)
        {
            double?[,] array = new double?[grid.ICount, grid.JCount];
            for (int j = 0; j < grid.JCount; j++)
            {
                for (int i = 0; i < grid.ICount; i++)
                {
                    array[i, j] = grid.GetCell(i, j);
                }
            }
            return array;
        }

        public static DoubleGrid ReadFromGrayImage(Bitmap bitmap)
        {
            DoubleGrid grid = new DoubleGrid(
                GridStructure.CreateSimple(bitmap.Width, bitmap.Height));
            for (int j = 0; j < grid.JCount; j++)
            {
                for (int i = 0; i < grid.ICount; i++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    grid.SetCell(i, j, color.R);
                }
            }

            return grid;
        }

        public DoubleGrid pyrDown()
        {
            double?[,] kernel_array = {
                                {1,  4,  7,  4,  1},
                                {4, 16, 26, 16,  4},
                                {7, 26, 41, 26,  7},
                                {4, 16, 26, 16,  4},
                                {1,  4,  7,  4,  1}
                                };
            DoubleGrid grid = new DoubleGrid(GridStructure);
            DoubleGrid kernel = ReadFromArray(kernel_array);

            for (int j = 0; j < JCount; j++)
            {
                for (int i = 0; i < ICount; i++)
                {
                    DoubleGrid g = GetRegionByCenter(i, 2, j, 2);

                    double sum = 0;
                    //
                    for (int j_kernel = 0; j_kernel < 5; j_kernel++)
                    {
                        for (int i_kernel = 0; i_kernel < 5; i_kernel++)
                        {
                            double v = g.GetCell(i_kernel, j_kernel) == null ?
                                0 : g.GetCell(i_kernel, j_kernel).Value;
                            sum += v * kernel.GetCell(i_kernel, j_kernel).Value;
                        }
                    }
                    grid.SetCell(i, j, sum / 273d);
                }
            }

            int gap = 1;
            int Increment = gap + 1;//增量

            int desICount = 0, desJCount = 0;
            for (int preJ = 0; preJ < grid.JCount; preJ += Increment)
                desJCount += 1;
            for (int preI = 0; preI < grid.ICount; preI += Increment)
                desICount += 1;

            GridStructure gs = GridStructure.CreateSimple(desICount, desJCount);
            DoubleGrid resample = new DoubleGrid(gs);

            //destination(目的) ——> previous(原始) 的反向坐标映射
            for (int preJ = 0; preJ < grid.ICount; preJ += Increment)
            {
                for (int preI = 0; preI < grid.JCount; preI += Increment)
                {
                    //取样赋值
                    resample.SetCell(preI / Increment, preJ / Increment, grid.GetCell(preI, preJ));
                }
            }

            return resample;
        }

        public DoubleGrid pyrUp()
        {
            int gap = 1;
            int Increment = gap + 1;//增量

            int desICount = 0, desJCount = 0;
            for (int preJ = 0; preJ < JCount; preJ += gap)
                desJCount += Increment;
            for (int preI = 0; preI < ICount; preI += gap)
                desICount += Increment;

            GridStructure gs = GridStructure.CreateSimple(desICount, desJCount);
            DoubleGrid upGrid = new DoubleGrid(gs);

            //previous(原始) ——> destination(目的) 的正向坐标映射
            for (int preJ = 0; preJ < ICount; preJ++)
            {
                for (int preI = 0; preI < JCount; preI++)
                {
                    //取样赋值
                    upGrid.SetCell(preI * Increment, preJ * Increment, GetCell(preI, preJ));
                }
            }

            double?[,] kernel_array = {
                                {1,  4,  7,  4,  1},
                                {4, 16, 26, 16,  4},
                                {7, 26, 41, 26,  7},
                                {4, 16, 26, 16,  4},
                                {1,  4,  7,  4,  1}
                                };
            DoubleGrid kernel = ReadFromArray(kernel_array);
            for (int j = 0; j < upGrid.JCount; j++)
            {
                for (int i = 0; i < upGrid.ICount; i++)
                {
                    DoubleGrid block = upGrid.GetRegionByCenter(i, 2, j, 2);

                    double sum = 0;
                    //
                    for (int j_kernel = 0; j_kernel < 5; j_kernel++)
                    {
                        for (int i_kernel = 0; i_kernel < 5; i_kernel++)
                        {
                            double v = block.GetCell(i_kernel, j_kernel) == null ?
                                0 : block.GetCell(i_kernel, j_kernel).Value;
                            sum += v * kernel.GetCell(i_kernel, j_kernel).Value;
                        }
                    }

                    upGrid.SetCell(i, j, sum / 273d);
                }
            }

            return upGrid;

        }

        #endregion

        public new void ToString()
        {
            if (Dimension == DimensionEnum._2D)
            {
                for (int i = 0; i < ICount; i++)
                {
                    Console.WriteLine("i=" + i);
                    for (int j = 0; j < JCount; j++)
                    {
                        Console.Write(GetCell(i, j) == null ?
                            "null" : GetCell(i, j).Value.ToString("f3"));
                        Console.Write("\t");
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            if (Dimension == DimensionEnum._3D)
            {
                for (int k = 0; k < KCount; k++)
                {
                    Console.WriteLine("k=" + k);
                    for (int i = 0; i < ICount; i++)
                    {
                        for (int j = 0; j < JCount; j++)
                        {
                            Console.Write(GetCell(i, j, k) == null ?
                                "null" : GetCell(i, j, k).Value.ToString("f3"));
                            Console.Write("\t");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }

        public string GetUniCode()
        {
            string s = string.Empty;

            for (int i = 0; i < Count; i++)
            {
                s += GetCell(i);
            }

            return s;
        }

        public static DoubleGrid ReadGridFromGSLIB_Console(string FileName)
        {
            System.Console.WriteLine("输入模型ICount");
            int model_ICount = int.Parse(System.Console.ReadLine());
            System.Console.WriteLine("输入模型JCount");
            int model_JCount = int.Parse(System.Console.ReadLine());
            System.Console.WriteLine("输入模型KCount");
            int model_KCount = int.Parse(System.Console.ReadLine());

            System.Console.WriteLine("输入模型Null对应的数值(默认-99)");
            int ValueOfNull = int.Parse(System.Console.ReadLine());

            GridStructure model_gs = null;
            if (model_KCount == 1)
            {
                model_gs = GridStructure.CreateSimple(model_ICount, model_JCount);
            }
            if (model_KCount > 1)
            {
                model_gs = GridStructure.CreateSimple(model_ICount, model_JCount, model_KCount);
            }

            DoubleGrid model = new DoubleGrid(model_gs);
            model.ReadFromGSLIB(FileName, ValueOfNull);//从文件导入models
            //model.Show();
            return model;
        }

        /// <summary>
        /// 替换值
        /// </summary>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        public void Replace(double? OldValue, double? NewValue)
        {
            for (int i = 0; i < Count; i++)
            {
                if (m_buffer[i] == OldValue)
                {
                    m_buffer[i] = NewValue;
                }
            }

        }
    }
}
