using AirClassificator.DatabaseMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

            // Таблица "Подходящие уровни загрязнённости воздуха"
            Label suitableLabel = new Label
            {
                Text = "Подходящие уровни загрязнённости воздуха",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            this.Controls.Add(suitableLabel);

            DataGridView suitableGrid = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(760, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false // Убираем заголовки строк для компактности
            };
            suitableGrid.Columns.Add("LevelName", "Уровень воздуха");
            suitableGrid.Columns.Add("Reasons", "Причины");
            suitableGrid.Columns["LevelName"].Width = 200; // Фиксированная ширина для первой колонки
            suitableGrid.Columns["Reasons"].DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Включаем перенос текста
            suitableGrid.Tag = "SuitableGrid";
            this.Controls.Add(suitableGrid);

            // Таблица "Не подходящие уровни загрязнённости воздуха"
            Label unsuitableLabel = new Label
            {
                Text = "Не подходящие уровни загрязнённости воздуха",
                AutoSize = true,
                Location = new Point(10, 260)
            };
            this.Controls.Add(unsuitableLabel);

            DataGridView unsuitableGrid = new DataGridView
            {
                Location = new Point(10, 290),
                Size = new Size(760, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false // Убираем заголовки строк для компактности
            };
            unsuitableGrid.Columns.Add("LevelName", "Уровень воздуха");
            unsuitableGrid.Columns.Add("Reasons", "Причины");
            unsuitableGrid.Columns["LevelName"].Width = 200; // Фиксированная ширина для первой колонки
            unsuitableGrid.Columns["Reasons"].DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Включаем перенос текста
            unsuitableGrid.Tag = "UnsuitableGrid";
            this.Controls.Add(unsuitableGrid);

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Назад",
                Location = new Point(10, 500),
                Size = new Size(100, 30)
            };
            backButton.Click += (s, e) => this.Close();
            this.Controls.Add(backButton);
        }

        private void DetermineAirPollutionLevels()
        {
            DataGridView suitableGrid = this.Controls.OfType<DataGridView>().First(g => g.Tag.ToString() == "SuitableGrid");
            DataGridView unsuitableGrid = this.Controls.OfType<DataGridView>().First(g => g.Tag.ToString() == "UnsuitableGrid");

            List<Dictionary<int, string>> airLevels = kdb.GetAllAirLevels();
            List<Dictionary<int, string>> features = kdb.GetAllFeatures();

            Dictionary<int, List<string>> suitableLevels = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> unsuitableLevels = new Dictionary<int, List<string>>();

            // Проверяем, ввёл ли пользователь хотя бы одно значение
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

                        bool isUserValueNumeric = double.TryParse(userValue, out double numericUserValue);

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
                                double minValue = double.Parse(value["minvalue"]);
                                double maxValue = double.Parse(value["maxvalue"]);

                                if (isUserValueNumeric && numericUserValue >= minValue && numericUserValue <= maxValue)
                                {
                                    valueMatches = true;
                                    suitableReason = $"Значение свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' ({userValue}) находится в диапазоне от {minValue} до {maxValue}";
                                    break;
                                }
                                else if (!isUserValueNumeric)
                                {
                                    unsuitableReason = $"Наличие свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' со значением '{userValue}' (ожидается числовое значение)";
                                }
                                else
                                {
                                    unsuitableReason = $"Наличие свойства '{features.First(f => f.Keys.First() == featureId).Values.First()}' со значением {userValue} (ожидается значение в диапазоне от {minValue} до {maxValue})";
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

            // Заполняем таблицу неподходящих уровней
            foreach (var level in airLevels)
            {
                int levelId = level.Keys.First();
                string levelName = level.Values.First();
                if (unsuitableLevels.ContainsKey(levelId))
                {
                    unsuitableGrid.Rows.Add(levelName, string.Join("; ", unsuitableLevels[levelId]));
                }
            }

            // Заполняем таблицу подходящих уровней
            foreach (var level in airLevels)
            {
                int levelId = level.Keys.First();
                string levelName = level.Values.First();
                if (suitableLevels.ContainsKey(levelId))
                {
                    suitableGrid.Rows.Add(levelName, string.Join("; ", suitableLevels[levelId]));
                }
            }

            // Настраиваем высоту строк для переноса текста
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
