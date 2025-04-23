using AirClassificator.DatabaseMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirClassificator
{
    public partial class AirPollutionResultForm : Form
    {
        private KnowledgeDB kdb;
        private Dictionary<int, string> inputValues; // Введённые пользователем значения
        private Label allClassesRefutedLabel; // Лейбл для текста "Все классы были опровергнуты"

        public AirPollutionResultForm(Dictionary<int, string> inputValues)
        {
            InitializeComponent();
            this.kdb = new KnowledgeDB();
            this.inputValues = inputValues;
            InitializeControls();
            DetermineAirPollutionLevels();
        }

        private void InitializeControls()
        {
            this.Text = "Результат определения уровня загрязнённости воздуха";
            this.Size = new Size(800, 600);

            // Текст "Все классы были опровергнуты"
            allClassesRefutedLabel = new Label
            {
                Text = "Все классы были опровергнуты",
                AutoSize = true,
                Location = new Point(10, 10),
                ForeColor = Color.Red,
                Visible = false
            };
            this.Controls.Add(allClassesRefutedLabel);

            // Таблица "Подходящие уровни загрязнённости воздуха"
            Label suitableLabel = new Label
            {
                Text = "Подходящие уровни загрязнённости воздуха",
                AutoSize = true,
                Location = new Point(10, 40)
            };
            this.Controls.Add(suitableLabel);

            DataGridView suitableGrid = new DataGridView
            {
                Location = new Point(10, 70),
                Size = new Size(760, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            suitableGrid.Columns.Add("LevelName", "Уровень воздуха");
            suitableGrid.Columns.Add("Reasons", "Причины");
            suitableGrid.Columns["LevelName"].Width = 200;
            suitableGrid.Columns["Reasons"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            suitableGrid.Tag = "SuitableGrid";
            this.Controls.Add(suitableGrid);

            // Таблица "Не подходящие уровни загрязнённости воздуха"
            Label unsuitableLabel = new Label
            {
                Text = "Не подходящие уровни загрязнённости воздуха",
                AutoSize = true,
                Location = new Point(10, 290)
            };
            this.Controls.Add(unsuitableLabel);

            DataGridView unsuitableGrid = new DataGridView
            {
                Location = new Point(10, 320),
                Size = new Size(760, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            unsuitableGrid.Columns.Add("LevelName", "Уровень воздуха");
            unsuitableGrid.Columns.Add("Reasons", "Причины");
            unsuitableGrid.Columns["LevelName"].Width = 200;
            unsuitableGrid.Columns["Reasons"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            unsuitableGrid.Tag = "UnsuitableGrid";
            this.Controls.Add(unsuitableGrid);

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Назад",
                Location = new Point(10, 530),
                Size = new Size(100, 30)
            };
            backButton.Click += (s, e) => this.Close();
            this.Controls.Add(backButton);
        }

        private void DetermineAirPollutionLevels()
        {
            DataGridView suitableGrid = this.Controls.OfType<DataGridView>().First(g => g.Tag.ToString() == "SuitableGrid");
            DataGridView unsuitableGrid = this.Controls.OfType<DataGridView>().First(g => g.Tag.ToString() == "UnsuitableGrid");
            Label suitableLabel = this.Controls.OfType<Label>().First(l => l.Text == "Подходящие уровни загрязнённости воздуха");

            List<Dictionary<int, string>> airLevels = kdb.GetAllAirLevels();
            List<Dictionary<int, string>> features = kdb.GetAllFeatures();

            Dictionary<int, List<string>> suitableLevels = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> unsuitableLevels = new Dictionary<int, List<string>>();

            // Проверяю, ввёл ли пользователь хотя бы одно значение
            bool hasAnyInput = inputValues.Any(kv => kv.Value != null);

            if (!hasAnyInput)
            {
                foreach (var level in airLevels)
                {
                    int levelId = level.Keys.First();
                    string levelName = level.Values.First();
                    unsuitableLevels[levelId] = new List<string> { "Значения свойств не заданы" };
                }
            }
            else
            {
                foreach (var level in airLevels)
                {
                    int levelId = level.Keys.First();
                    string levelName = level.Values.First();
                    List<string> unsuitableReasons = new List<string>();
                    List<string> suitableReasons = new List<string>(); // Для хранения причин, почему уровень подходит
                    bool levelIsSuitable = true;

                    List<int> assignedFeatureIds = kdb.GetFeaturesForLevel(levelId);
                    foreach (int featureId in assignedFeatureIds)
                    {
                        if (!inputValues.ContainsKey(featureId) || inputValues[featureId] == null)
                            continue;

                        string userValue = inputValues[featureId];
                        List<Dictionary<string, string>> featureValues = kdb.GetFeatureValues(featureId);
                        List<int> markedFeatureValueIds = kdb.GetMarkedFeatureValuesForLevel(levelId);

                        bool isUserValueNumeric = double.TryParse(userValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double numericUserValue);
                        if (!isUserValueNumeric)
                        {
                            Console.WriteLine($"Failed to parse userValue: '{userValue}' for featureId: {featureId}. Expected a numeric value with a dot as the decimal separator (e.g., 15.2).");
                        }
                        else
                        {
                            Console.WriteLine($"Successfully parsed userValue: '{userValue}' to numericUserValue: {numericUserValue} for featureId: {featureId}");
                        }

                        bool valueMatches = false;
                        string unsuitableReason = null;
                        string suitableReason = null;

                        foreach (var value in featureValues)
                        {
                            int valueId = int.Parse(value["id"]);
                            if (!markedFeatureValueIds.Contains(valueId))
                                continue;

                            if (value["typevalue"] == "num")
                            {
                                Console.WriteLine($"Processing numeric value check for featureId: {featureId}");
                                Console.WriteLine($"value['minvalue']: '{value["minvalue"]}', value['maxvalue']: '{value["maxvalue"]}");

                                if (string.IsNullOrWhiteSpace(value["minvalue"]))
                                {
                                    Console.WriteLine("minvalue is empty or null.");
                                    throw new ArgumentException($"minvalue is empty or null for featureId {featureId}.");
                                }
                                if (string.IsNullOrWhiteSpace(value["maxvalue"]))
                                {
                                    Console.WriteLine("maxvalue is empty or null.");
                                    throw new ArgumentException($"maxvalue is empty or null for featureId {featureId}.");
                                }

                                double minValue, maxValue;
                                if (!double.TryParse(value["minvalue"], NumberStyles.Float, CultureInfo.InvariantCulture, out minValue))
                                {
                                    Console.WriteLine($"Failed to parse minvalue: '{value["minvalue"]}'");
                                    throw new ArgumentException($"Cannot parse minvalue '{value["minvalue"]}' to double for featureId {featureId}.");
                                }
                                if (!double.TryParse(value["maxvalue"], NumberStyles.Float, CultureInfo.InvariantCulture, out maxValue))
                                {
                                    Console.WriteLine($"Failed to parse maxvalue: '{value["maxvalue"]}'");
                                    throw new ArgumentException($"Cannot parse maxvalue '{value["maxvalue"]}' to double for featureId {featureId}.");
                                }
                                Console.WriteLine($"Parsed minValue: {minValue}, maxValue: {maxValue}");

                                // Проверка существования признака
                                var feature = features.FirstOrDefault(f => f.Keys.First() == featureId);
                                if (feature == null)
                                {
                                    Console.WriteLine($"No feature found with featureId {featureId}. Features: {string.Join(", ", features.Select(f => $"[{f.Keys.First()}: {f.Values.First()}]"))}");
                                    throw new ArgumentException($"Feature with ID {featureId} not found in features.");
                                }
                                string featureName = feature.Values.First();
                                Console.WriteLine($"Feature name for featureId {featureId}: {featureName}");

                                Console.WriteLine($"isUserValueNumeric: {isUserValueNumeric}, numericUserValue: {numericUserValue}");
                                if (isUserValueNumeric && numericUserValue >= minValue && numericUserValue <= maxValue)
                                {
                                    valueMatches = true;
                                    suitableReason = $"Значение свойства '{featureName}' ({userValue}) находится в диапазоне от {minValue} до {maxValue}";
                                    Console.WriteLine($"Value matches: {suitableReason}");
                                    break;
                                }
                                else if (!isUserValueNumeric)
                                {
                                    unsuitableReason = $"Наличие свойства '{featureName}' со значением '{userValue}' (ожидается числовое значение)";
                                    Console.WriteLine($"Value does not match (not numeric): {unsuitableReason}");
                                }
                                else
                                {
                                    unsuitableReason = $"Наличие свойства '{featureName}' со значением {userValue} (ожидается значение в диапазоне от {minValue} до {maxValue})";
                                    Console.WriteLine($"Value does not match (out of range): {unsuitableReason}");
                                }
                            }
                            else if (value["typevalue"] == "enum")
                            {
                                string enumValue = value["constantvalue"];
                                if (userValue == enumValue)
                                {
                                    valueMatches = true;
                                    suitableReason = $"Значение свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' ({userValue}) соответствует допустимому значению '{enumValue}'";
                                    break;
                                }
                                else
                                {
                                    unsuitableReason = $"Наличие свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' со значением '{userValue}' (ожидается значение '{enumValue}')";
                                }
                            }
                        }

                        if (!valueMatches)
                        {
                            levelIsSuitable = false;
                            if (unsuitableReason != null)
                            {
                                unsuitableReasons.Add(unsuitableReason);
                            }
                            else
                            {
                                unsuitableReasons.Add($"Наличие свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' со значением '{userValue}'");
                            }
                        }
                        else
                        {
                            suitableReasons.Add(suitableReason);
                        }
                    }

                    if (!levelIsSuitable)
                    {
                        unsuitableLevels[levelId] = unsuitableReasons;
                    }
                    else
                    {
                        suitableLevels[levelId] = suitableReasons.Count > 0 ? suitableReasons : new List<string> { "Все введённые значения соответствуют допустимым" };
                    }
                }
            }

            // Заполнение таблицы неподходящих уровней
            foreach (var level in airLevels)
            {
                int levelId = level.Keys.First();
                string levelName = level.Values.First();
                if (unsuitableLevels.ContainsKey(levelId))
                {
                    unsuitableGrid.Rows.Add(levelName, string.Join("; ", unsuitableLevels[levelId]));
                }
            }

            // Заполнение таблицы подходящих уровней
            foreach (var level in airLevels)
            {
                int levelId = level.Keys.First();
                string levelName = level.Values.First();
                if (suitableLevels.ContainsKey(levelId))
                {
                    suitableGrid.Rows.Add(levelName, string.Join("; ", suitableLevels[levelId]));
                }
            }

            if (suitableGrid.Rows.Count == 0 && unsuitableGrid.Rows.Count == airLevels.Count)
            {
                allClassesRefutedLabel.Visible = true; // текст "Все классы были опровергнуты"
                suitableLabel.Location = new Point(10, 40);
                suitableGrid.Location = new Point(10, 70);
            }
            else
            {
                allClassesRefutedLabel.Visible = false;
                // Возвращаю таблицу "Подходящие уровни" на исходную позицию
                suitableLabel.Location = new Point(10, 10);
                suitableGrid.Location = new Point(10, 40);
            }

            foreach (DataGridViewRow row in suitableGrid.Rows)
            {
                row.Height = row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCells, true);
            }
            foreach (DataGridViewRow row in unsuitableGrid.Rows)
            {
                row.Height = row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCells, true);
            }
        }

        private void AirPollutionResultForm_Load(object sender, EventArgs e)
        {

        }
    }
}