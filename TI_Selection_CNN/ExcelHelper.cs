using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.IO;
using System.Text.RegularExpressions;

using NPOI.SS.UserModel;
using NPOI.SS.Util;
//2003
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
//2007
using NPOI.XSSF.UserModel;
using NPOI.XSSF.Util;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;
using TI_Selection_CNN;

namespace JAM.Utilities
{
    /// <summary>
    /// 基于NPOI的Excel辅助类——完美版本(Excel 2003 and Excel 2007)
    /// </summary>
    public class ExcelHelper
    {
        #region Excel <-> Table

        /// <summary>
        /// 将Excel文件的数据读出到DataTable中(Excel 2003 & Excel 2007)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DataTable ExcelToTable(string file)
        {
            if (file == string.Empty) return null;
            DataTable dt = new DataTable();
            dt.TableName = FileHelper.GetFileName(file, false);
            using (FileStream stream = new FileStream(@file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string extension = FileHelper.GetFileExtension(file);
                //使用接口，自动识别excel2003/2007格式
                IWorkbook workbook = WorkbookFactory.Create(stream);
                //得到里面第一个sheet
                ISheet sheet = workbook.GetSheetAt(0);

                #region 表头
                //表头
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                List<int> columns = new List<int>();
                for (int i = 0; i < header.LastCellNum; i++)
                {
                    object obj = GetValue(header.GetCell(i));
                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        dt.Columns.Add(new DataColumn("Column" + i.ToString()));
                    }
                    else
                    {
                        dt.Columns.Add(new DataColumn(obj.ToString()));
                    }
                    columns.Add(i);
                }
                #endregion

                #region 数据
                //数据
                for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
                {
                    DataRow dr = dt.NewRow();
                    bool hasValue = false;
                    foreach (int j in columns)
                    {
                        dr[j] = GetValue(sheet.GetRow(i).GetCell(j));
                        if (dr[j] != null && dr[j].ToString() != string.Empty)
                        {
                            hasValue = true;
                        }
                    }
                    if (hasValue)
                    {
                        dt.Rows.Add(dr);
                    }
                }
                #endregion
            }
            return dt;
        }

        /// <summary>
        /// 将DataTable数据导出到Excel文件中(Excel 2003 & Excel 2007)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="file"></param>
        public static void TableToExcel(DataTable dt, string file)
        {
            //根据输出文件格式，创建对应的excel2003/2007格式的工作薄
            IWorkbook workbook = null;
            if (file.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (file.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");

            //设置时间格式
            ICellStyle dateStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

            #region 表头
            //表头
            IRow row = sheet.CreateRow(0);

            //设置cell风格
            ICellStyle style = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.Color = HSSFColor.Green.Index;//字体颜色
            //font.Boldweight = (short)FontBoldWeight.Bold;//字体加粗样式
            //style.FillForegroundColor = HSSFColor.White.Index;//GetXLColour(wb, LevelOneColor);// 设置背景色
            //style.FillPattern = FillPattern.SolidForeground;
            style.SetFont(font);//样式里的字体设置具体的字体样式
            style.Alignment = HorizontalAlignment.Center;//文字水平对齐方式
            style.VerticalAlignment = VerticalAlignment.Center;//文字垂直对齐方式
            //row.HeightInPoints = 20;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.CellStyle = style;
                cell.SetCellValue(dt.Columns[i].ColumnName);
            }
            #endregion

            #region 数据
            //数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row1 = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row1.CreateCell(j);
                    string drValue = dt.Rows[i][j].ToString();

                    switch (dt.Columns[j].DataType.ToString())
                    {
                        #region 字符串类型
                        case "System.String": //字符串类型
                            double result;
                            if (isNumeric(drValue, out result))
                            {
                                double.TryParse(drValue, out result);
                                cell.SetCellValue(result);
                                break;
                            }
                            else
                            {
                                cell.SetCellValue(drValue);
                                break;
                            }
                        #endregion
                        #region 日期类型
                        case "System.DateTime": //日期类型
                            DateTime dateV;
                            DateTime.TryParse(drValue, out dateV);
                            cell.SetCellValue(dateV);

                            cell.CellStyle = dateStyle; //格式化显示
                            break;
                        #endregion
                        #region 布尔型
                        case "System.Boolean": //布尔型
                            bool boolV = false;
                            bool.TryParse(drValue, out boolV);
                            cell.SetCellValue(boolV);
                            break;
                        #endregion
                        #region 整型
                        case "System.Int16": //整型
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            int intV = 0;
                            int.TryParse(drValue, out intV);
                            cell.SetCellValue(intV);
                            break;
                        #endregion
                        #region 浮点型
                        case "System.Decimal": //浮点型
                        case "System.Double":
                            double doubV = 0;
                            double.TryParse(drValue, out doubV);
                            cell.SetCellValue(doubV);
                            break;
                        #endregion
                        #region 空值处理
                        case "System.DBNull": //空值处理
                            cell.SetCellValue("");
                            break;
                        #endregion
                        #region default
                        default:
                            cell.SetCellValue("");
                            break;
                            #endregion
                    }
                }
            }
            #endregion

            //转为字节数组
            MemoryStream stream = new MemoryStream();
            workbook.Write(stream);
            var buf = stream.ToArray();

            //保存为Excel文件
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
            }
        }

        #endregion

        #region Excel <-> List<double> or List<string>

        public static void ListToExcel(List<double> list, string file)
        {
            if (list == null || list.Count == 0)
                return;

            //把list转换为DataTable
            DataTable dt = EntityListHelper.ListToDataTable(list);
            TableToExcel(dt, file);
        }

        public static List<double> ExcelToList(string file)
        {
            DataTable dt = ExcelToTable(file);
            List<double> list = new List<double>();
            foreach (DataRow item in dt.Rows)
            {
                list.Add(Convert.ToDouble(item[0]));
            }
            return list;
        }

        public static void StringListToExcel(List<string> list, string file)
        {
            if (list == null || list.Count == 0)
                return;

            //把list转换为DataTable
            DataTable dt = EntityListHelper.ListToDataTable(list);
            TableToExcel(dt, file);
        }

        public static List<string> ExcelToStringList(string file)
        {
            DataTable dt = ExcelToTable(file);
            List<string> list = new List<string>();
            foreach (DataRow item in dt.Rows)
            {
                list.Add(Convert.ToString(item[0]));
            }
            return list;
        }

        #endregion

        #region 私有方法

        private static object GetValue(ICell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:
                    return null;
                case CellType.Boolean: //BOOLEAN:
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return DateTime.FromOADate(cell.NumericCellValue);
                    else
                        return cell.NumericCellValue;
                case CellType.String: //STRING:
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:
                default:
                    return "=" + cell.CellFormula;
            }
        }

        private static bool isNumeric(String message, out double result)
        {
            Regex rex = new Regex(@"^[-]?\d+[.]?\d*$");
            result = -1;
            if (rex.IsMatch(message))
            {
                result = double.Parse(message);
                return true;
            }
            else
                return false;
        }

        #endregion
    }
}
