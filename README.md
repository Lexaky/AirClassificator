# AirClassificator
Структура базы данных (базы знаний):
![изображение](https://github.com/user-attachments/assets/f89eed29-69a0-4959-9c19-2951dc50c0b4)

Программное средство:
![изображение](https://github.com/user-attachments/assets/13573616-61f7-4e3b-b006-917854b41276)

«MainForm» – главная форма, которая показывается пользователю при запуске программы и позволяет осуществить переход к формам «DataInputForm», «KnowledgeBaseEditor».
   «Edit_knowledge_base_button_Click» – метод для перехода к форме «KnowledgeBaseEditor».
  «Input_values_button_Click» – метод для перехода к форме «DataInputForm».
  «MainForm_Load» – метод инициализации системных переменных при загрузке формы «MainForm».

«DataInputForm» – форма для ввода значений признаков для дальнейшей классификации с помощью ИИ («AirQualityPredictor») или с помощью базы знаний («AirPollutionResultForm»).
  «DataInputForm» – конструктор формы «DataInputForm» для инициализации переменных.
  «InitializeControls» – метод для инициализации элементов формы DataInputForm.
  «AIPredictionButton_Click» – метод для инициализации создания класса «AirQualityPredictor».
  «LoadFeatures» – метод для получения всех признаков из базы знаний.
  «FeaturesListBox_SelectedIndexChanged» – метод для обновления списка признаков при нажатии на один из признаков.
  «SaveButton_Click» – метод для сохранения введённого значения признака из окна «TextBox» в список «savedValuesListBox».
  «BackButton_Click» – метод для возвращения к «MainForm».
  «ViewKnowledgeBaseButton_Click» – метод для перехода к форме «KnowledgeDBlook».
  «DetermineLevelButton_Click» – метод для перехода к форме «AirPollutionResultForm».
  «DataInputForm_Load» – метод инициализации системных переменных при загрузке формы «DataInputForm».
  «DataInputForm_Closed» – метод закрытия формы «DataInputForm».

«KnowledgeBaseEditor» – форма для редактирования базы знаний экспертной системы.
  «KnowledgeBaseEditor» – конструктор формы «KnowledgeBaseEditor» для инициализации переменных и настройки формы.
  «options_list_SelectedIndexChanged» – метод для обработки изменения выбранного элемента в списке «options_list».
  «DisplayFeaturesList» – метод для отображения списка признаков.
  «DisplayFeatureValuesAndSettings» – метод для отображения значений и настроек выбранного признака.
  «DisplayLevelsList» – метод для отображения списка уровней загрязнённости воздуха.
  «DisplayFeaturesForLevel» – метод для отображения списка признаков, связанных с выбранным уровнем загрязнённости.
  «DisplayLevelsListForValues» – метод для отображения списка уровней загрязнённости, связанных с редактированием значений признаков.
  «DisplayFeatureValuesForLevel» – метод для отображения значений признака, связанных с выбранным уровнем загрязнённости.
  «DisplayFeaturesForLevelValues» – метод для отображения списка признаков, связанных с редактированием значений для выбранного уровня загрязнённости
  «back_button_Click» – метод для обработки нажатия кнопки «Назад».
  «KnowledgeBaseEditor_FormClosed» – метод для обработки закрытия формы «KnowledgeBaseEditor».

«AirQualityPredictor» – класс для инициализации обучения и предоставления модели машинного обучения для дальнейшей классификации уровня загрязнённости воздуха.
  «AirQualityPredictor» – конструктор класса «AirQualityPredictor» для инициализации переменных и обучения модели. 
  «TrainModel» – метод для обучения модели машинного обучения на основе выборки. 
  «Predict» – метод для выполнения предсказания уровня загрязнённости воздуха на основе входных данных. 
  «CanPredict» – метод для проверки возможности выполнения предсказания на основе введённых данных.


«KnowledgeDB» – класс для отправки запросов к БД «Knowledge» (Базе знаний).
  «KnowledgeDB» – конструктор класса «KnowledgeDB» для инициализации подключения к базе данных. 
  «TestDatabaseConnection» – метод для проверки подключения к базе данных. 
  «GetAllAirLevels» – метод для получения списка всех уровней загрязнённости воздуха из базы данных. 
  «GetAirLevelById» – метод для получения названия уровня загрязнённости воздуха по его идентификатору. 
  «DeleteAirLevelById» – метод для удаления уровня загрязнённости воздуха из базы данных по его идентификатору. 
  «AddAirLevel» – метод для добавления нового уровня загрязнённости воздуха в базу данных. 
  «GetAllFeatures» – метод для получения списка всех признаков из базы данных. 
  «DeleteFeatureById» – метод для удаления признака из базы данных по его идентификатору. 
  «AddFeature» – метод для добавления нового признака в базу данных. 
  «GetFeatureValues» – метод для получения списка значений признака из базы данных. 
  «AddFeatureValue» – метод для добавления нового значения для признака в базу данных. 
  «DeleteFeatureValue» – метод для удаления значения признака из базы данных. 
  «AddFeatureToLevel» – метод для связывания признака с уровнем загрязнённости в базе данных. 
  «RemoveFeatureFromLevel» – метод для удаления связи между признаком и уровнем загрязнённости в базе данных. 
  «GetFeaturesForLevel» – метод для получения списка идентификаторов признаков, связанных с уровнем загрязнённости. 
  «AddLevelToFeatureValueMarked» – метод для пометки значения признака как связанного с уровнем загрязнённости. 
  «RemoveLevelToFeatureValueMarked» – метод для удаления пометки связи между значением признака и уровнем загрязнённости. 
  «GetMarkedFeatureValuesForLevel» – метод для получения списка идентификаторов значений признаков, помеченных как связанные с уровнем загрязнённости. 
  «GetFeaturesWithoutValues» – метод для получения списка признаков, у которых нет заданных значений. 
  «GetLevelsWithoutFeatures» – метод для получения списка уровней загрязнённости, у которых нет связанных признаков.

«KnowledgeDBlook» – форма для просмотра базы знаний.
  «KnowledgeDBlook» – конструктор формы «KnowledgeDBlook» для инициализации переменных и настройки формы. 
  «InitializeControls» – метод для инициализации элементов управления формы «KnowledgeDBlook». 
  «DisplayLevelsList» – метод для отображения списка уровней загрязнённости воздуха в форме «KnowledgeDBlook». 
  «DisplayFeaturesForLevelValues» – метод для отображения списка признаков, связанных с выбранным уровнем загрязнённости, в форме «KnowledgeDBlook». 
  «DisplayFeatureValuesForLevel» – метод для отображения значений признака, связанных с выбранным уровнем загрязнённости, в форме «KnowledgeDBlook». 
  «BackButton_Click» – метод для обработки нажатия кнопки "Назад" в форме «KnowledgeDBlook».

«AirPollutionResultForm» – форма для предоставления результатов классификации.
  «AirPollutionResultForm» – конструктор формы «AirPollutionResultForm» для инициализации переменных и настройки формы. 
  «InitializeControls» – метод для инициализации элементов управления формы «AirPollutionResultForm». 
  «DetermineAirPollutionLevels» – метод для определения подходящих и неподходящих уровней загрязнённости воздуха на основе введённых данных. 
  «AirPollutionResultForm_Load» – метод для обработки события загрузки формы «AirPollutionResultForm».


