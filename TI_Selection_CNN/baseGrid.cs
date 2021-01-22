/*
 * base Grid class
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2017
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace JAM.Algorithm.Geometry
{
    [Serializable]
    public class baseGrid<T>
    {
        #region 属性

        //Null Cell 的总数
        public int NullCellCount { get; internal set; }
        /// <summary>
        /// Grid是否是Empty
        /// (如果有一个网格节点不是NUll，就是false；否则是true。)
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                //NullCell的数量等于Cell总数，说明全部为Null，所有Cell都是Null，则是Empty
                if (NullCellCount == Count)
                    return true;
                else
                    return false;
            }
        }

        #region GridStructure属性

        protected GridStructure m_gs = null;
        public GridStructure GridStructure
        {
            get
            {
                return m_gs;
            }
        }
        public DimensionEnum Dimension//维度
        {
            get { return m_gs.Dimension; }
        }
        public int ICount
        {
            get
            {
                return m_gs.ICount;
            }
        }
        public int JCount
        {
            get
            {
                return m_gs.JCount;
            }
        }
        public int KCount
        {
            get
            {
                return m_gs.KCount;
            }
        }
        public double ISize
        {
            get
            {
                return m_gs.ISize;
            }
        }
        public double JSize
        {
            get
            {
                return m_gs.JSize;
            }
        }
        public double KSize
        {
            get
            {
                return m_gs.KSize;
            }
        }
        public double IExtent
        {
            get
            {
                return m_gs.IExtent;
            }
        }
        public double JExtent
        {
            get
            {
                return m_gs.JExtent;
            }
        }
        public double KExtent
        {
            get
            {
                return m_gs.KExtent;
            }
        }
        public double OriginCellX
        {
            get
            {
                return m_gs.OriginCellX;
            }
        }
        public double OriginCellY
        {
            get
            {
                return m_gs.OriginCellY;
            }
        }
        public double OriginCellZ
        {
            get
            {
                return m_gs.OriginCellZ;
            }
        }

        #endregion

        #region 缓存区

        protected T[] m_buffer = null;
        /// <summary>
        /// 缓存数组
        /// </summary>
        public T[] Buffer
        {
            get
            {
                return m_buffer;
            }
        }

        #endregion

        #region 统计量

        /// <summary>
        /// 节点总数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_buffer.Length;
            }
        }

        public virtual T Min()
        {
            return default(T);
        }
        public virtual T Max()
        {
            return default(T);
        }

        #endregion

        #endregion

        #region 构造函数

        //实际并不使用的构造函数
        public baseGrid() { }
        public baseGrid(GridStructure gs)
        {
            m_gs = gs;
            if (gs.Dimension == DimensionEnum._2D)
            {
                //开辟缓存空间
                m_buffer = new T[ICount * JCount];
                for (int i = 0; i < m_buffer.Length; i++)
                { m_buffer[i] = (default(T)); }
            }
            if (gs.Dimension == DimensionEnum._3D)
            {
                m_buffer = new T[ICount * JCount * KCount];//开辟缓存空间
                for (int i = 0; i < m_buffer.Length; i++)
                { m_buffer[i] = (default(T)); }
            }
            this.NullCellCount = Count;
        }

        #endregion

        #region 方法组1(SetValue & GetValue of Cell)

        public void SetCell(T Cell)
        {
            for (int i = 0; i < Count; i++)
                m_buffer[i] = Cell;
        }

        public void SetCell(int arrayIndex, T Cell)
        {
            if (arrayIndex < 0 || arrayIndex >= Count)
                return;
            else
            {
                T old = GetCell(arrayIndex);
                if (old == null && Cell != null)
                {
                    NullCellCount -= 1;
                }
                if (old != null && Cell == null)
                {
                    NullCellCount += 1;
                }
                m_buffer[arrayIndex] = Cell;
            }
        }
        public void SetCell(int I, int J, T Cell)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I < 0 || J < 0 || I >= ICount || J >= JCount)
                return;
            else
            {
                T old = GetCell(I, J);
                if (old == null && Cell != null)
                {
                    NullCellCount -= 1;
                }
                if (old != null && Cell == null)
                {
                    NullCellCount += 1;
                }
                m_buffer[J * ICount + I] = Cell;
            }
        }
        public void SetCell(int I, int J, int K, T Cell)
        {
            if (DimensionEnum._2D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I < 0 || J < 0 || K < 0 || I >= ICount || J >= JCount || K >= KCount)
                return;
            else
            {
                T old = GetCell(I, J, K);
                if (old == null && Cell != null)
                {
                    NullCellCount -= 1;
                }
                if (old != null && Cell == null)
                {
                    NullCellCount += 1;
                }
                m_buffer[K * JCount * ICount + J * ICount + I] = Cell;
            }
        }
        public void SetCell(SpatialIndex spatialIndex, T Cell)
        {
            if (Dimension == DimensionEnum._2D)
                SetCell(spatialIndex.I, spatialIndex.J, Cell);
            else
                SetCell(spatialIndex.I, spatialIndex.J, spatialIndex.K, Cell);
        }

        public T GetCell(SpatialIndex spatialIndex)
        {
            switch (Dimension)
            {
                case DimensionEnum._2D:
                    return GetCell(spatialIndex.I, spatialIndex.J);
                case DimensionEnum._3D:
                    return GetCell(spatialIndex.I, spatialIndex.J, spatialIndex.K);
            }
            return default;
        }
        public T GetCell(int I, int J)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I < 0 || J < 0 || I >= ICount || J >= JCount)
                return default;
            else
                return m_buffer[J * ICount + I];
        }
        public T GetCell(int I, int J, int K)
        {
            if (DimensionEnum._2D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            if (I < 0 || J < 0 || K < 0 || I >= ICount || J >= JCount || K >= KCount)
                return default;
            else
                return m_buffer[K * JCount * ICount + J * ICount + I];
        }
        public T GetCell(int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex >= Count)
                return default;
            else
                return m_buffer[arrayIndex];
        }

        #endregion

        #region 方法组(GetValue of Block,Block is many Cells)

        public Tuple<List<SpatialIndex>, List<T>> GetBlockByRange(int I1, int I2, int J1, int J2)
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

            List<SpatialIndex> indexes = new List<SpatialIndex>();
            List<T> values = new List<T>();
            for (int j = J1; j <= J2; j++)
            {
                for (int i = I1; i <= I2; i++)
                {
                    indexes.Add(new SpatialIndex(i, j));
                    values.Add(GetCell(i, j));
                }
            }
            return new Tuple<List<SpatialIndex>, List<T>>(indexes, values);
        }

        public Tuple<List<SpatialIndex>, List<T>> GetBlockByRange(int I1, int I2, int J1, int J2, int K1, int K2)
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

            List<SpatialIndex> indexes = new List<SpatialIndex>();
            List<T> values = new List<T>();

            for (int k = K1; k <= K2; k++)
            {
                for (int j = J1; j <= J2; j++)
                {
                    for (int i = I1; i <= I2; i++)
                    {
                        indexes.Add(new SpatialIndex(i, j, k));
                        values.Add(GetCell(i, j, k));
                    }
                }
            }

            return new Tuple<List<SpatialIndex>, List<T>>(indexes, values);
        }

        public Tuple<List<SpatialIndex>, List<T>> GetBlockByCenter(int I, int IRadius, int J, int JRadius)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);
            int I1 = I - IRadius;
            int I2 = I + IRadius;
            int J1 = J - JRadius;
            int J2 = J + JRadius;
            return GetBlockByRange(I1, I2, J1, J2);
        }
        public Tuple<List<SpatialIndex>, List<T>> GetBlockByCenter(int I, int IRadius, int J, int JRadius, int K, int KRadius)
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

            return GetBlockByRange(I1, I2, J1, J2, K1, K2);
        }

        #endregion

        #region 方法组2(Is Index-I J K inside of Grid)

        /// <summary>
        /// 检验Cell索引是否在Grid范围内部(2D)，索引在网格范围内，则返回true.
        /// </summary>
        /// <param name="I"></param>
        /// <param name="J"></param>
        /// <returns></returns>
        public bool IsIndexValid(int I, int J)
        {
            if (I < 0 || J < 0 || I >= ICount || J >= JCount) //在范围之外，返回false
                return false;
            else//在范围内，返回true
                return true;
        }
        /// <summary>
        /// 检验节点索引是否在Grid范围内部(3D)
        /// </summary>
        /// <param name="I"></param>
        /// <param name="J"></param>
        /// <param name="K"></param>
        /// <returns></returns>
        public bool IsIndexValid(int I, int J, int K)
        {
            if (I < 0 || J < 0 || K < 0 || I >= ICount || J >= JCount || K >= KCount)
                return false;
            else
                return true;
        }
        /// <summary>
        /// 检验节点索引是否在TGrid范围内部(2D & 3D)
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public bool IsIndexValid(SpatialIndex spatialIndex)
        {
            if (spatialIndex.Dimension == DimensionEnum._2D)
                return IsIndexValid(spatialIndex.I, spatialIndex.J);
            if (spatialIndex.Dimension == DimensionEnum._3D)
                return IsIndexValid(spatialIndex.I, spatialIndex.J, spatialIndex.K);
            else
                return false;
        }

        #endregion

        #region 方法组3(create a new grid based on old grid)

        /// <summary>
        /// 浅度克隆网格
        /// </summary>
        /// <returns></returns>
        public baseGrid<T> LightClone()
        {
            //创建一个新的网格体
            baseGrid<T> CloneGrid = new baseGrid<T>(m_gs);
            return CloneGrid;
        }
        /// <summary>
        /// 浅度克隆网格，并用给定参数进行初始化
        /// </summary>
        /// <returns></returns>
        public baseGrid<T> LightClone(T value)
        {
            //创建一个新的网格体
            baseGrid<T> CloneGrid = LightClone();
            CloneGrid.SetCell(value);//使用给定参数初始化
            return CloneGrid;
        }
        /// <summary>
        /// 浅度克隆网格，同时修改网格体数据类型
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public baseGrid<V> LightClone<V>()
        {
            //创建一个新的网格体
            baseGrid<V> CloneGrid = new baseGrid<V>(m_gs);
            return CloneGrid;
        }
        /// <summary>
        /// 浅度克隆网格，同时修改网格体数据类型，并且赋值进行初始化
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public baseGrid<V> LightClone<V>(V value)
        {
            //创建一个新的网格体
            baseGrid<V> CloneGrid = LightClone<V>();
            CloneGrid.SetCell(value);//使用给定参数初始化
            return CloneGrid;
        }
        /// <summary>
        /// 深度克隆网格，数据完全复制
        /// </summary>
        /// <returns></returns>
        public virtual baseGrid<T> DeepClone()
        {
            //创建一个新的网格体
            baseGrid<T> CloneGrid = LightClone();
            for (int i = 0; i < Count; i++)
            {
                CloneGrid.SetCell(i, this.GetCell(i));
            }
            return CloneGrid;
        }
        /// <summary>
        /// 深度克隆网格，数据完全复制
        /// 同时修改网格体数据类型(假如可以的话)
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public baseGrid<V> DeepClone<V>()
        {
            //创建一个新的网格体
            baseGrid<V> CloneGrid = LightClone<V>();
            for (int i = 0; i < Count; i++)
            {
                object o = GetCell(i);
                CloneGrid.SetCell(i, (V)o);//如果类型不允许转换，提示异常
            }
            return CloneGrid;
        }

        #endregion

        #region 方法组5(GridResampling)

        /// <summary>
        /// 近邻取样插值方法(重采样) 2D维度
        /// </summary>
        /// <param name="desICount">目的输出ICount</param>
        /// <param name="desJCount">目的输出JCount</param>
        /// <returns></returns>
        public baseGrid<T> NearestNeighborResample(int desICount, int desJCount)
        {
            /// 名称：近邻取样插值方法(重采样) 2D维度
            /// 作用：最简单的插值算法，输出像素等于距离它映射的位置最近的输入像素的值
            /// 作者：喻思羽
            /// 编写时间：2015-10-9
            /// 参考资料：http://blog.chinaunix.net/uid-7525568-id-3452691.html
            /// 将Grid从nx,ny缩小到nx1,ny1，本质是缩略图思想

            if (this.Dimension != DimensionEnum._2D)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            GridStructure gs = new GridStructure(ISize, JSize, desICount, desJCount, OriginCellX, OriginCellY);
            //注意的是preISize需要根据情况修改
            baseGrid<T> desGrid = new baseGrid<T>(gs);

            int preI, preJ;
            //destination(目的) ——> previous(原始) 的反向坐标映射
            for (int desJ = 0; desJ < desJCount; desJ++)
            {
                for (int desI = 0; desI < desICount; desI++)
                {
                    preI = (int)(desI * ICount / desICount);
                    preJ = (int)(desJ * JCount / desJCount);
                    //取样赋值
                    desGrid.SetCell(new SpatialIndex(desI, desJ),
                        this.GetCell(new SpatialIndex(preI, preJ)));
                }
            }
            return desGrid;
        }
        /// <summary>
        /// 近邻取样插值方法(重采样) 3D维度
        /// </summary>
        /// <param name="desICount">目的输出ICount</param>
        /// <param name="desJCount">目的输出JCount</param>
        /// <param name="desKCount">目的输出KCount</param>
        /// <returns></returns>
        public baseGrid<T> NearestNeighborResample(int desICount, int desJCount, int desKCount)
        {
            GridStructure gs = new GridStructure(ISize, JSize, KSize, desICount, desJCount, desKCount, OriginCellX, OriginCellY, OriginCellZ);
            //注意的是preISize需要根据情况修改
            baseGrid<T> desGrid = new baseGrid<T>(gs);

            int preI, preJ, preK;
            //destination(目的) ——> previous(原始) 的反向坐标映射
            for (int desK = 0; desK < desKCount; desK++)
            {
                for (int desJ = 0; desJ < desJCount; desJ++)
                {
                    for (int desI = 0; desI < desICount; desI++)
                    {
                        preI = (int)(desI * ICount / desICount);
                        preJ = (int)(desJ * JCount / desJCount);
                        preK = (int)(desK * KCount / desKCount);
                        //取样赋值
                        desGrid.SetCell(new SpatialIndex(desI, desJ, desK),
                            this.GetCell(new SpatialIndex(preI, preJ, preK)));
                    }
                }
            }
            return desGrid;
        }
        /// <summary>
        /// 近邻取样插值方法(重采样) 2D & 3D 维度
        /// </summary>
        /// <param name="SpaceCount">抽样节点之间的间距</param>
        /// <returns></returns>
        public baseGrid<T> NearestNeighborResample(int SpaceCount = 1)
        {
            /// 名称：近邻取样插值方法(重采样) 2D & 3D 维度
            /// 作用：最简单的插值算法，输出像素等于距离它映射的位置最近的输入像素的值
            /// 作者：喻思羽
            /// 编写时间：2016-5-13
            /// 
            /// 方法示意图，节点用“*”表示
            /// ICount=10 JCount=7
            /// **********    0123456789
            /// **********    1
            /// **********    2
            /// **********    3
            /// **********    4
            /// **********    5
            /// **********    6
            /// 
            /// 经过计算后得到新的网格，去掉的节点用“0”表示
            /// ICount=5 JCount=4
            /// *0*0*0*0*0    0 2 4 6 8 
            /// 0000000000
            /// *0*0*0*0*0    2
            /// 0000000000
            /// *0*0*0*0*0    4
            /// 0000000000
            /// *0*0*0*0*0    6

            int Increment = SpaceCount + 1;//增量
            if (this.Dimension == DimensionEnum._2D)
            {
                int desICount = 0, desJCount = 0;
                for (int preJ = 0; preJ < m_gs.JCount; preJ += Increment)
                    desJCount += 1;
                for (int preI = 0; preI < m_gs.ICount; preI += Increment)
                    desICount += 1;

                baseGrid<T> desGrid = SimpleGrid(desICount, desJCount);

                for (int preJ = 0; preJ < this.ICount; preJ += Increment)//destination(目的) ——> previous(原始) 的反向坐标映射
                {
                    for (int preI = 0; preI < this.JCount; preI += Increment)
                    {
                        desGrid.SetCell(preI / Increment, preJ / Increment, this.GetCell(preI, preJ));//取样赋值
                    }
                }

                return desGrid;
            }
            else if (this.Dimension == DimensionEnum._3D)
            {
                int desICount = 0, desJCount = 0, desKCount = 0;
                for (int preK = 0; preK < this.KCount; preK += Increment)
                    desKCount += 1;
                for (int preJ = 0; preJ < this.JCount; preJ += Increment)
                    desJCount += 1;
                for (int preI = 0; preI < this.ICount; preI += Increment)
                    desICount += 1;

                baseGrid<T> desGrid = SimpleGrid(desICount, desJCount, desKCount);

                for (int preK = 0; preK < this.KCount; preK += Increment)//destination(目的) ——> previous(原始) 的反向坐标映射
                {
                    for (int preJ = 0; preJ < this.JCount; preJ += Increment)
                    {
                        for (int preI = 0; preI < this.JCount; preI += Increment)
                        {
                            desGrid.SetCell(preI / Increment, preJ / Increment, preK / Increment, this.GetCell(preI, preJ, preK));//取样赋值
                        }
                    }
                }
                return desGrid;
            }
            return null;
        }

        #endregion

        #region 方法组6(Create simple grid)

        public static baseGrid<T> SimpleGrid(int ICount, int JCount)
        {
            return new baseGrid<T>(new GridStructure(1.0, 1.0, ICount, JCount, 0.5, 0.5));
        }
        public static baseGrid<T> SimpleGrid(int ICount, int JCount, int KCount)
        {
            return new baseGrid<T>(new GridStructure(1.0, 1.0, 1.0, ICount, JCount, KCount, 0.5, 0.5, 0.5));
        }


        #endregion

        #region 方法组8 2D网格做垂向翻转

        /// <summary>
        /// 2D网格做垂向翻转(说明：仅用于二维网格)
        /// </summary>
        /// <returns></returns>
        public baseGrid<T> ReverseUpDown2D()
        {
            if (Dimension != DimensionEnum._2D)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            //创建一个新的网格体
            baseGrid<T> ReverseGrid = this.LightClone();
            //笛卡尔坐标系to屏幕坐标系（坐标转换）
            for (int j = 0; j < JCount; j++)
            {
                for (int i = 0; i < ICount; i++)
                {
                    ReverseGrid.SetCell(i, JCount - j - 1, GetCell(i, j));
                }
            }

            return ReverseGrid;
        }

        #endregion

        #region 方法组9 Convert baseGrid to DoubleGrid

        public DoubleGrid ConvertToDoubleGrid()
        {
            DoubleGrid grid = new DoubleGrid(GridStructure);
            for (int i = 0; i < Count; i++)
            {
                double? val = this.GetCell(i) == null ?
                    null :
                    (double?)Convert.ToDouble(this.GetCell(i));
                grid.SetCell(i, val);
            }
            return grid;
        }

        #endregion

        #region 方法组10 Read or Write with GSLIB

        public void WriteToGSLIB(string FileName, string Title, string ColumnName, double ValueOfNaN)
        {
            Type t = Nullable.GetUnderlyingType(typeof(T));
            string typeName = t.Name;
            string GridSize = "{"
                + "[" + typeName + "]"
                + "[" + GridStructure.ToMyString() + "]"
                + "}";

            if (File.Exists(FileName))
                File.Delete(FileName);

            using (StreamWriter sw = new StreamWriter(FileName))
            {
                sw.WriteLine(Title + GridSize);//输出GSLIB数据的标题
                sw.WriteLine("1");//输出变量数目
                sw.WriteLine(ColumnName);//输出属性名称
                for (int i = 0; i < Count; i++)//输出数据
                {
                    if (GetCell(i) == null)
                        sw.WriteLine(ValueOfNaN);
                    else
                        sw.WriteLine(GetCell(i));
                }
            }
        }
        public void ReadFromGSLIB(string FileName, double ValueOfNull)
        {
            List<string> tmp = new List<string>();//临时数据容器
            using (StreamReader sr = new StreamReader(FileName))//读取所有行数据
            {
                string s = sr.ReadLine();//read property name
                sr.ReadLine();//read column count
                sr.ReadLine();//read column name
                while (sr.Peek() > -1)
                {
                    tmp.Add(sr.ReadLine());
                }
            }

            for (int i = 0; i < tmp.Count; i++)//处理有效数据行
            {
                T val = GetValue(tmp[i]);//类型转换
                if (i < tmp.Count)//范围内的网格节点读取数值
                {
                    if (Convert.ToDouble(tmp[i]) == ValueOfNull)//如果value等于ValueOfNull，作为无效值
                        SetCell(i, default(T) == null ? default(T) : val);
                    else
                        SetCell(i, val);
                }
                if (i >= tmp.Count)//范围外的网格节点赋予空值null
                {
                    SetCell(i,
                        default(T) == null ? default(T) : (T)Convert.ChangeType(ValueOfNull, typeof(T)));
                }
            }
        }

        //string类型 to T类型
        public T GetValue(string value)
        {
            if (value == null || DBNull.Value.Equals(value))
                return default(T);

            var t = typeof(T);
            return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(t) ?? t);
        }

        #endregion

        #region 方法组11 点对点距离

        public virtual double Distance(baseGrid<T> other, int distType)
        {
            return -1;
        }

        #endregion

        #region 方法组12 Get Region

        /// <summary>
        /// 根据索引的界限提取区域部分网格(2D)
        /// </summary>
        /// <param name="I1"></param>
        /// <param name="I2"></param>
        /// <param name="J1"></param>
        /// <param name="J2"></param>
        /// <returns></returns>
        public virtual baseGrid<T> GetRegionByRange(int I1, int I2, int J1, int J2)
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

            //计算有效区间（有效区间可能不等于输入区间）
            I1 = Math.Max(I1, 0);
            I2 = Math.Min(I2, ICount - 1);
            J1 = Math.Max(J1, 0);
            J2 = Math.Min(J2, JCount - 1);

            int newICount = I2 - I1 + 1;
            int newJCount = J2 - J1 + 1;

            //创建一个新网格对象
            GridStructure gs = new GridStructure(ISize, JSize, newICount, newJCount, 0, 0);
            baseGrid<T> newGrid = new baseGrid<T>(gs);

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
        public virtual baseGrid<T> GetRegionByRange(int I1, int I2, int J1, int J2, int K1, int K2)
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
            baseGrid<T> newGrid = new baseGrid<T>(gs);

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
        public virtual baseGrid<T> GetRegionByCenter(int I, int IRadius, int J, int JRadius)
        {
            if (DimensionEnum._3D == Dimension)
                throw new Exception(MyExceptions.Geometry_DimensionException);

            int I1 = 0, I2 = 0;
            int J1 = 0, J2 = 0;

            I1 = I - IRadius;
            I2 = I + IRadius;
            J1 = J - JRadius;
            J2 = J + JRadius;

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
        public virtual baseGrid<T> GetRegionByCenter(int I, int IRadius, int J, int JRadius, int K, int KRadius)
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

        #endregion

        #region 方法组13 Clear

        /// <summary>
        /// 清空所有Cell
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_buffer.Length; i++)
            { m_buffer[i] = (default(T)); }
            this.NullCellCount = Count;
        }

        #endregion
    }
}
