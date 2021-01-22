/*
 * Traing image Selection with CNN
 * 
 * This procedure can achieve TI selected precision test, the output file is output.log. 
 * By default, there are 4 examples of data. If you need to do custom testing, please modify 
 * the example section of the code.
 * 
 * If you are only doing training image selection, change yourself to the training image and 
 * test file, and then run the Tensorflow_ImageClassification project.
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2021.1
 * 
 */

using ConsoleProgressBar;
using JAM.Algorithm.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace TI_Selection_CNN
{
    public class TI_Selection
    {
        public static void Run()
        {
            //sampling ratios
            List<double> ratios = new List<double>() { 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.1 };
            int K = 300;//Sampling 300 times
            int CellCount = 0;//Amount of model grid cells
            bool use_block = true;//Enable block sampling mode
            Random rnd = new Random();
            GridStructure gs = null;
            List<string> trainModels_path = new List<string>();
            string testModel_path = string.Empty;
            string save_path = $@"..\..\..\data\_sampling";//抽样文件目录
            int progress_counter = 0;

            string AssetsFolder = @"..\..\..\ML_Assets";
            string TrainSamples_Path = $@"{AssetsFolder}\ImageClassification\train";
            string TestSamples_Path = $@"{AssetsFolder}\ImageClassification\test";
            string SavePath_LabelTag = $@"{AssetsFolder}\ImageClassification";

            trainModels_path.Clear();
            string example = "Example 4";
            if (example == "Example 1")
            {
                #region Example 1

                /* 不同相模式的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\data\ti_3types\circle.out");
                trainModels_path.Add(@"..\..\..\data\ti_3types\channel.out");
                trainModels_path.Add(@"..\..\..\data\ti_3types\smalltrain101.out");

                testModel_path = @"..\..\..\data\ti_3types\(circle_realization)_n100.out";
                //testModel_path = @"..\..\..\data\ti_3types\(channel_realization)_n100.out";
                //testModel_path = @"..\..\..\data\ti_3types\(smalltrain101_realization)_n100.out";

                #endregion
            }
            if (example == "Example 2")
            {
                #region Example 2

                /* 不同宽度河道的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\data\channel\channel.out");
                trainModels_path.Add(@"..\..\..\data\channel\(coarsening)channel.out");
                trainModels_path.Add(@"..\..\..\data\channel\(Refinement)channel.out");

                testModel_path = @"..\..\..\data\channel\(normal)node100.out";
                //testModel_path = @"..\..\..\data\channel\(coarsening)n100.out";
                //testModel_path = @"..\..\..\data\channel\(Refinement)n100.out";

                #endregion
            }
            if (example == "Example 3")
            {
                #region Example 3

                /* 不同走向河道的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\data\channel_rotate\TIdegree0_n80.out");
                trainModels_path.Add(@"..\..\..\data\channel_rotate\TIdegree60_n80.out");
                trainModels_path.Add(@"..\..\..\data\channel_rotate\TIdegree120_n80.out");

                //testModel_path = @"..\..\..\data\channel_rotate\Testdegree0_n80.out";
                testModel_path = @"..\..\..\data\channel_rotate\Testdegree60_n80.out";
                //testModel_path = @"..\..\..\data\channel_rotate\Testdegree120_n80.out";

                #endregion
            }
            if (example == "Example 4")
            {
                #region Example 4

                gs = GridStructure.CreateSimple(200, 200);
                CellCount = 200 * 200;
                bool useTrick1 = false;
                if (useTrick1 == false)
                {
                    /* 1 continuous training images without using Trik1 */
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius1.out");
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius2.out");
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius15.out");

                    testModel_path = @"..\..\..\data\ti_stonewall\radius15.out";
                }
                else
                {
                    /* 2 continuous training images using Trik1 */
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius1(4codes).out");
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius2(4codes).out");
                    trainModels_path.Add(@"..\..\..\data\ti_stonewall\radius15(4codes).out");

                    testModel_path = @"..\..\..\data\ti_stonewall\radius15(4codes).out";
                }
                #endregion
            }

            foreach (var ratio in ratios)//compute for each ratio
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"* * * * * * * * * * * * * sampling ratio is {ratio} * * * * * * * * * * * * * ");
                System.Console.WriteLine();

                #region Random sampling first 首先随机采样

                int N = Convert.ToInt32(CellCount * ratio);//the number of sampled pixels of the Grid

                if (!DirHelper.IsExistDirectory(save_path))
                    DirHelper.CreateDir(save_path);
                else
                    DirHelper.ClearDirectory(save_path);

                List<double> NotNull百分比Mean = new List<double>();

                List<string> grid_paths = new List<string>();//文件路径
                grid_paths.AddRange(trainModels_path);
                grid_paths.Add(testModel_path);
                foreach (var grid_path in grid_paths)
                {
                    DoubleGrid grid = new DoubleGrid(gs);
                    grid.ReadFromGSLIB(grid_path, -99);
                    System.Console.WriteLine($"grid file path is {grid_path}");

                    using (var pb = new ProgressBar())
                    {
                        progress_counter = 0;
                        for (int k = 1; k <= K; k++)
                        {
                            progress_counter++;
                            double progress = progress_counter / (double)K;
                            pb.Progress.Report(progress, $"Do the sampling,A total of {K} samples");

                            var tempGrid = grid.LightClone().ConvertToDoubleGrid();
                            for (int i = 0; i < N; i++)//Sampling for N locations
                            {
                                int I = rnd.Next(0, grid.ICount);
                                int J = rnd.Next(0, grid.JCount);

                                if (use_block)//block-based sampling
                                {
                                    var block = grid.GetBlockByCenter(I, 1, J, 1);
                                    List<SpatialIndex> indexes = block.Item1;
                                    List<double?> values = block.Item2;

                                    for (int c = 0; c < indexes.Count; c++)
                                    {
                                        tempGrid.SetCell(indexes[c], values[c]);
                                    }
                                }
                                else//point-based sampling
                                {
                                    var index = new SpatialIndex(I, J);
                                    tempGrid.SetCell(index, grid.GetCell(index));
                                }
                            }

                            double NotNull百分比 = (tempGrid.Count - tempGrid.NullCellCount) / (double)tempGrid.Count;
                            NotNull百分比Mean.Add(NotNull百分比);
                            //输出
                            string TI_Name = FileHelper.GetFileName(grid_path, false);
                            string outpath = $"{save_path}\\{TI_Name}_sampled{k}.out";
                            tempGrid.WriteToGSLIB(outpath, $"sampled {N} from {TI_Name}", "code", -99);
                        }
                    }
                }
                double temp = NotNull百分比Mean.Average();

                #endregion

                #region gslib -> image 绘制模型图像

                //绘制图
                string[] fileNames = DirHelper.GetFileNames(save_path, "*.out", false);
                System.Console.WriteLine($"一共{fileNames.Length}个Grid文件");
                int ColorMapEnumCode = 4;
                bool IsRePic = true;
                string IsRePic_Str = "0";
                if (IsRePic_Str == "0")
                    IsRePic = false;
                if (IsRePic_Str == "1")
                    IsRePic = true;

                System.Console.WriteLine();
                System.Console.WriteLine("# the output image file is jpeg format");

                progress_counter = 0;
                using (var pb = new ProgressBar())
                {
                    foreach (var fileName in fileNames)
                    {
                        progress_counter++;
                        double progress = progress_counter / (double)fileNames.Length;
                        pb.Progress.Report(progress, "Draw Grids");

                        string bitmap_fileName = FileHelper.GetFileName(fileName, false);

                        DoubleGrid grid = new DoubleGrid(gs);
                        grid.ReadFromGSLIB(fileName, -99);//Read training images

                        GridPlot gridPlot = new GridPlot();
                        gridPlot.Grid = grid;
                        Bitmap bitmap = gridPlot.DrawGrid(1, (JAM.Algorithm.Visualization.ColorMapEnum)ColorMapEnumCode, false, IsRePic, 255);

                        string bitmap_filePath = $"{save_path}\\{bitmap_fileName}.jpg";
                        bitmap.Save(bitmap_filePath);
                        bitmap.Save(bitmap_filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                #endregion

                #region 导入到training 和test文件夹

                #region CopyImagesToFolder

                var filePaths = DirHelper.GetFileNames(save_path, "*.jpg", false);
                FileHelper.SortByName(ref filePaths);

                Dictionary<string, List<string>> dict_filePaths = new Dictionary<string, List<string>>();

                for (int i = 0; i < filePaths.Length; i++)
                {
                    string filePath = filePaths[i];
                    string fileName = FileHelper.GetFileName(filePaths[i], false);

                    string key = fileName.Substring(0, fileName.LastIndexOf('_'));
                    if (dict_filePaths.ContainsKey(key))
                    {
                        dict_filePaths[key].Add(filePath);
                    }
                    else
                    {
                        dict_filePaths.Add(key, new List<string>() { filePath });
                    }
                }

                if (!DirHelper.IsExistDirectory(TrainSamples_Path))
                    DirHelper.CreateDir(TrainSamples_Path);
                else
                    DirHelper.ClearDirectory(TrainSamples_Path);

                if (!DirHelper.IsExistDirectory(TestSamples_Path))
                    DirHelper.CreateDir(TestSamples_Path);
                else
                    DirHelper.ClearDirectory(TestSamples_Path);

                foreach (var idx in trainModels_path)
                {
                    string key_train = FileHelper.GetFileName(idx, false);
                    List<string> filePaths_train = dict_filePaths[key_train];
                    foreach (var filePath in filePaths_train)
                    {
                        string filePath_new = string.Format("{0}\\{1}", TrainSamples_Path, FileHelper.GetFileName(filePath));
                        File.Copy(filePath, filePath_new);
                    }
                }
                string key_test = FileHelper.GetFileName(testModel_path, false);
                List<string> filePaths_test = dict_filePaths[key_test];
                foreach (var filePath in filePaths_test)
                {
                    string filePath_new = string.Format("{0}\\{1}", TestSamples_Path, FileHelper.GetFileName(filePath));
                    File.Copy(filePath, filePath_new);
                }


                #region MakeTagFile

                var filePaths_temp = DirHelper.GetFileNames(TrainSamples_Path);
                FileHelper.SortByName(ref filePaths_temp);

                MyDataTable myDT = new MyDataTable();
                myDT.AddColumn("fileName");
                myDT.AddColumn("label");

                for (int j = 0; j < filePaths_temp.Length; j++)
                {
                    MyRow row = myDT.NewRow();
                    string fileName = FileHelper.GetFileName(filePaths_temp[j]);
                    string label = fileName.Substring(0, fileName.LastIndexOf('_'));
                    row["fileName"] = fileName;
                    row["label"] = label;
                    myDT.AddRow(row);
                }
                string TagFilePath = $"{SavePath_LabelTag}\\train_tags.tsv";
                myDT.WriteToTxt(TagFilePath, "\t", false);

                #endregion

                #endregion

                #endregion

                #region Call the convolutional deep learning program EXE 

                //output dir is "..\..\..\..\output.log"
                string exefile = string.Empty;
#if DEBUG
                exefile = @"..\..\..\TensorFlow_ImageClassification\bin\Debug\netcoreapp3.1\TensorFlow_ImageClassification.exe";
#else
                exefile = @"..\..\..\TensorFlow_ImageClassification\bin\Release\netcoreapp3.1\TensorFlow_ImageClassification.exe";
#endif

                System.Console.WriteLine();
                System.Console.WriteLine("Perform convolutional training and testing");

                Process p = new Process();
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exefile);
                p.StartInfo.FileName = "TensorFlow_ImageClassification.exe";
                p.StartInfo.Arguments = $"* * * * Sampling Ratio is {ratio} * * * *";
                p.Start();
                p.WaitForExit();
                //Process.Start(exefile).WaitForExit();

                #endregion
            }
        }
    }
}
