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
    public partial class DataInputForm : Form
    {
        private MainForm mainForm;
        private bool wasWindowsClosed = false;
        private KnowledgeDB kdb;
        private Dictionary<int, string> inputValues; // для хранения введённых значений
        private ListBox featuresListBox; // Список свойств
        private ListBox valuesListBox; // Список для ввода значений
        private ListBox savedValuesListBox; // Список сохранённых значений
        private ComboBox valueComboBox; // Для перечислимых значений
        private TextBox valueTextBox; // Для числовых значений
        private KnowledgeDBlook kdbLook;
        private AirQualityPredictor airQualityPredictor; // Добавляю предсказатель (ии)
        public DataInputForm(MainForm parentForm)
        {
            InitializeComponent();
            this.FormClosed += DataInputForm_FormClosed;
            wasWindowsClosed = false;
            this.mainForm = parentForm; // Ссылка на MainForm
            kdb = new KnowledgeDB();
            inputValues = new Dictionary<int, string>(); // Словарь для хранения значений
            InitializeControls();
            //this.airQualityPredictor = new AirQualityPredictor("C:\\Users\\lesha\\source\\repos\\AirClassificator\\Datasets\\full_air_quality_cluster_imputed.csv");
            this.airQualityPredictor = new AirQualityPredictor("C:\\Users\\lesha\\source\\repos\\AirClassificator\\Datasets\\full_air_quality_clustered.csv");
        }

        private void InitializeControls()
        {
            this.Text = "Ввод исходных данных";
            this.Size = new Size(800, 600);

            Label featuresLabel = new Label
            {
                Text = "Свойства",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            this.Controls.Add(featuresLabel);

            featuresListBox = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(200, 400),
                ScrollAlwaysVisible = true,
                DisplayMember = "Value",
                ValueMember = "Key",
                HorizontalScrollbar = true
            };
            featuresListBox.SelectedIndexChanged += FeaturesListBox_SelectedIndexChanged;
            this.Controls.Add(featuresListBox);

            Label valuesLabel = new Label
            {
                Text = "Значение",
                AutoSize = true,
                Location = new Point(250, 10)
            };
            this.Controls.Add(valuesLabel);

            valueTextBox = new TextBox
            {
                Location = new Point(250, 40),
                Size = new Size(200, 20),
                Visible = false
            };
            this.Controls.Add(valueTextBox);

            Button saveButton = new Button
            {
                Text = "Сохранить",
                Location = new Point(250, 70),
                Size = new Size(90, 30)
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            Label savedValuesLabel = new Label
            {
                Text = "Сохранённые значения",
                AutoSize = true,
                Location = new Point(490, 10)
            };
            this.Controls.Add(savedValuesLabel);

            savedValuesListBox = new ListBox
            {
                Location = new Point(490, 40),
                Size = new Size(200, 400),
                ScrollAlwaysVisible = true,
                HorizontalScrollbar = true
            };
            this.Controls.Add(savedValuesListBox);

            Button backButton = new Button
            {
                Text = "Назад",
                Location = new Point(10, 500),
                Size = new Size(100, 30)
            };
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);

            Button viewKnowledgeBaseButton = new Button
            {
                Text = "Просмотр базы знаний",
                Location = new Point(120, 500),
                Size = new Size(150, 30)
            };
            viewKnowledgeBaseButton.Click += ViewKnowledgeBaseButton_Click;
            this.Controls.Add(viewKnowledgeBaseButton);

            Button determineLevelButton = new Button
            {
                Text = "Определить уровень загрязнённости воздуха",
                Location = new Point(280, 500),
                Size = new Size(250, 30)
            };
            determineLevelButton.Click += DetermineLevelButton_Click;
            this.Controls.Add(determineLevelButton);

            // Кнопка "Посмотреть ответ ИИ"
            Button aiPredictionButton = new Button
            {
                Text = "Посмотреть ответ ИИ",
                Location = new Point(540, 500),
                Size = new Size(150, 30)
            };
            aiPredictionButton.Click += AIPredictionButton_Click;
            this.Controls.Add(aiPredictionButton);

            LoadFeatures();
        }
        private void AIPredictionButton_Click(object sender, EventArgs e)
        {
            if (airQualityPredictor == null)
            {
                MessageBox.Show("Модель машинного обучения не инициализирована!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Чекаю, можно ли сделать предсказание
            if (!airQualityPredictor.CanPredict(inputValues, kdb.GetAllFeatures()))
            {
                MessageBox.Show("Не задано ни одного признака для ИИ!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Собираю введённые значения в словарь для предсказания
            var featuresForPrediction = new Dictionary<string, float?>();
            foreach (var feature in kdb.GetAllFeatures())
            {
                int featureId = feature.Keys.First();
                string featureName = feature.Values.First();
                if (inputValues.ContainsKey(featureId) && !string.IsNullOrEmpty(inputValues[featureId]))
                {
                    // Проверяю, является ли значение числовым
                    if (float.TryParse(inputValues[featureId], out float value))
                    {
                        featuresForPrediction[featureName] = value;
                    }
                    else
                    {
                        // Если значение не числовое, пропускаем его
                        featuresForPrediction[featureName] = null;
                    }
                }
                else
                {
                    featuresForPrediction[featureName] = null;
                }
            }

            // Делаю предсказание
            try
            {
                string predictedLevel = airQualityPredictor.Predict(featuresForPrediction);
                MessageBox.Show($"Предсказанный уровень загрязнённости воздуха: {predictedLevel}", "Ответ ИИ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при предсказании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFeatures()
        {
            List<Dictionary<int, string>> features = kdb.GetAllFeatures();
            featuresListBox.Items.Clear();
            savedValuesListBox.Items.Clear();
            foreach (var feature in features)
            {
                int featureId = feature.Keys.First();
                string featureName = feature.Values.First();
                featuresListBox.Items.Add(new KeyValuePair<int, string>(featureId, featureName));
                savedValuesListBox.Items.Add($"{featureName}: NULL");
                inputValues[featureId] = null; // Изначально все значения NULL
            }
        }

        private void FeaturesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (featuresListBox.SelectedItem == null) return;

            var selectedFeature = (KeyValuePair<int, string>)featuresListBox.SelectedItem;
            int featureId = selectedFeature.Key;

            // Показываем TextBox для ввода значения
            valueTextBox.Visible = true;
            valueTextBox.Text = inputValues.ContainsKey(featureId) && inputValues[featureId] != null ? inputValues[featureId] : "";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (featuresListBox.SelectedItem == null) return;

            var selectedFeature = (KeyValuePair<int, string>)featuresListBox.SelectedItem;
            int featureId = selectedFeature.Key;
            string featureName = selectedFeature.Value;

            // Проверка на пустое значение
            if (string.IsNullOrWhiteSpace(valueTextBox.Text))
            {
                MessageBox.Show("Значение не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string value = valueTextBox.Text;

            // Сохраняю значение в словарь
            inputValues[featureId] = value;

            // Обновляю список сохранённых значений
            savedValuesListBox.Items.Clear();
            foreach (var feature in featuresListBox.Items.Cast<KeyValuePair<int, string>>())
            {
                int id = feature.Key;
                string name = feature.Value;
                string savedValue = inputValues.ContainsKey(id) && inputValues[id] != null ? inputValues[id] : "NULL";
                savedValuesListBox.Items.Add($"{name}: {savedValue}");
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            wasWindowsClosed = true;
            this.Close();
            mainForm.Show();
        }

        private void ViewKnowledgeBaseButton_Click(object sender, EventArgs e)
        {
            if (kdbLook == null || kdbLook.IsDisposed)
            {
                kdbLook = new KnowledgeDBlook(this);
            }
            kdbLook.Show();
            this.Hide();
        }

        private void DetermineLevelButton_Click(object sender, EventArgs e)
        {
            AirPollutionResultForm resultForm = new AirPollutionResultForm(inputValues);
            resultForm.ShowDialog();
        }
        private void DataInputForm_Load(object sender, EventArgs e)
        {

        }

        private void DataInputForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (wasWindowsClosed == false)
            {
                mainForm.Close();
                Application.Exit(); // Немедленно кикаю процесс
            }
        }
    }
}
