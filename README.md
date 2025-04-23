# AirClassificator
Структура базы данных (базы знаний):

![изображение](https://github.com/user-attachments/assets/f89eed29-69a0-4959-9c19-2951dc50c0b4)

Программное средство:

![изображение](https://github.com/user-attachments/assets/13573616-61f7-4e3b-b006-917854b41276)

«MainForm» – главная форма, которая показывается пользователю при запуске программы и позволяет осуществить переход к формам «DataInputForm», «KnowledgeBaseEditor».  
Методы  
1 «Edit_knowledge_base_button_Click» – метод для перехода к форме «KnowledgeBaseEditor».  
2 «Input_values_button_Click» – метод для перехода к форме «DataInputForm».  
3 «MainForm_Load» – метод инициализации системных переменных при загрузке формы «MainForm».  

«DataInputForm» – форма для ввода значений признаков для дальнейшей классификации с помощью ИИ («AirQualityPredictor») или с помощью базы знаний («AirPollutionResultForm»).  
Методы  
1 «DataInputForm» – конструктор формы «DataInputForm» для инициализации переменных.  
2 «InitializeControls» – метод для инициализации элементов формы DataInputForm.  
3 «AIPredictionButton_Click» – метод для инициализации создания класса «AirQualityPredictor».  
4 «LoadFeatures» – метод для получения всех признаков из базы знаний.  
5 «FeaturesListBox_SelectedIndexChanged» – метод для обновления списка признаков при нажатии на один из признаков.  
6 «SaveButton_Click» – метод для сохранения введённого значения признака из окна «TextBox» в список «savedValuesListBox».  
7 «BackButton_Click» – метод для возвращения к «MainForm».  
8 «ViewKnowledgeBaseButton_Click» – метод для перехода к форме «KnowledgeDBlook».  
9 «DetermineLevelButton_Click» – метод для перехода к форме «AirPollutionResultForm».  
10 «DataInputForm_Load» – метод инициализации системных переменных при загрузке формы «DataInputForm».  
11 «DataInputForm_Closed» – метод закрытия формы «DataInputForm».  

«KnowledgeBaseEditor» – форма для редактирования базы знаний экспертной системы.  
Методы  
1 «KnowledgeBaseEditor» – конструктор формы «KnowledgeBaseEditor» для инициализации переменных и настройки формы.  
2 «options_list_SelectedIndexChanged» – метод для обработки изменения выбранного элемента в списке «options_list».  
3 «DisplayFeaturesList» – метод для отображения списка признаков.  
4 «DisplayFeatureValuesAndSettings» – метод для отображения значений и настроек выбранного признака.  
5 «DisplayLevelsList» – метод для отображения списка уровней загрязнённости воздуха.  
6 «DisplayFeaturesForLevel» – метод для отображения списка признаков, связанных с выбранным уровнем загрязнённости.  
7 «DisplayLevelsListForValues» – метод для отображения списка уровней загрязнённости, связанных с редактированием значений признаков.  
8 «DisplayFeatureValuesForLevel» – метод для отображения значений признака, связанных с выбранным уровнем загрязнённости.  
9 «DisplayFeaturesForLevelValues» – метод для отображения списка признаков, связанных с редактированием значений для выбранного уровня загрязнённости  
10 «back_button_Click» – метод для обработки нажатия кнопки «Назад».  
11 «KnowledgeBaseEditor_FormClosed» – метод для обработки закрытия формы «KnowledgeBaseEditor».  

«AirQualityPredictor» – класс для инициализации обучения и предоставления модели машинного обучения для дальнейшей классификации уровня загрязнённости воздуха.  
Методы  
1 «AirQualityPredictor» – конструктор класса «AirQualityPredictor» для инициализации переменных и обучения модели.   
2 «TrainModel» – метод для обучения модели машинного обучения на основе выборки.  
3 «Predict» – метод для выполнения предсказания уровня загрязнённости воздуха на основе входных данных.  
4 «CanPredict» – метод для проверки возможности выполнения предсказания на основе введённых данных.  

«KnowledgeDB» – класс для отправки запросов к БД «Knowledge» (Базе знаний).  
Методы  
1 «KnowledgeDB» – конструктор класса «KnowledgeDB» для инициализации подключения к базе данных.  
2 «TestDatabaseConnection» – метод для проверки подключения к базе данных.  
3 «GetAllAirLevels» – метод для получения списка всех уровней загрязнённости воздуха из базы данных.  
4 «GetAirLevelById» – метод для получения названия уровня загрязнённости воздуха по его идентификатору.  
5 «DeleteAirLevelById» – метод для удаления уровня загрязнённости воздуха из базы данных по его идентификатору.  
6 «AddAirLevel» – метод для добавления нового уровня загрязнённости воздуха в базу данных.  
7 «GetAllFeatures» – метод для получения списка всех признаков из базы данных.  
8 «DeleteFeatureById» – метод для удаления признака из базы данных по его идентификатору.  
9 «AddFeature» – метод для добавления нового признака в базу данных.  
10 «GetFeatureValues» – метод для получения списка значений признака из базы данных.  
11 «AddFeatureValue» – метод для добавления нового значения для признака в базу данных.  
12 «DeleteFeatureValue» – метод для удаления значения признака из базы данных.  
13 «AddFeatureToLevel» – метод для связывания признака с уровнем загрязнённости в базе данных.  
14 «RemoveFeatureFromLevel» – метод для удаления связи между признаком и уровнем загрязнённости в базе данных.  
15 «GetFeaturesForLevel» – метод для получения списка идентификаторов признаков, связанных с уровнем загрязнённости.  
16 «AddLevelToFeatureValueMarked» – метод для пометки значения признака как связанного с уровнем загрязнённости.  
17 «RemoveLevelToFeatureValueMarked» – метод для удаления пометки связи между значением признака и уровнем загрязнённости.  
18 «GetMarkedFeatureValuesForLevel» – метод для получения списка идентификаторов значений признаков, помеченных как связанные с уровнем загрязнённости.  
19 «GetFeaturesWithoutValues» – метод для получения списка признаков, у которых нет заданных значений.  
20 «GetLevelsWithoutFeatures» – метод для получения списка уровней загрязнённости, у которых нет связанных признаков.  

«KnowledgeDBlook» – форма для просмотра базы знаний.  
Методы  
1 «KnowledgeDBlook» – конструктор формы «KnowledgeDBlook» для инициализации переменных и настройки формы.  
2 «InitializeControls» – метод для инициализации элементов управления формы «KnowledgeDBlook».  
3 «DisplayLevelsList» – метод для отображения списка уровней загрязнённости воздуха в форме «KnowledgeDBlook».  
4 «DisplayFeaturesForLevelValues» – метод для отображения списка признаков, связанных с выбранным уровнем загрязнённости, в форме «KnowledgeDBlook».  
5 «DisplayFeatureValuesForLevel» – метод для отображения значений признака, связанных с выбранным уровнем загрязнённости, в форме «KnowledgeDBlook».  
6 «BackButton_Click» – метод для обработки нажатия кнопки "Назад" в форме «KnowledgeDBlook».  

«AirPollutionResultForm» – форма для предоставления результатов классификации.  
Методы  
1 «AirPollutionResultForm» – конструктор формы «AirPollutionResultForm» для инициализации переменных и настройки формы.  
2 «InitializeControls» – метод для инициализации элементов управления формы «AirPollutionResultForm».  
3 «DetermineAirPollutionLevels» – метод для определения подходящих и неподходящих уровней загрязнённости воздуха на основе введённых данных.  
4 «AirPollutionResultForm_Load» – метод для обработки события загрузки формы «AirPollutionResultForm».  


