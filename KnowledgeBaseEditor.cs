using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AirClassificator.DatabaseMethods;

namespace AirClassificator
{
    public partial class KnowledgeBaseEditor : Form
    {
        private MainForm mainForm;
        private bool wasWindowsClosed = false;
        KnowledgeDB kdb;
        private int? selectedFeatureId = null; // Храним ID выбранного свойства
        private int? selectedLevelId = null; // Храним ID выбранного уровня
        private int? selectedFeatureIdForLevel = null; // Храним ID выбранного свойства для уровня

        public KnowledgeBaseEditor(MainForm parentForm)
        {
            InitializeComponent();
            this.FormClosed += KnowledgeBaseEditor_FormClosed;
            wasWindowsClosed = false;
            this.mainForm = parentForm; // Сохраняем ссылку на MainForm

            // Заполняю ListBox элементами
            options_list.Items.Add("Уровни загрязнённости воздуха");
            options_list.Items.Add("Свойства");
            options_list.Items.Add("Возможные значения");
            options_list.Items.Add("Описание свойств уровней");
            options_list.Items.Add("Значения для уровня");
            options_list.Items.Add("Проверка полноты знаний");
            kdb = new KnowledgeDB();
            // Подписываюсь на событие выбора элемента в ListBox
            options_list.SelectedIndexChanged += options_list_SelectedIndexChanged;
        }

        private void options_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Очищаю правую панель перед обновлением 
            panelDetails.Controls.Clear();

            // В зависимости от выбранного элемента показываем разные данные
            if (options_list.SelectedItem != null)
            {
                string selectedItem = options_list.SelectedItem.ToString();
                Label label = new Label
                {
                    Text = $"Вы выбрали: {selectedItem}",
                    AutoSize = true,
                    Location = new Point(10, 10) // Позиция внутри panelDetails
                };
                panelDetails.Controls.Add(label);

                if (selectedItem == "Уровни загрязнённости воздуха")
                {
                    List<Dictionary<int, string>> listOfLevels = kdb.GetAllAirLevels();

                    int yPosition = 40;
                    foreach (var level in listOfLevels)
                    {
                        int id = level.Keys.First();
                        string name = level.Values.First();

                        // Сначала добавляем кнопку "Удалить"
                        Button deleteButton = new Button
                        {
                            Text = "Удалить",
                            Location = new Point(10, yPosition), // Кнопка слева
                            Width = 80 // Фиксируем ширину кнопки
                        };
                        deleteButton.Click += (s, ev) =>
                        {
                            int levelId = (int)deleteButton.Tag;
                            kdb.DeleteAirLevelById(levelId);
                            options_list_SelectedIndexChanged(sender, e);
                        };
                        deleteButton.Tag = id;
                        panelDetails.Controls.Add(deleteButton);

                        // Затем добавляем Label с названием уровня
                        Label levelLabel = new Label
                        {
                            Text = name,
                            AutoSize = true,
                            Location = new Point(100, yPosition) // Название справа от кнопки
                        };
                        panelDetails.Controls.Add(levelLabel);

                        yPosition += 30;
                    }

                    TextBox newLevelTextBox = new TextBox
                    {
                        Location = new Point(10, yPosition),
                        Width = 200,
                        Text = "Введите название уровня...",
                        ForeColor = Color.Gray
                    };

                    newLevelTextBox.Enter += (s, ev) =>
                    {
                        if (newLevelTextBox.Text == "Введите название уровня...")
                        {
                            newLevelTextBox.Text = "";
                            newLevelTextBox.ForeColor = Color.Black;
                        }
                    };
                    newLevelTextBox.Leave += (s, ev) =>
                    {
                        if (string.IsNullOrWhiteSpace(newLevelTextBox.Text))
                        {
                            newLevelTextBox.Text = "Введите название уровня...";
                            newLevelTextBox.ForeColor = Color.Gray;
                        }
                    };

                    panelDetails.Controls.Add(newLevelTextBox);

                    Button addButton = new Button
                    {
                        Text = "Добавить",
                        Location = new Point(220, yPosition)
                    };
                    addButton.Click += (s, ev) =>
                    {
                        if (!string.IsNullOrWhiteSpace(newLevelTextBox.Text) && newLevelTextBox.Text != "Введите название уровня...")
                        {
                            kdb.AddAirLevel(newLevelTextBox.Text);
                            newLevelTextBox.Text = "";
                            options_list_SelectedIndexChanged(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("Введите название уровня!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };
                    panelDetails.Controls.Add(addButton);
                }
                else if (selectedItem == "Свойства")
                {
                    List<Dictionary<int, string>> listOfFeatures = kdb.GetAllFeatures();

                    int yPosition = 40;
                    foreach (var feature in listOfFeatures)
                    {
                        int id = feature.Keys.First();
                        string name = feature.Values.First();

                        Button deleteButton = new Button
                        {
                            Text = "Удалить",
                            Location = new Point(10, yPosition),
                            Width = 80
                        };
                        deleteButton.Click += (s, ev) =>
                        {
                            int featureId = (int)deleteButton.Tag;
                            kdb.DeleteFeatureById(featureId);
                            options_list_SelectedIndexChanged(sender, e);
                        };
                        deleteButton.Tag = id;
                        panelDetails.Controls.Add(deleteButton);

                        Label featureLabel = new Label
                        {
                            Text = name,
                            AutoSize = true,
                            Location = new Point(100, yPosition)
                        };
                        panelDetails.Controls.Add(featureLabel);

                        yPosition += 30;
                    }

                    TextBox newFeatureTextBox = new TextBox
                    {
                        Location = new Point(10, yPosition),
                        Width = 200,
                        Text = "Введите название свойства...",
                        ForeColor = Color.Gray
                    };

                    newFeatureTextBox.Enter += (s, ev) =>
                    {
                        if (newFeatureTextBox.Text == "Введите название свойства...")
                        {
                            newFeatureTextBox.Text = "";
                            newFeatureTextBox.ForeColor = Color.Black;
                        }
                    };
                    newFeatureTextBox.Leave += (s, ev) =>
                    {
                        if (string.IsNullOrWhiteSpace(newFeatureTextBox.Text))
                        {
                            newFeatureTextBox.Text = "Введите название свойства...";
                            newFeatureTextBox.ForeColor = Color.Gray;
                        }
                    };

                    panelDetails.Controls.Add(newFeatureTextBox);

                    Button addButton = new Button
                    {
                        Text = "Добавить",
                        Location = new Point(220, yPosition)
                    };
                    addButton.Click += (s, ev) =>
                    {
                        if (!string.IsNullOrWhiteSpace(newFeatureTextBox.Text) && newFeatureTextBox.Text != "Введите название свойства...")
                        {
                            kdb.AddFeature(newFeatureTextBox.Text);
                            newFeatureTextBox.Text = "";
                            options_list_SelectedIndexChanged(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("Введите название свойства!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };
                    panelDetails.Controls.Add(addButton);
                }
                else if (selectedItem == "Возможные значения")
                {
                    selectedFeatureId = null; // Сбрасываем выбор
                    DisplayFeaturesList(sender, e);
                }
                else if (selectedItem == "Описание свойств уровней")
                {
                    selectedLevelId = null;
                    DisplayLevelsList(sender, e);
                }
                else if (selectedItem == "Значения для уровня")
                {
                    selectedLevelId = null;
                    selectedFeatureIdForLevel = null;
                    DisplayLevelsListForValues(sender, e);
                }
                else if (selectedItem == "Проверка полноты знаний")
                {
                    // Получаем свойства без значений и уровни без свойств
                    List<Dictionary<int, string>> featuresWithoutValues = kdb.GetFeaturesWithoutValues();
                    List<Dictionary<int, string>> levelsWithoutFeatures = kdb.GetLevelsWithoutFeatures();

                    int yPosition = 40;

                    // Если всё заполнено
                    if (featuresWithoutValues.Count == 0 && levelsWithoutFeatures.Count == 0)
                    {
                        Label allFilledLabel = new Label
                        {
                            Text = "Все поля заполнены",
                            AutoSize = true,
                            Location = new Point(10, yPosition),
                            ForeColor = Color.Green
                        };
                        panelDetails.Controls.Add(allFilledLabel);
                        return;
                    }

                    // Отображаем свойства без значений
                    if (featuresWithoutValues.Count > 0)
                    {
                        Label featuresLabel = new Label
                        {
                            Text = "Свойства без значений:",
                            AutoSize = true,
                            Location = new Point(10, yPosition),
                            ForeColor = Color.Red
                        };
                        panelDetails.Controls.Add(featuresLabel);
                        yPosition += 30;

                        foreach (var feature in featuresWithoutValues)
                        {
                            int featureId = feature.Keys.First();
                            string featureName = feature.Values.First();

                            Label featureLabel = new Label
                            {
                                Text = featureName,
                                AutoSize = true,
                                Location = new Point(20, yPosition)
                            };
                            panelDetails.Controls.Add(featureLabel);
                            yPosition += 20;
                        }
                    }

                    // Отображаем уровни без свойств
                    if (levelsWithoutFeatures.Count > 0)
                    {
                        yPosition += 10; // Небольшой отступ перед следующим разделом
                        Label levelsLabel = new Label
                        {
                            Text = "Уровни без свойств:",
                            AutoSize = true,
                            Location = new Point(10, yPosition),
                            ForeColor = Color.Red
                        };
                        panelDetails.Controls.Add(levelsLabel);
                        yPosition += 30;

                        foreach (var level in levelsWithoutFeatures)
                        {
                            int levelId = level.Keys.First();
                            string levelName = level.Values.First();

                            Label levelLabel = new Label
                            {
                                Text = levelName,
                                AutoSize = true,
                                Location = new Point(20, yPosition)
                            };
                            panelDetails.Controls.Add(levelLabel);
                            yPosition += 20;
                        }
                    }
                }
            }
        }

        private void DisplayFeaturesList(object sender, EventArgs e)
        {
            // Очищаем panelDetails, но сохраняем заголовок
            var headerLabel = panelDetails.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Вы выбрали:"));
            panelDetails.Controls.Clear();
            if (headerLabel != null)
            {
                panelDetails.Controls.Add(headerLabel);
            }

            List<Dictionary<int, string>> listOfFeatures = kdb.GetAllFeatures();

            int yPosition = 40;
            foreach (var feature in listOfFeatures)
            {
                int featureId = feature.Keys.First();
                string featureName = feature.Values.First();

                LinkLabel featureLink = new LinkLabel
                {
                    Text = featureName,
                    AutoSize = true,
                    Location = new Point(10, yPosition),
                    Tag = featureId
                };
                featureLink.Click += (s, ev) =>
                {
                    selectedFeatureId = (int)featureLink.Tag;
                    DisplayFeaturesList(sender, e); // Обновляем список свойств
                    DisplayFeatureValuesAndSettings(sender, e, (int)selectedFeatureId); // Отображаем значения и настройки
                };
                if (selectedFeatureId.HasValue && selectedFeatureId == featureId)
                {
                    featureLink.LinkColor = Color.Red; // Выделяем выбранное свойство
                }
                panelDetails.Controls.Add(featureLink);
                yPosition += 20;
            }

            // Если есть выбранное свойство, отображаем его значения и настройки
            if (selectedFeatureId.HasValue)
            {
                DisplayFeatureValuesAndSettings(sender, e, selectedFeatureId.Value);
            }
        }

        private void DisplayFeatureValuesAndSettings(object sender, EventArgs e, int featureId)
        {
            int innerYPosition = panelDetails.Controls.OfType<LinkLabel>().Count() * 20 + 40;

            List<Dictionary<string, string>> values = kdb.GetFeatureValues(featureId);
            Label valuesLabel = new Label
            {
                Text = "Возможные значения:",
                AutoSize = true,
                Location = new Point(10, innerYPosition)
            };
            panelDetails.Controls.Add(valuesLabel);
            innerYPosition += 20;

            foreach (var value in values)
            {
                string displayText = value["typevalue"] == "num"
                    ? $"Числовой: от {value["minvalue"]} до {value["maxvalue"]}"
                    : $"Перечислимый: {value["constantvalue"]}";

                Label valueLabel = new Label
                {
                    Text = displayText,
                    AutoSize = true,
                    Location = new Point(10, innerYPosition)
                };
                panelDetails.Controls.Add(valueLabel);

                Button deleteValueButton = new Button
                {
                    Text = "Удалить",
                    Location = new Point(200, innerYPosition),
                    Tag = value
                };
                deleteValueButton.Click += (s2, ev2) =>
                {
                    var val = (Dictionary<string, string>)deleteValueButton.Tag;
                    kdb.DeleteFeatureValue(int.Parse(val["feature_id"]), int.Parse(val["id"]));
                    DisplayFeaturesList(sender, e);
                };
                panelDetails.Controls.Add(deleteValueButton);

                innerYPosition += 30;
            }

            RadioButton numRadio = new RadioButton
            {
                Text = "Числовой тип",
                Location = new Point(10, innerYPosition),
                AutoSize = true
            };
            panelDetails.Controls.Add(numRadio);

            RadioButton enumRadio = new RadioButton
            {
                Text = "Перечислимый тип",
                Location = new Point(150, innerYPosition),
                AutoSize = true
            };
            panelDetails.Controls.Add(enumRadio);
            innerYPosition += 30;

            Label fromLabel = new Label
            {
                Text = "от",
                AutoSize = true,
                Location = new Point(10, innerYPosition),
                Visible = false
            };
            panelDetails.Controls.Add(fromLabel);

            TextBox minValueTextBox = new TextBox
            {
                Location = new Point(40, innerYPosition),
                Width = 100,
                Visible = false
            };
            panelDetails.Controls.Add(minValueTextBox);

            Label toLabel = new Label
            {
                Text = "до",
                AutoSize = true,
                Location = new Point(150, innerYPosition),
                Visible = false
            };
            panelDetails.Controls.Add(toLabel);

            TextBox maxValueTextBox = new TextBox
            {
                Location = new Point(180, innerYPosition),
                Width = 100,
                Visible = false
            };
            panelDetails.Controls.Add(maxValueTextBox);

            TextBox enumValueTextBox = new TextBox
            {
                Location = new Point(10, innerYPosition),
                Width = 200,
                Visible = false
            };
            panelDetails.Controls.Add(enumValueTextBox);

            Button addValueButton = new Button
            {
                Text = "Добавить",
                Location = new Point(220, innerYPosition + 30),
                Visible = false
            };
            addValueButton.Click += (s2, ev2) =>
            {
                if (numRadio.Checked)
                {
                    if (string.IsNullOrWhiteSpace(minValueTextBox.Text) || string.IsNullOrWhiteSpace(maxValueTextBox.Text))
                    {
                        MessageBox.Show("Введите минимальное и максимальное значения!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    kdb.AddFeatureValue(featureId, minValueTextBox.Text, maxValueTextBox.Text, "num", null);
                    minValueTextBox.Text = "";
                    maxValueTextBox.Text = "";
                }
                else if (enumRadio.Checked)
                {
                    if (string.IsNullOrWhiteSpace(enumValueTextBox.Text))
                    {
                        MessageBox.Show("Введите значение!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    kdb.AddFeatureValue(featureId, null, null, "enum", enumValueTextBox.Text);
                    enumValueTextBox.Text = "";
                }
                DisplayFeaturesList(sender, e);
            };
            panelDetails.Controls.Add(addValueButton);

            numRadio.CheckedChanged += (s2, ev2) =>
            {
                if (numRadio.Checked)
                {
                    fromLabel.Visible = true;
                    minValueTextBox.Visible = true;
                    toLabel.Visible = true;
                    maxValueTextBox.Visible = true;
                    enumValueTextBox.Visible = false;
                    addValueButton.Visible = true;
                }
            };
            enumRadio.CheckedChanged += (s2, ev2) =>
            {
                if (enumRadio.Checked)
                {
                    fromLabel.Visible = false;
                    minValueTextBox.Visible = false;
                    toLabel.Visible = false;
                    maxValueTextBox.Visible = false;
                    enumValueTextBox.Visible = true;
                    addValueButton.Visible = true;
                }
            };
        }

        private void DisplayLevelsList(object sender, EventArgs e)
        {
            var headerLabel = panelDetails.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Вы выбрали:"));
            panelDetails.Controls.Clear();
            if (headerLabel != null)
            {
                panelDetails.Controls.Add(headerLabel);
            }

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
                    DisplayLevelsList(sender, e);
                    DisplayFeaturesForLevel(sender, e, (int)selectedLevelId);
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
                DisplayFeaturesForLevel(sender, e, selectedLevelId.Value);
            }
        }

        private void DisplayFeaturesForLevel(object sender, EventArgs e, int levelId)
        {
            int innerYPosition = panelDetails.Controls.OfType<LinkLabel>().Count() * 20 + 40;

            List<Dictionary<int, string>> listOfFeatures = kdb.GetAllFeatures();
            List<int> assignedFeatureIds = kdb.GetFeaturesForLevel(levelId);

            Label featuresLabel = new Label
            {
                Text = "Свойства уровня:",
                AutoSize = true,
                Location = new Point(10, innerYPosition)
            };
            panelDetails.Controls.Add(featuresLabel);
            innerYPosition += 20;

            foreach (var feature in listOfFeatures)
            {
                int featureId = feature.Keys.First();
                string featureName = feature.Values.First();

                CheckBox featureCheckBox = new CheckBox
                {
                    Location = new Point(10, innerYPosition),
                    AutoSize = true,
                    Checked = assignedFeatureIds.Contains(featureId)
                };
                featureCheckBox.Tag = new Tuple<int, int>(levelId, featureId);
                featureCheckBox.CheckedChanged += (s, ev) =>
                {
                    var tag = (Tuple<int, int>)featureCheckBox.Tag;
                    int lvlId = tag.Item1;
                    int ftrId = tag.Item2;

                    if (featureCheckBox.Checked)
                    {
                        kdb.AddFeatureToLevel(lvlId, ftrId);
                    }
                    else
                    {
                        kdb.RemoveFeatureFromLevel(lvlId, ftrId);
                    }
                };
                panelDetails.Controls.Add(featureCheckBox);

                Label featureLabel = new Label
                {
                    Text = featureName,
                    AutoSize = true,
                    Location = new Point(40, innerYPosition)
                };
                panelDetails.Controls.Add(featureLabel);

                innerYPosition += 30;
            }
        }

        private void DisplayLevelsListForValues(object sender, EventArgs e)
        {
            var headerLabel = panelDetails.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Вы выбрали:"));
            panelDetails.Controls.Clear();
            if (headerLabel != null)
            {
                panelDetails.Controls.Add(headerLabel);
            }

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
                    selectedFeatureIdForLevel = null;
                    DisplayLevelsListForValues(sender, e);
                    if (selectedLevelId.HasValue)
                    {
                        DisplayFeaturesForLevelValues(sender, e, selectedLevelId.Value);
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
                DisplayFeaturesForLevelValues(sender, e, selectedLevelId.Value);
            }
        }

        private void DisplayFeatureValuesForLevel(object sender, EventArgs e, int levelId, int featureId)
        {
            // Удаляем старые элементы значений, чтобы избежать дублирования
            var existingLabels = panelDetails.Controls.OfType<Label>().Where(l => l.Text == "Значения свойства:").ToList();
            foreach (var label in existingLabels)
            {
                panelDetails.Controls.Remove(label);
            }
            var existingCheckBoxes = panelDetails.Controls.OfType<CheckBox>().ToList();
            foreach (var checkBox in existingCheckBoxes)
            {
                panelDetails.Controls.Remove(checkBox);
            }
            var existingValueLabels = panelDetails.Controls.OfType<Label>().Where(l => l.Location.X >= 230 && l.Text != "Свойства уровня:").ToList();
            foreach (var label in existingValueLabels)
            {
                panelDetails.Controls.Remove(label);
            }

            // Начальная позиция для значений (справа от уровней, ниже свойств)
            int xPosition = 200; // Сдвигаем вправо
            int featureLinkHeight = panelDetails.Controls.OfType<LinkLabel>().Count(l => l.Location.X >= 200) * 30; // Высота всех свойств
            int yPosition = 40 + 30 + featureLinkHeight + 20; // 40 (начало) + 30 (заголовок "Свойства уровня:") + высота свойств + отступ

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

            foreach (var value in featureValues)
            {
                int valueId = int.Parse(value["id"]);
                string displayText = value["typevalue"] == "num"
                    ? $"Числовой: от {value["minvalue"]} до {value["maxvalue"]}"
                    : $"Перечислимый: {value["constantvalue"]}";

                CheckBox valueCheckBox = new CheckBox
                {
                    Location = new Point(xPosition, yPosition),
                    AutoSize = true,
                    Checked = markedFeatureValueIds.Contains(valueId)
                };
                valueCheckBox.Tag = new Tuple<int, int>(levelId, valueId);
                valueCheckBox.CheckedChanged += (s, ev) =>
                {
                    var tag = (Tuple<int, int>)valueCheckBox.Tag;
                    int lvlId = tag.Item1;
                    int markedValueId = tag.Item2;

                    if (valueCheckBox.Checked)
                    {
                        kdb.AddLevelToFeatureValueMarked(lvlId, markedValueId);
                    }
                    else
                    {
                        kdb.RemoveLevelToFeatureValueMarked(lvlId, markedValueId);
                    }
                };
                panelDetails.Controls.Add(valueCheckBox);

                Label valueLabel = new Label
                {
                    Text = displayText,
                    AutoSize = true,
                    Location = new Point(xPosition + 30, yPosition) // Сдвигаем текст значений чуть правее CheckBox
                };
                panelDetails.Controls.Add(valueLabel);

                yPosition += 30;
            }
        }

        private void DisplayFeaturesForLevelValues(object sender, EventArgs e, int levelId)
        {
            // Удаляем старые элементы свойств, чтобы избежать дублирования
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
            int xPosition = 200; // Сдвигаем вправо
            int yPosition = 40;  // Начинаем с той же высоты, что и уровни

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
                    selectedFeatureIdForLevel = (int)featureLink.Tag;
                    DisplayLevelsListForValues(sender, e);
                    DisplayFeaturesForLevelValues(sender, e, levelId);
                    DisplayFeatureValuesForLevel(sender, e, levelId, (int)selectedFeatureIdForLevel);
                };
                if (selectedFeatureIdForLevel.HasValue && selectedFeatureIdForLevel == featureId)
                {
                    featureLink.LinkColor = Color.Red;
                }
                panelDetails.Controls.Add(featureLink);
                yPosition += 30;
            }

            if (selectedFeatureIdForLevel.HasValue)
            {
                DisplayFeatureValuesForLevel(sender, e, levelId, selectedFeatureIdForLevel.Value);
            }
        }

        private void button_list_settings_panel_Paint(object sender, PaintEventArgs e) {}

        private void back_button_Click(object sender, EventArgs e)
        {
            mainForm.Show(); // Показываем MainForm
            wasWindowsClosed = true;
            this.Close(); // Закрываем KnowledgeBaseEditor
        }
        
        private void KnowledgeBaseEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (wasWindowsClosed == false)
            {
                mainForm.Close();
                Application.Exit(); // Немедленно завершаем процесс
            }
        }
    }
}
