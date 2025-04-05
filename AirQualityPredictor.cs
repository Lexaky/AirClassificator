using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    }

    public class AirQualityPrediction
    {
        [ColumnName("Score")]
        public float[] Scores { get; set; }

        public uint ClusterId
        {
            get
            {
                if (Scores == null || Scores.Length == 0)
                    throw new InvalidOperationException("Scores are not available for prediction.");
                return (uint)Array.IndexOf(Scores, Scores.Max());
            }
        }
    }

    public class AirQualityPredictor
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string[] _requiredFeatures = { "Твёрдые микрочастицы диаметром 2.5 мкм", "Твёрдые микрочастицы диаметром 10 мкм", "Озон", "Диоксид азота", "Сернистый газ", "Монооксид углерода" };
        private readonly string[] _airQualityLevels = { "Высокий уровень загрязнённости воздуха", "Средний уровень загрязнённости воздуха", "Низкий уровень загрязнённости воздуха", "Оптимальное состояние воздуха" };
        private Dictionary<uint, string> _clusterToLevelMapping;
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

            // Загружаем данные
            var data = _mlContext.Data.LoadFromTextFile<AirQualityData>(
                path: datasetPath,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // Сохраняем максимальные значения признаков
            var dataEnumerable = _mlContext.Data.CreateEnumerable<AirQualityData>(data, reuseRowObject: false).ToList();
            _maxFeatureValues = new Dictionary<string, float>
            {
                { "Твёрдые микрочастицы диаметром 10 мкм", dataEnumerable.Max(d => d.PM10) },
                { "Твёрдые микрочастицы диаметром 2.5 мкм", dataEnumerable.Max(d => d.PM2_5) },
                { "Монооксид углерода", dataEnumerable.Max(d => d.CO) },
                { "Диоксид азота", dataEnumerable.Max(d => d.NO2) },
                { "Озон", dataEnumerable.Max(d => d.O3) },
                { "Сернистый газ", dataEnumerable.Max(d => d.SO2) }
            };

            Console.WriteLine("Maximum Feature Values:");
            foreach (var maxValue in _maxFeatureValues)
            {
                Console.WriteLine($"{maxValue.Key}: {maxValue.Value}");
            }

            // Создаём пайплайн для кластеризации
            var pipeline = _mlContext.Transforms.Concatenate("Features", "PM10", "PM2_5", "CO", "NO2", "O3", "SO2")
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 4));

            // Обучаем модель
            _model = pipeline.Fit(data);

            // После обучения сопоставляем кластеры с уровнями загрязнённости
            MapClustersToLevels(data);
        }

        private void MapClustersToLevels(IDataView data)
        {
            // Делаем предсказания для всех данных, чтобы получить кластеры
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<AirQualityData, AirQualityPrediction>(_model);
            var predictions = _mlContext.Data.CreateEnumerable<AirQualityPrediction>(_model.Transform(data), reuseRowObject: false).ToList();
            var originalData = _mlContext.Data.CreateEnumerable<AirQualityData>(data, reuseRowObject: false).ToList();

            // Собираем данные по кластерам
            var clusterData = new Dictionary<uint, List<AirQualityData>>();
            for (int i = 0; i < predictions.Count; i++)
            {
                uint clusterId = predictions[i].ClusterId;
                if (!clusterData.ContainsKey(clusterId))
                {
                    clusterData[clusterId] = new List<AirQualityData>();
                }
                clusterData[clusterId].Add(originalData[i]);
            }

            // Вычисляем средние значения признаков для каждого кластера и выводим информацию
            var clusterAverages = new Dictionary<uint, float>();
            Console.WriteLine("\nCluster Analysis:");
            foreach (var cluster in clusterData)
            {
                uint clusterId = cluster.Key;
                var clusterPoints = cluster.Value;

                // Вычисляем средние значения для каждого признака
                float avgPM10 = clusterPoints.Average(d => d.PM10);
                float avgPM2_5 = clusterPoints.Average(d => d.PM2_5);
                float avgCO = clusterPoints.Average(d => d.CO);
                float avgNO2 = clusterPoints.Average(d => d.NO2);
                float avgO3 = clusterPoints.Average(d => d.O3);
                float avgSO2 = clusterPoints.Average(d => d.SO2);

                // Суммируем средние значения всех признаков для оценки "загрязнённости"
                float totalPollution = avgPM10 + avgPM2_5 + avgCO + avgNO2 + avgO3 + avgSO2;
                clusterAverages[clusterId] = totalPollution;

                // Выводим информацию о кластере
                Console.WriteLine($"\nCluster {clusterId}:");
                Console.WriteLine($"Number of points: {clusterPoints.Count}");
                Console.WriteLine($"Average PM10: {avgPM10:F2}");
                Console.WriteLine($"Average PM2_5: {avgPM2_5:F2}");
                Console.WriteLine($"Average CO: {avgCO:F2}");
                Console.WriteLine($"Average NO2: {avgNO2:F2}");
                Console.WriteLine($"Average O3: {avgO3:F2}");
                Console.WriteLine($"Average SO2: {avgSO2:F2}");
                Console.WriteLine($"Total Pollution Score: {totalPollution:F2}");
            }

            // Сортируем кластеры по среднему уровню загрязнённости (от большего к меньшему)
            var sortedClusters = clusterAverages.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();

            // Сопоставляем кластеры с уровнями загрязнённости
            _clusterToLevelMapping = new Dictionary<uint, string>();

            // Убедимся, что все кластеры от 0 до 3 имеют сопоставление
            for (uint i = 0; i < 4; i++)
            {
                int levelIndex = sortedClusters.IndexOf(i);
                if (levelIndex >= 0)
                {
                    _clusterToLevelMapping[i] = _airQualityLevels[levelIndex];
                }
                else
                {
                    _clusterToLevelMapping[i] = _airQualityLevels[_airQualityLevels.Length - 1]; // "Оптимальное состояние воздуха"
                }
            }

            // Выводим сопоставление кластеров с уровнями загрязнённости
            Console.WriteLine("\nCluster to Level Mapping:");
            foreach (var mapping in _clusterToLevelMapping)
            {
                Console.WriteLine($"Cluster {mapping.Key} -> {mapping.Value}");
            }
        }

        public string Predict(Dictionary<string, float?> inputFeatures)
        {
            // Проверяем, есть ли экстремально большие значения
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

            // Ограничиваем входные значения максимальными значениями из датасета
            var cappedInputFeatures = new Dictionary<string, float?>();
            foreach (var feature in inputFeatures)
            {
                if (feature.Value.HasValue && _maxFeatureValues.ContainsKey(feature.Key))
                {
                    cappedInputFeatures[feature.Key] = Math.Min(feature.Value.Value, _maxFeatureValues[feature.Key]);
                }
                else
                {
                    cappedInputFeatures[feature.Key] = feature.Value;
                }
            }

            // Создаём объект для предсказания
            var predictionInput = new AirQualityData
            {
                PM10 = cappedInputFeatures.ContainsKey("Твёрдые микрочастицы диаметром 10 мкм") && cappedInputFeatures["Твёрдые микрочастицы диаметром 10 мкм"].HasValue ? cappedInputFeatures["Твёрдые микрочастицы диаметром 10 мкм"].Value : 0f,
                PM2_5 = cappedInputFeatures.ContainsKey("Твёрдые микрочастицы диаметром 2.5 мкм") && cappedInputFeatures["Твёрдые микрочастицы диаметром 2.5 мкм"].HasValue ? cappedInputFeatures["Твёрдые микрочастицы диаметром 2.5 мкм"].Value : 0f,
                CO = cappedInputFeatures.ContainsKey("Монооксид углерода") && cappedInputFeatures["Монооксид углерода"].HasValue ? cappedInputFeatures["Монооксид углерода"].Value : 0f,
                NO2 = cappedInputFeatures.ContainsKey("Диоксид азота") && cappedInputFeatures["Диоксид азота"].HasValue ? cappedInputFeatures["Диоксид азота"].Value : 0f,
                O3 = cappedInputFeatures.ContainsKey("Озон") && cappedInputFeatures["Озон"].HasValue ? cappedInputFeatures["Озон"].Value : 0f,
                SO2 = cappedInputFeatures.ContainsKey("Сернистый газ") && cappedInputFeatures["Сернистый газ"].HasValue ? cappedInputFeatures["Сернистый газ"].Value : 0f
            };

            // Добавляем отладочный вывод для всех признаков
            Console.WriteLine($"Input Values - PM10: {predictionInput.PM10}, PM2_5: {predictionInput.PM2_5}, CO: {predictionInput.CO}, NO2: {predictionInput.NO2}, O3: {predictionInput.O3}, SO2: {predictionInput.SO2}");

            // Создаём движок предсказания
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<AirQualityData, AirQualityPrediction>(_model);

            // Делаем предсказание
            var prediction = predictionEngine.Predict(predictionInput);

            // Выводим отладочную информацию
            Console.WriteLine($"Predicted Cluster ID: {prediction.ClusterId}");

            // Проверяем, есть ли кластер в словаре
            if (!_clusterToLevelMapping.ContainsKey(prediction.ClusterId))
            {
                throw new KeyNotFoundException($"Cluster ID {prediction.ClusterId} not found in mapping. Available clusters: {string.Join(", ", _clusterToLevelMapping.Keys)}");
            }

            return _clusterToLevelMapping[prediction.ClusterId];
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