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
    public partial class KnowledgeDBlook : Form
    {
        private DataInputForm datainputform;
        private bool wasWindowsClosed = false;
        private KnowledgeDB kdb; // Поле для доступа к методам KnowledgeBaseEditor
        private Panel panelDetails; // Панель для отображения данных
        private int? selectedLevelId; // Выбранный уровень
        private int? selectedFeatureId; // Выбранное свойство
        public KnowledgeDBlook(DataInputForm parentForm)
        {
            InitializeComponent();
            wasWindowsClosed = false;
            this.datainputform = parentForm;
            this.kdb = new KnowledgeDB();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Настройка формы
            this.Text = "Просмотр базы знаний";
            this.Size = new Size(800, 600);

            // Панель для отображения данных
            panelDetails = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(760, 450),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelDetails);

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Назад",
                Location = new Point(10, 470),
                Size = new Size(100, 30)
            };
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);

            DisplayLevelsList();
        }

        private void DisplayLevelsList()
        {
            panelDetails.Controls.Clear();

            Label headerLabel = new Label
            {
                Text = "Просмотр базы знаний",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            panelDetails.Controls.Add(headerLabel);

            List<Dictionary<int, string>> listOfLevels = kdb.GetAllAirLevels();

            int yPosition = 40;
            foreach (var level in listOfLevels)
            {
                int levelId = level.Keys.First();
                string levelName = level.Values.First();

                LinkLabel levelLink = new LinkLabel
                {
                    Text = levelName,
                    AutoSize = true,
                    Location = new Point(10, yPosition),
                    Tag = levelId
                };
                levelLink.Click += (s, ev) =>
                {
                    selectedLevelId = (int)levelLink.Tag;
                    selectedFeatureId = null;
                    DisplayLevelsList();
                    if (selectedLevelId.HasValue)
                    {
                        DisplayFeaturesForLevelValues(levelId);
                    }
                };
                if (selectedLevelId.HasValue && selectedLevelId == levelId)
                {
                    levelLink.LinkColor = Color.Red;
                }
                panelDetails.Controls.Add(levelLink);
                yPosition += 20;
            }

            if (selectedLevelId.HasValue)
            {
                DisplayFeaturesForLevelValues(selectedLevelId.Value);
            }
        }

        private void DisplayFeaturesForLevelValues(int levelId)
        {
            // Удаляю старые элементы свойств, чтобы избежать дублирования
            var existingLabels = panelDetails.Controls.OfType<Label>().Where(l => l.Text == "Свойства уровня:").ToList();
            foreach (var label in existingLabels)
            {
                panelDetails.Controls.Remove(label);
            }
            var existingFeatureLinks = panelDetails.Controls.OfType<LinkLabel>().Where(l => l.Location.X >= 200).ToList();
            foreach (var link in existingFeatureLinks)
            {
                panelDetails.Controls.Remove(link);
            }

            // Начальная позиция для свойств (справа от уровней)
            int xPosition = 200;
            int yPosition = 40;

            List<int> assignedFeatureIds = kdb.GetFeaturesForLevel(levelId);
            List<Dictionary<int, string>> listOfFeatures = kdb.GetAllFeatures().Where(f => assignedFeatureIds.Contains(f.Keys.First())).ToList();

            Label featuresLabel = new Label
            {
                Text = "Свойства уровня:",
                AutoSize = true,
                Location = new Point(xPosition, yPosition)
            };
            panelDetails.Controls.Add(featuresLabel);
            yPosition += 30;

            foreach (var feature in listOfFeatures)
            {
                int featureId = feature.Keys.First();
                string featureName = feature.Values.First();

                LinkLabel featureLink = new LinkLabel
                {
                    Text = featureName,
                    AutoSize = true,
                    Location = new Point(xPosition, yPosition),
                    Tag = featureId
                };
                featureLink.Click += (s, ev) =>
                {
                    selectedFeatureId = (int)featureLink.Tag;
                    DisplayLevelsList();
                    DisplayFeaturesForLevelValues(levelId);
                    DisplayFeatureValuesForLevel(levelId, (int)selectedFeatureId);
                };
                if (selectedFeatureId.HasValue && selectedFeatureId == featureId)
                {
                    featureLink.LinkColor = Color.Red;
                }
                panelDetails.Controls.Add(featureLink);
                yPosition += 30;
            }

            if (selectedFeatureId.HasValue)
            {
                DisplayFeatureValuesForLevel(levelId, selectedFeatureId.Value);
            }
        }

        private void DisplayFeatureValuesForLevel(int levelId, int featureId)
        {
            // Удаляю старые элементы значений
            var existingLabels = panelDetails.Controls.OfType<Label>().Where(l => l.Text == "Значения свойства:" || l.Location.X >= 230).ToList();
            foreach (var label in existingLabels)
            {
                panelDetails.Controls.Remove(label);
            }

            // Начальная позиция для значений
            int xPosition = 200;
            int featureLinkHeight = panelDetails.Controls.OfType<LinkLabel>().Count(l => l.Location.X >= 200) * 30;
            int yPosition = 40 + 30 + featureLinkHeight + 20;

            List<Dictionary<string, string>> featureValues = kdb.GetFeatureValues(featureId);
            List<int> markedFeatureValueIds = kdb.GetMarkedFeatureValuesForLevel(levelId);

            Label valuesLabel = new Label
            {
                Text = "Значения свойства:",
                AutoSize = true,
                Location = new Point(xPosition, yPosition)
            };
            panelDetails.Controls.Add(valuesLabel);
            yPosition += 20;

            // Фильтрую только доступные значения (те, которые отмечены)
            var availableValues = featureValues.Where(value =>
            {
                int valueId = int.Parse(value["id"]);
                return markedFeatureValueIds.Contains(valueId);
            }).ToList();

            // Если нет доступных значений, вывожу сообщение
            if (availableValues.Count == 0)
            {
                Label noValuesLabel = new Label
                {
                    Text = "Нет значений для свойства!",
                    AutoSize = true,
                    Location = new Point(xPosition, yPosition),
                    ForeColor = Color.Red
                };
                panelDetails.Controls.Add(noValuesLabel);
                return;
            }

            // Отображаю только доступные значения
            foreach (var value in availableValues)
            {
                int valueId = int.Parse(value["id"]);
                string displayText = value["typevalue"] == "num"
                    ? $"Числовой: от {value["minvalue"]} до {value["maxvalue"]}"
                    : $"Перечислимый: {value["constantvalue"]}";

                // Отображаю значение
                Label valueLabel = new Label
                {
                    Text = displayText,
                    AutoSize = true,
                    Location = new Point(xPosition, yPosition)
                };
                panelDetails.Controls.Add(valueLabel);

                // Отображаю зелёную галочку
                Label markedLabel = new Label
                {
                    Text = "✓",
                    AutoSize = true,
                    Location = new Point(xPosition + 200, yPosition),
                    ForeColor = Color.Green
                };
                panelDetails.Controls.Add(markedLabel);

                yPosition += 30;
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            wasWindowsClosed = true;
            this.Close();
            datainputform.Show();
        }

        private void KnowledgeDBlook_Load(object sender, EventArgs e)
        {

        }

        private void KnowledgeDBlook_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (wasWindowsClosed == false)
            {
                datainputform.Close();
                Application.Exit(); // Немедленно завершаю процесс
            }
        }
    }
}
