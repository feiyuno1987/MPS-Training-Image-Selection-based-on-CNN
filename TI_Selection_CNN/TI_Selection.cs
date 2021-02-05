/*
 * Traing image Selection with CNN
 * 
 * This procedure can achieve TI selected precision test, the output file is output.xlsx. 
 * By default, there are 4 examples of data. If you need to do custom testing, please modify 
 * the example section of the code.
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2021.1
 * 
 */

using ConsoleProgressBar;
using JAM.Algorithm.Geometry;
using JAM.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;

namespace TI_Selection_CNN
{
    public class TI_Selection
    {
        public class Prediction
        {
            public string TrainModelName { get; set; }
            public int Counter { get; set; }
        }
        public static void Run()
        {
            //sampling ratios
            List<double> ratios = new List<double>() { 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.1 };
            int progress_counter = 0;

            Random rnd = new Random();
            GridStructure gs = null;//grid structure for set the dimentions

            int N = 90;//Sampling N times for train models
            int N1 = 100;//Sampling N1 times for test model
            int CellCount = 0;//Amount of model grid cells

            List<string> trainModels_path = new List<string>();
            string testModel_path = string.Empty;

            string save_path = $@"..\..\..\..\data\_sampling";//Sample file directory

            string AssetsFolder = @"..\..\..\..\ML_Assets";
            string TrainSamples_Path = $@"{AssetsFolder}\ImageClassification\train";
            string TestSamples_Path = $@"{AssetsFolder}\ImageClassification\test";
            string SavePath_LabelTag = $@"{AssetsFolder}\ImageClassification";

            trainModels_path.Clear();
            string example = "Example 2";

            if (example == "Example 1")
            {
                #region Example 1

                /* 不同相模式的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\..\data\ti_3types\circle.out");
                trainModels_path.Add(@"..\..\..\..\data\ti_3types\channel.out");
                trainModels_path.Add(@"..\..\..\..\data\ti_3types\smalltrain101.out");

                //testModel_path = @"..\..\..\..\data\ti_3types\(circle_realization)_n100.out";
                testModel_path = @"..\..\..\..\data\ti_3types\(channel_realization)_n100.out";
                //testModel_path = @"..\..\..\..\data\ti_3types\(smalltrain101_realization)_n100.out";

                #endregion
            }
            if (example == "Example 2")
            {
                #region Example 2

                /* 不同宽度河道的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\..\data\channel\channel.out");
                trainModels_path.Add(@"..\..\..\..\data\channel\(coarsening)channel.out");
                trainModels_path.Add(@"..\..\..\..\data\channel\(Refinement)channel.out");

                testModel_path = @"..\..\..\..\data\channel\(normal)node100.out";
                //testModel_path = @"..\..\..\..\data\channel\(coarsening)n100.out";
                //testModel_path = @"..\..\..\..\data\channel\(Refinement)n100.out";

                #endregion
            }
            if (example == "Example 3")
            {
                #region Example 3

                /* 不同走向河道的研究 */
                gs = GridStructure.CreateSimple(101, 101);
                CellCount = 101 * 101;

                trainModels_path.Add(@"..\..\..\..\data\channel_rotate\TIdegree0_n80.out");
                trainModels_path.Add(@"..\..\..\..\data\channel_rotate\TIdegree60_n80.out");
                trainModels_path.Add(@"..\..\..\..\data\channel_rotate\TIdegree120_n80.out");

                //testModel_path = @"..\..\..\..\data\channel_rotate\Testdegree0_n80.out";
                //testModel_path = @"..\..\..\..\data\channel_rotate\Testdegree60_n80.out";
                testModel_path = @"..\..\..\..\data\channel_rotate\Testdegree120_n80.out";

                #endregion
            }
            if (example == "Example 4")
            {
                #region Example 4

                gs = GridStructure.CreateSimple(200, 200);
                CellCount = 200 * 200;
                bool useTrick1 = true;
                if (useTrick1 == false)
                {
                    /* 1 continuous training images without using Trik1 */
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\radius1.out");
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\radius2.out");
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\stonewall.out");

                    testModel_path = @"..\..\..\..\data\ti_stonewall\radius15.out";
                }
                else
                {
                    /* 2 continuous training images using Trik1 */
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\radius1(4codes).out");
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\radius2(4codes).out");
                    trainModels_path.Add(@"..\..\..\..\data\ti_stonewall\stonewall(4codes).out");

                    testModel_path = @"..\..\..\..\data\ti_stonewall\radius15(4codes).out";
                }
                #endregion
            }

            string Output_Path = @"..\..\..\..\output.xlsx";

            MyDataTable myDt_result = new MyDataTable();
            foreach (var item in trainModels_path)
            {
                string trainModelName = FileHelper.GetFileName(item, false);
                myDt_result.AddColumn(trainModelName);
            }

            foreach (var ratio in ratios)//compute selection accuracy for each ratio
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"* * * * * * * * * * * * * sampling ratio is {ratio} * * * * * * * * * * * * * ");
                System.Console.WriteLine();

                #region I. Random sampling first 首先随机采样

                int K = Convert.ToInt32(CellCount * ratio);//the number of sampled pixels of the Grid

                if (!DirHelper.IsExistDirectory(save_path))
                    DirHelper.CreateDir(save_path);
                else
                    DirHelper.ClearDirectory(save_path);

                #region Random sampling for train Models(TI)

                foreach (var trainModel_path in trainModels_path)
                {
                    DoubleGrid trainModel = new DoubleGrid(gs);
                    trainModel.ReadFromGSLIB(trainModel_path, -99);
                    System.Console.WriteLine($"training image file path is {trainModel_path}");

                    using (var pb = new ProgressBar())
                    {
                        progress_counter = 0;
                        for (int n = 1; n <= N; n++)
                        {
                            progress_counter++;
                            double progress = progress_counter / (double)N;
                            pb.Progress.Report(progress, $"Do the sampling,A total of {N} samples");

                            var tempGrid = trainModel.LightClone().ConvertToDoubleGrid();
                            for (int k = 0; k < K*1; k++)//Sampling for K locations
                            {
                                int I = rnd.Next(0, trainModel.ICount);
                                int J = rnd.Next(0, trainModel.JCount);
                                //point-based sampling
                                var index = new SpatialIndex(I, J);
                                tempGrid.SetCell(index, trainModel.GetCell(index));
                            }

                            //output sampled pointset
                            string trainModel_Name = FileHelper.GetFileName(trainModel_path, false);
                            string outpath = $"{save_path}\\{trainModel_Name}_sampled{n}.out";
                            tempGrid.WriteToGSLIB(outpath, $"sampled {tempGrid.Count - tempGrid.NullCellCount} from {trainModel_Name}", "code", -99);
                        }
                    }
                }

                #endregion

                #region Random sampling for test model

                DoubleGrid testModel = new DoubleGrid(gs);
                testModel.ReadFromGSLIB(testModel_path, -99);
                System.Console.WriteLine($"test model file path is {testModel_path}");

                using (var pb = new ProgressBar())
                {
                    progress_counter = 0;
                    for (int n = 1; n <= N1; n++)
                    {
                        progress_counter++;
                        double progress = progress_counter / (double)N1;
                        pb.Progress.Report(progress, $"Do the sampling,A total of {N} samples");

                        var tempGrid = testModel.LightClone().ConvertToDoubleGrid();
                        for (int k = 0; k < K; k++)//Sampling for K locations
                        {
                            int I = rnd.Next(0, testModel.ICount);
                            int J = rnd.Next(0, testModel.JCount);
                            //point-based sampling
                            var index = new SpatialIndex(I, J);
                            tempGrid.SetCell(index, testModel.GetCell(index));
                        }

                        //output sampled pointset
                        string testModel_Name = FileHelper.GetFileName(testModel_path, false);
                        string outpath = $"{save_path}\\{testModel_Name}_sampled{n}.out";
                        tempGrid.WriteToGSLIB(outpath, $"sampled {tempGrid.Count - tempGrid.NullCellCount} from {testModel_Name}", "code", -99);
                    }
                }

                #endregion

                #endregion

                #region II. gslib -> image 绘制模型图像

                //绘制图
                string[] fileNames = DirHelper.GetFileNames(save_path, "*.out", false);
                System.Console.WriteLine($"一共{fileNames.Length}个Grid文件");
                int ColorMapEnumCode = 4;
                bool IsRePic = false;

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

                #region III. Import sampled pointset into the Training and Test folders 导入到training 和test文件夹

                #region 1. Copy Images To Folder

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

                #endregion

                #region 2. Make Tag File

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

                #region IV. Image Classification with TensorFlow

                System.Console.WriteLine();
                System.Console.WriteLine("Training Convolutional Neural Network");
                System.Console.WriteLine();

                TensorFlow_ImageClassification.GetTrainImageNames();
                TensorFlow_ImageClassification.TrainAndSaveModel();
                var predict = TensorFlow_ImageClassification.LoadAndPrediction();
                MyRow row1 = myDt_result.NewRow();
                foreach (var item in predict)
                {
                    row1[item.TrainModelName] = item.Counter;
                    //output to console
                    System.Console.WriteLine($"{item.TrainModelName}:{item.Counter}");
                }
                myDt_result.AddRow(row1);

                #endregion

            }

            DataTable dt = MyDataTable.WriteToDataTable(myDt_result);
            ExcelHelper.TableToExcel(dt, Output_Path);
        }
    }
}
