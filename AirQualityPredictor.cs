using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

namespace AirClassificator
{
    public class AirQualityData
    {
        [LoadColumn(0)]
        public float PM10 { get; set; }

        [LoadColumn(1)]
        public float PM2_5 { get; set; }

        [LoadColumn(2)]
        public float CO { get; set; }

        [LoadColumn(3)]
        public float NO2 { get; set; }

        [LoadColumn(4)]
        public float O3 { get; set; }

        [LoadColumn(5)]
        public float SO2 { get; set; }

        [LoadColumn(6)]
        public uint Cluster { get; set; }
    }

    public class AirQualityPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedLabel { get; set; }

        [ColumnName("Score")]
        public float[] Scores { get; set; }
    }

    public class AirQualityPredictor
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string[] _requiredFeatures = { "Твёрдые микрочастицы диаметром 2.5 мкм", "Твёрдые микрочастицы диаметром 10 мкм", "Озон", "Диоксид азота", "Сернистый газ", "Монооксид углерода" };
        private readonly Dictionary<uint, string> _clusterToLevelMapping = new Dictionary<uint, string>
        {
            { 3, "Высокий уровень загрязнённости воздуха" },
            { 0, "Оптимальное состояние воздуха" },
            { 1, "Низкий уровень загрязнённости воздуха" },
            { 2, "Средний уровень загрязнённости воздуха" }
        };
        private Dictionary<string, float> _maxFeatureValues;

        public AirQualityPredictor(string datasetPath)
        {
            _mlContext = new MLContext(seed: 0);
            TrainModel(datasetPath);
        }

        private void TrainModel(string datasetPath)
        {
            if (!File.Exists(datasetPath))
            {
                throw new FileNotFoundException($"Датасет не найден по пути: {datasetPath}");
            }

            // Загрузка данных данные
            var data = _mlContext.Data.LoadFromTextFile<AirQualityData>(
                path: datasetPath,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // Анализ данных
            var dataEnumerable = _mlContext.Data.CreateEnumerable<AirQualityData>(data, reuseRowObject: false).ToList();
            Console.WriteLine($"Total number of records in dataset: {dataEnumerable.Count}");

            // Проверка на пропущенные значения
            bool hasMissingValues = dataEnumerable.Any(d =>
                float.IsNaN(d.PM10) || float.IsNaN(d.PM2_5) || float.IsNaN(d.CO) ||
                float.IsNaN(d.NO2) || float.IsNaN(d.O3) || float.IsNaN(d.SO2));
            if (hasMissingValues)
            {
                Console.WriteLine("Warning: Dataset contains missing values (NaN). Replacing with 0.");
                dataEnumerable = dataEnumerable.Select(d => new AirQualityData
                {
                    PM10 = float.IsNaN(d.PM10) ? 0 : d.PM10,
                    PM2_5 = float.IsNaN(d.PM2_5) ? 0 : d.PM2_5,
                    CO = float.IsNaN(d.CO) ? 0 : d.CO,
                    NO2 = float.IsNaN(d.NO2) ? 0 : d.NO2,
                    O3 = float.IsNaN(d.O3) ? 0 : d.O3,
                    SO2 = float.IsNaN(d.SO2) ? 0 : d.SO2,
                    Cluster = d.Cluster
                }).ToList();
                data = _mlContext.Data.LoadFromEnumerable(dataEnumerable);
            }

            // Объединяем классы 0 и 4 (прошлая реализация с автоназначением меток)
            //foreach (var item in dataEnumerable)
            //{
            //    if (item.Cluster == 4)
            //    {
            //        item.Cluster = 0;
            //    }
            //}
            data = _mlContext.Data.LoadFromEnumerable(dataEnumerable);

            // Проверка меток
            var uniqueClusters = dataEnumerable.Select(d => d.Cluster).Distinct().OrderBy(c => c).ToList();
            Console.WriteLine($"Unique clusters after merging: {string.Join(", ", uniqueClusters)}");
            if (uniqueClusters.Any(c => c > 3))
            {
                throw new InvalidOperationException($"Found invalid cluster values: {string.Join(", ", uniqueClusters.Where(c => c > 3))}. Expected clusters: 0, 1, 2, 3.");
            }

            // Вывод распределения классов
            Console.WriteLine("\nClass distribution in dataset:");
            var classCounts = dataEnumerable.GroupBy(d => d.Cluster)
                .Select(g => new { Cluster = g.Key, Count = g.Count() });
            foreach (var group in classCounts.OrderBy(g => g.Cluster))
            {
                Console.WriteLine($"Cluster {group.Cluster}: {group.Count} records");
            }

            // Вывод диапазонов значений признаков
            Console.WriteLine("\nFeature ranges in dataset:");
            Console.WriteLine($"PM10: [{dataEnumerable.Min(d => d.PM10)} - {dataEnumerable.Max(d => d.PM10)}]");
            Console.WriteLine($"PM2_5: [{dataEnumerable.Min(d => d.PM2_5)} - {dataEnumerable.Max(d => d.PM2_5)}]");
            Console.WriteLine($"CO: [{dataEnumerable.Min(d => d.CO)} - {dataEnumerable.Max(d => d.CO)}]");
            Console.WriteLine($"NO2: [{dataEnumerable.Min(d => d.NO2)} - {dataEnumerable.Max(d => d.NO2)}]");
            Console.WriteLine($"O3: [{dataEnumerable.Min(d => d.O3)} - {dataEnumerable.Max(d => d.O3)}]");
            Console.WriteLine($"SO2: [{dataEnumerable.Min(d => d.SO2)} - {dataEnumerable.Max(d => d.SO2)}]");

            // Сейв максимальных значений признаков
            _maxFeatureValues = new Dictionary<string, float>
            {
                { "Твёрдые микрочастицы диаметром 10 мкм", dataEnumerable.Max(d => d.PM10) },
                { "Твёрдые микрочастицы диаметром 2.5 мкм", dataEnumerable.Max(d => d.PM2_5) },
                { "Монооксид углерода", dataEnumerable.Max(d => d.CO) },
                { "Диоксид азота", dataEnumerable.Max(d => d.NO2) },
                { "Озон", dataEnumerable.Max(d => d.O3) },
                { "Сернистый газ", dataEnumerable.Max(d => d.SO2) }
            };

            Console.WriteLine("\nMaximum Feature Values:");
            foreach (var maxValue in _maxFeatureValues)
            {
                Console.WriteLine($"{maxValue.Key}: {maxValue.Value}");
            }

            // Деление данных 60/40 обучающая/тестовая
            var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.6);
            var trainData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            var trainDataCount = _mlContext.Data.CreateEnumerable<AirQualityData>(trainData, reuseRowObject: false).Count();
            var testDataCount = _mlContext.Data.CreateEnumerable<AirQualityData>(testData, reuseRowObject: false).Count();
            Console.WriteLine($"\nTraining data count: {trainDataCount}");
            Console.WriteLine($"Test data count: {testDataCount}");
            if (trainDataCount < 10 || testDataCount < 5)
            {
                throw new InvalidOperationException("Not enough data for training and testing. Need at least 10 training records and 5 test records.");
            }

            // Пайплайн для классификации (SdcaMaximumEntropy)
            var pipeline = _mlContext.Transforms.Concatenate("Features", "PM10", "PM2_5", "CO", "NO2", "O3", "SO2")
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", "Cluster"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

            // Обучение модели
            try
            {
                _model = pipeline.Fit(trainData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while training model: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }

            // Оценка качества модели
            try
            {
                var predictions = _model.Transform(testData);
                var metrics = _mlContext.MulticlassClassification.Evaluate(predictions, labelColumnName: "Label");

                // Метрики качества
                Console.WriteLine("\nModel Evaluation Metrics:");
                Console.WriteLine($"MicroAccuracy: {metrics.MicroAccuracy:F3}");
                Console.WriteLine($"MacroAccuracy: {metrics.MacroAccuracy:F3}");
                Console.WriteLine($"LogLoss: {metrics.LogLoss:F3}");

                // Confusion Matrix
                Console.WriteLine("\nConfusion Matrix:");
                var confusionMatrix = metrics.ConfusionMatrix;
                Console.WriteLine("True\\Predicted");
                Console.WriteLine($"    {string.Join(" ", Enumerable.Range(0, confusionMatrix.NumberOfClasses).Select(i => i.ToString().PadLeft(5)))}");
                for (int i = 0; i < confusionMatrix.NumberOfClasses; i++)
                {
                    var row = new double[confusionMatrix.NumberOfClasses];
                    for (int j = 0; j < confusionMatrix.NumberOfClasses; j++)
                    {
                        row[j] = confusionMatrix.GetCountForClassPair(i, j); 
                    }
                    Console.WriteLine($"{i.ToString().PadLeft(2)} {string.Join(" ", row.Select(x => x.ToString().PadLeft(5)))}");
                }

                // Вычисление Precision, Recall и F1-Score для каждого класса
                Console.WriteLine("\nPer-class Metrics:");
                for (int i = 0; i < confusionMatrix.NumberOfClasses; i++)
                {
                    // True Positives — диагональный элемент
                    double tp = confusionMatrix.GetCountForClassPair(i, i);

                    // False Positives (FP) — сумма элементов в столбце i, кроме TP
                    double fp = 0;
                    for (int k = 0; k < confusionMatrix.NumberOfClasses; k++)
                    {
                        if (k != i) fp += confusionMatrix.GetCountForClassPair(k, i);
                    }

                    // False Negatives (FN) — сумма элементов в строке i, кроме TP
                    double fn = 0;
                    for (int k = 0; k < confusionMatrix.NumberOfClasses; k++)
                    {
                        if (k != i) fn += confusionMatrix.GetCountForClassPair(i, k);
                    }

                    // Precision = TP / (TP + FP)
                    double precision = (tp + fp) > 0 ? tp / (tp + fp) : 0;

                    // Recall = TP / (TP + FN)
                    double recall = (tp + fn) > 0 ? tp / (tp + fn) : 0;

                    // F1-Score = 2 * (Precision * Recall) / (Precision + Recall)
                    double f1Score = (precision + recall) > 0 ? 2 * precision * recall / (precision + recall) : 0;

                    Console.WriteLine($"Class {i} ({_clusterToLevelMapping[(uint)i]}):");
                    Console.WriteLine($"  Precision: {precision:F3}");
                    Console.WriteLine($"  Recall: {recall:F3}");
                    Console.WriteLine($"  F1-Score: {f1Score:F3}");
                }

                // Сохранение данных с предсказаниями в predicted_data.csv
                var predictedData = _mlContext.Data.CreateEnumerable<AirQualityDataWithPrediction>(predictions, reuseRowObject: false).ToList();
                using (var writer = new StreamWriter("predicted_data.csv"))
                {
                    writer.WriteLine("PM10,PM2_5,CO,NO2,O3,SO2,TrueCluster,PredictedCluster");
                    foreach (var pred in predictedData)
                    {
                        writer.WriteLine($"{pred.PM10},{pred.PM2_5},{pred.CO},{pred.NO2},{pred.O3},{pred.SO2},{pred.Cluster},{pred.PredictedLabel}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while evaluating model: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private class AirQualityDataWithPrediction : AirQualityData
        {
            public uint PredictedLabel { get; set; }
        }

        public string Predict(Dictionary<string, float?> inputFeatures)
        {
            // Есть ли экстремально большие значения
            bool isExtreme = false;
            foreach (var feature in inputFeatures)
            {
                if (feature.Value.HasValue && _maxFeatureValues.ContainsKey(feature.Key))
                {
                    float threshold = _maxFeatureValues[feature.Key] * 2;
                    if (feature.Value.Value > threshold)
                    {
                        isExtreme = true;
                        break;
                    }
                }
            }

            if (isExtreme)
            {
                Console.WriteLine("Extreme value detected, assigning to 'Высокий уровень загрязнённости воздуха'");
                return "Высокий уровень загрязнённости воздуха";
            }

            // Ограничиваю входные значения максимальными значениями из датасета
            var cappedInputFeatures = new Dictionary<string, float?>();
            foreach (var feature in inputFeatures)
            {
                if (feature.Value.HasValue && _maxFeatureValues.ContainsKey(feature.Key))
                {
                    cappedInputFeatures[feature.Key] = Math.Min(feature.Value.Value, _maxFeatureValues[feature.Key] * 5);
                }
                else
                {
                    cappedInputFeatures[feature.Key] = feature.Value;
                }
            }

            // Создаю объект для предсказания
            var predictionInput = new AirQualityData
            {
                PM10 = cappedInputFeatures.ContainsKey("Твёрдые микрочастицы диаметром 10 мкм") && cappedInputFeatures["Твёрдые микрочастицы диаметром 10 мкм"].HasValue ? cappedInputFeatures["Твёрдые микрочастицы диаметром 10 мкм"].Value : 0f,
                PM2_5 = cappedInputFeatures.ContainsKey("Твёрдые микрочастицы диаметром 2.5 мкм") && cappedInputFeatures["Твёрдые микрочастицы диаметром 2.5 мкм"].HasValue ? cappedInputFeatures["Твёрдые микрочастицы диаметром 2.5 мкм"].Value : 0f,
                CO = cappedInputFeatures.ContainsKey("Монооксид углерода") && cappedInputFeatures["Монооксид углерода"].HasValue ? cappedInputFeatures["Монооксид углерода"].Value : 0f,
                NO2 = cappedInputFeatures.ContainsKey("Диоксид азота") && cappedInputFeatures["Диоксид азота"].HasValue ? cappedInputFeatures["Диоксид азота"].Value : 0f,
                O3 = cappedInputFeatures.ContainsKey("Озон") && cappedInputFeatures["Озон"].HasValue ? cappedInputFeatures["Озон"].Value : 0f,
                SO2 = cappedInputFeatures.ContainsKey("Сернистый газ") && cappedInputFeatures["Сернистый газ"].HasValue ? cappedInputFeatures["Сернистый газ"].Value : 0f
            };

            // Для отладки
            Console.WriteLine($"Input Values - PM10: {predictionInput.PM10}, PM2_5: {predictionInput.PM2_5}, CO: {predictionInput.CO}, NO2: {predictionInput.NO2}, O3: {predictionInput.O3}, SO2: {predictionInput.SO2}");

            // Движок предсказания
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<AirQualityData, AirQualityPrediction>(_model);

            // Предсказание
            var prediction = predictionEngine.Predict(predictionInput);

            // Вероятности для каждого класса
            Console.WriteLine($"Prediction Probabilities: {string.Join(", ", prediction.Scores)}");
            Console.WriteLine($"Predicted Cluster ID: {prediction.PredictedLabel}");

            // Есть ли метка в словаре
            if (!_clusterToLevelMapping.ContainsKey(prediction.PredictedLabel))
            {
                throw new KeyNotFoundException($"Cluster ID {prediction.PredictedLabel} not found in mapping. Available clusters: {string.Join(", ", _clusterToLevelMapping.Keys)}");
            }

            return _clusterToLevelMapping[prediction.PredictedLabel];
        }

        public bool CanPredict(Dictionary<int, string> inputValues, List<Dictionary<int, string>> features)
        {
            var inputFeatureNames = new List<string>();
            foreach (var kv in inputValues)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    var feature = features.FirstOrDefault(f => f.Keys.First() == kv.Key);
                    if (feature != null)
                    {
                        inputFeatureNames.Add(feature.Values.First());
                    }
                }
            }

            return inputFeatureNames.Any(name => _requiredFeatures.Contains(name));
        }
    }
}