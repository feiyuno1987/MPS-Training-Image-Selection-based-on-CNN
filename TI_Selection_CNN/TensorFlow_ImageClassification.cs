﻿/*
 * image classification with CNN by using ML.NET
 * 
 * Author : Siyu YU(573315294@qq.com)
 * Date : 2020.8
 * 
 */

using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static TI_Selection_CNN.TI_Selection;

namespace TI_Selection_CNN
{
    public class TensorFlow_ImageClassification
    {
        static readonly string AssetsFolder = @"..\..\..\..\ML_Assets";
        static readonly string TrainDataFolder = Path.Combine(AssetsFolder, "ImageClassification", "train");
        static readonly string TrainTagsPath = Path.Combine(AssetsFolder, "ImageClassification", "train_tags.tsv");
        static readonly string TestDataFolder = Path.Combine(AssetsFolder, "ImageClassification", "test");
        static readonly string inceptionPb = Path.Combine(AssetsFolder, "TensorFlow", "tensorflow_inception_graph.pb");
        static readonly string imageClassifierZip = Path.Combine(AssetsFolder, "MLModel", "imageClassifier.zip");

        static HashSet<string> TrainImageNames = null;

        //配置用常量
        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public static void TrainAndSaveModel()
        {
            MLContext mlContext = new MLContext(seed: 2);

            // STEP 1: 准备数据
            var fulldata = mlContext.Data.LoadFromTextFile<ImageNetData>(path: TrainTagsPath, separatorChar: '\t', hasHeader: false);

            var trainTestData = mlContext.Data.TrainTestSplit(fulldata, testFraction: 0.15);
            var trainData = trainTestData.TrainSet;
            var testData = trainTestData.TestSet;

            // STEP 2：创建学习管道
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelTokey", inputColumnName: "Label")
                .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: TrainDataFolder, inputColumnName: nameof(ImageNetData.ImagePath)))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "input"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: ImageNetSettings.channelsLast, offsetImage: ImageNetSettings.mean))
                .Append(mlContext.Model.LoadTensorFlowModel(inceptionPb).
                     ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelTokey", featureColumnName: "softmax2_pre_activation"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"))
                .AppendCacheCheckpoint(mlContext);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            // STEP 3：通过训练数据调整模型    
            ITransformer model = pipeline.Fit(trainData);
            sw.Stop();
            System.Console.WriteLine($"Training Time:{sw.ElapsedMilliseconds} Milliseconds");
            System.Console.WriteLine();

            // STEP 4：评估模型
            Console.WriteLine("===== Evaluate model =======");
            var evaData = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(evaData, labelColumnName: "LabelTokey", predictedLabelColumnName: "PredictedLabel");
            PrintMultiClassClassificationMetrics(metrics);

            //STEP 5：保存模型
            Console.WriteLine("====== Save model to local file =========");
            if (false == Directory.Exists(Path.GetDirectoryName(imageClassifierZip)))//create folder to save model if not exist
                Directory.CreateDirectory(Path.GetDirectoryName(imageClassifierZip));
            mlContext.Model.Save(model, trainData.Schema, imageClassifierZip);
        }

        public static List<Prediction> LoadAndPrediction()
        {
            MLContext mlContext = new MLContext(seed: 2);

            // Load the model
            ITransformer loadedModel = mlContext.Model.Load(imageClassifierZip, out var modelInputSchema);

            // Make prediction function (input = ImageNetData, output = ImageNetPrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(loadedModel);

            DirectoryInfo testdir = new DirectoryInfo(TestDataFolder);

            Dictionary<string, int> _counter = new Dictionary<string, int>();
            foreach (var item in TrainImageNames)
            {
                _counter.Add(item, 0);
            }
            foreach (var jpgfile in testdir.GetFiles("*.jpg"))
            {
                ImageNetData image = new ImageNetData();
                image.ImagePath = jpgfile.FullName;
                var pred = predictor.Predict(image);

                Console.WriteLine($"Filename:{jpgfile.Name}:\tPredict Result:{pred.PredictedLabelValue}");

                //统计
                _counter[pred.PredictedLabelValue]++;
            }
            Dictionary<string, int> ordered_counter = _counter
                .OrderBy(o => o.Key)
                .ToDictionary(o => o.Key, p => p.Value);
            List<Prediction> result = new List<Prediction>();
            foreach (var item in ordered_counter)
            {
                result.Add(new Prediction { TrainModelName = item.Key, Counter = item.Value });
            }
            return result;
        }

        public static void PrintMultiClassClassificationMetrics(MulticlassClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for L-BFGS  multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            //Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
            //Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
            //Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");
            Console.WriteLine($"************************************************************");
        }

        public static void GetTrainImageNames()
        {
            TrainImageNames = new HashSet<string>();

            //获取文件列表
            var filePaths = Directory.GetFiles(TrainDataFolder);

            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i];
                string fileName = GetFileName(filePaths[i], false);

                string TrainImageName = fileName.Substring(0, fileName.LastIndexOf('_'));
                if (!TrainImageNames.Contains(TrainImageName))
                {
                    TrainImageNames.Add(TrainImageName);
                }
            }
        }

        static string GetFileName(string fileFullPath, bool includeExtension)
        {
            if (File.Exists(fileFullPath))
            {
                var f = new FileInfo(fileFullPath);
                if (includeExtension)
                {
                    return f.Name;
                }
                return f.Name.Replace(f.Extension, "");
            }
            return null;
        }
    }

    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }

    public class ImageNetPrediction
    {
        //public float[] Score;
        public string PredictedLabelValue;
    }
}
