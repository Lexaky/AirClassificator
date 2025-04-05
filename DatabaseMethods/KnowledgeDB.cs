using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;
using System.Windows.Forms;
using System.Linq.Expressions;

namespace AirClassificator.DatabaseMethods
{
    internal class KnowledgeDB
    {
        private string connectionString;
        public KnowledgeDB()
        {
            connectionString = ConfigurationManager.ConnectionStrings["ConnectionToKnowledgeBase"].ConnectionString;
            //TestDatabaseConnection(); // Проверка подключения к бд knowledge PostgreSQL
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Подключение к базе данных успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Метод для получения данных о всех хранимых названиях уровней загрязнённостей воздуха и их id в БД
        /// </summary>
        public List<Dictionary<int, string>> GetAllAirLevels()
        {
            List<Dictionary<int, string>> result = new List<Dictionary<int, string>>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM airlevels ORDER BY id;";
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<int, string> item = new Dictionary<int, string>();
                            item.Add(reader.GetInt32(0), reader.GetString(1)); // id, air_level_name
                            result.Add(item);
                            // Убираем item.Clear(), так как это очищает словарь перед добавлением
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении уровней воздуха: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            return result;
        }

        /// <summary>
        /// Метод для получения названия уровня загрязнённости воздуха по id
        /// </summary>
        public string GetAirLevelById(int id)
        {
            string item = "-1";
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    // SQL-запрос для получения данных из таблицы KnowledgeItems
                    string query = "SELECT name FROM airlevels WHERE id = " + id.ToString() + ";";
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            item = reader.GetString(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении id воздуха: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            return item;
        }

        /// <summary>
        /// Удаление уровня воздуха по id из БД
        /// </summary>
        public void DeleteAirLevelById(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Удаляем все записи из description_features_of_levels, связанные с этим уровнем
                    string deleteDescriptionQuery = "DELETE FROM description_features_of_levels WHERE level_id = @id;";
                    using (var command = new NpgsqlCommand(deleteDescriptionQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }

                    // Удаляем все записи из level_to_feature_value_marked, связанные с этим уровнем
                    string deleteMarkedQuery = "DELETE FROM level_to_feature_value_marked WHERE level_id = @id;";
                    using (var command = new NpgsqlCommand(deleteMarkedQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }

                    // Удаляем сам уровень из airlevels
                    string deleteLevelQuery = "DELETE FROM airlevels WHERE id = @id;";
                    using (var command = new NpgsqlCommand(deleteLevelQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении уровня воздуха: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddAirLevel(string airLevelName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO airlevels (name) VALUES (@name);";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("name", airLevelName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении уровня воздуха: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<Dictionary<int, string>> GetAllFeatures()
        {
            List<Dictionary<int, string>> result = new List<Dictionary<int, string>>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM features ORDER BY id;";
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<int, string> item = new Dictionary<int, string>();
                            item.Add(reader.GetInt32(0), reader.GetString(1)); // id, name
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении свойств: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            return result;
        }

        public void DeleteFeatureById(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем все id значений из feature_values для этого свойства
                    List<int> featureValueIds = new List<int>();
                    List<Dictionary<string, string>> featureValues = GetFeatureValues(id);
                    foreach (var value in featureValues)
                    {
                        featureValueIds.Add(int.Parse(value["id"]));
                    }

                    // Удаляем все записи из level_to_feature_value_marked, связанные с этими значениями
                    if (featureValueIds.Count > 0)
                    {
                        string deleteMarkedQuery = "DELETE FROM level_to_feature_value_marked WHERE marked_feature_value_id = ANY(@ids);";
                        using (var command = new NpgsqlCommand(deleteMarkedQuery, connection))
                        {
                            command.Parameters.AddWithValue("ids", featureValueIds.ToArray());
                            command.ExecuteNonQuery();
                        }
                    }

                    // Удаляем все записи из feature_values, связанные с этим свойством
                    string deleteValuesQuery = "DELETE FROM feature_values WHERE feature_id = @id;";
                    using (var command = new NpgsqlCommand(deleteValuesQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }

                    // Удаляем все записи из description_features_of_levels, связанные с этим свойством
                    string deleteDescriptionQuery = "DELETE FROM description_features_of_levels WHERE feature_id = @id;";
                    using (var command = new NpgsqlCommand(deleteDescriptionQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }

                    // Удаляем само свойство из features
                    string deleteFeatureQuery = "DELETE FROM features WHERE id = @id;";
                    using (var command = new NpgsqlCommand(deleteFeatureQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении свойства: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddFeature(string featureName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO features (name) VALUES (@name);";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("name", featureName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении свойства: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<Dictionary<string, string>> GetFeatureValues(int featureId)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM feature_values WHERE feature_id = @featureId ORDER BY id;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("featureId", featureId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new Dictionary<string, string>
                        {
                            { "feature_id", reader.GetInt32(0).ToString() },
                            { "minvalue", reader.IsDBNull(1) ? null : reader.GetString(1) },
                            { "maxvalue", reader.IsDBNull(2) ? null : reader.GetString(2) },
                            { "typevalue", reader.GetString(3) },
                            { "constantvalue", reader.IsDBNull(4) ? null : reader.GetString(4) },
                            { "id", reader.GetInt32(5).ToString() }
                        };
                                result.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении значений свойства: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }

        public void AddFeatureValue(int featureId, string minValue, string maxValue, string typeValue, string constantValue)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO feature_values (feature_id, minvalue, maxvalue, typevalue, constantvalue) VALUES (@featureId, @minValue, @maxValue, @typeValue, @constantValue);";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("featureId", featureId);
                        command.Parameters.AddWithValue("minValue", (object)minValue ?? DBNull.Value);
                        command.Parameters.AddWithValue("maxValue", (object)maxValue ?? DBNull.Value);
                        command.Parameters.AddWithValue("typeValue", typeValue);
                        command.Parameters.AddWithValue("constantValue", (object)constantValue ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении значения свойства: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteFeatureValue(int featureId, int valueId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Удаляем записи из level_to_feature_value_marked с этим marked_feature_value_id
                    string deleteMarkedQuery = "DELETE FROM level_to_feature_value_marked WHERE marked_feature_value_id = @valueId;";
                    using (var command = new NpgsqlCommand(deleteMarkedQuery, connection))
                    {
                        command.Parameters.AddWithValue("valueId", valueId);
                        command.ExecuteNonQuery();
                    }

                    // Удаляем само значение из feature_values
                    string query = "DELETE FROM feature_values WHERE feature_id = @featureId AND id = @valueId;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("featureId", featureId);
                        command.Parameters.AddWithValue("valueId", valueId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении значения свойства: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddFeatureToLevel(int levelId, int featureId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO description_features_of_levels (level_id, feature_id) VALUES (@levelId, @featureId) ON CONFLICT DO NOTHING;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        command.Parameters.AddWithValue("featureId", featureId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении свойства к уровню: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RemoveFeatureFromLevel(int levelId, int featureId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM description_features_of_levels WHERE level_id = @levelId AND feature_id = @featureId;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        command.Parameters.AddWithValue("featureId", featureId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении свойства из уровня: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<int> GetFeaturesForLevel(int levelId)
        {
            List<int> featureIds = new List<int>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT feature_id FROM description_features_of_levels WHERE level_id = @levelId;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                featureIds.Add(reader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении свойств уровня: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return featureIds;
        }

        public void AddLevelToFeatureValueMarked(int levelId, int markedFeatureValueId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO level_to_feature_value_marked (level_id, marked_feature_value_id) VALUES (@levelId, @markedFeatureValueId) ON CONFLICT DO NOTHING;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        command.Parameters.AddWithValue("markedFeatureValueId", markedFeatureValueId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении значения для уровня: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RemoveLevelToFeatureValueMarked(int levelId, int markedFeatureValueId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM level_to_feature_value_marked WHERE level_id = @levelId AND marked_feature_value_id = @markedFeatureValueId;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        command.Parameters.AddWithValue("markedFeatureValueId", markedFeatureValueId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении значения для уровня: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<int> GetMarkedFeatureValuesForLevel(int levelId)
        {
            List<int> markedFeatureValueIds = new List<int>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT marked_feature_value_id FROM level_to_feature_value_marked WHERE level_id = @levelId;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("levelId", levelId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                markedFeatureValueIds.Add(reader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении отмеченных значений для уровня: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return markedFeatureValueIds;
        }

        public List<Dictionary<int, string>> GetFeaturesWithoutValues()
        {
            List<Dictionary<int, string>> featuresWithoutValues = new List<Dictionary<int, string>>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT f.id, f.name 
                FROM features f
                LEFT JOIN feature_values fv ON f.id = fv.feature_id
                WHERE fv.feature_id IS NULL;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var feature = new Dictionary<int, string>
                        {
                            { reader.GetInt32(0), reader.GetString(1) }
                        };
                                featuresWithoutValues.Add(feature);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении свойств без значений: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return featuresWithoutValues;
        }

        public List<Dictionary<int, string>> GetLevelsWithoutFeatures()
        {
            List<Dictionary<int, string>> levelsWithoutFeatures = new List<Dictionary<int, string>>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT al.id, al.name 
                FROM airlevels al
                LEFT JOIN description_features_of_levels dfl ON al.id = dfl.level_id
                WHERE dfl.level_id IS NULL;";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var level = new Dictionary<int, string>
                        {
                            { reader.GetInt32(0), reader.GetString(1) }
                        };
                                levelsWithoutFeatures.Add(level);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении уровней без свойств: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return levelsWithoutFeatures;
        }


    }
}
