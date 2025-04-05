namespace AirClassificator
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.edit_knowledge_base_button = new System.Windows.Forms.Button();
            this.input_values_button = new System.Windows.Forms.Button();
            this.describe_app_name = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // edit_knowledge_base_button
            // 
            this.edit_knowledge_base_button.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.edit_knowledge_base_button.Location = new System.Drawing.Point(93, 133);
            this.edit_knowledge_base_button.Name = "edit_knowledge_base_button";
            this.edit_knowledge_base_button.Size = new System.Drawing.Size(289, 108);
            this.edit_knowledge_base_button.TabIndex = 0;
            this.edit_knowledge_base_button.Text = "Редактировать базу знаний";
            this.edit_knowledge_base_button.UseVisualStyleBackColor = true;
            this.edit_knowledge_base_button.Click += new System.EventHandler(this.edit_knowledge_base_button_Click);
            // 
            // input_values_button
            // 
            this.input_values_button.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.input_values_button.Location = new System.Drawing.Point(429, 133);
            this.input_values_button.Name = "input_values_button";
            this.input_values_button.Size = new System.Drawing.Size(289, 108);
            this.input_values_button.TabIndex = 1;
            this.input_values_button.Text = "Ввод исходных значений";
            this.input_values_button.UseVisualStyleBackColor = true;
            this.input_values_button.Click += new System.EventHandler(this.input_values_button_Click);
            // 
            // describe_app_name
            // 
            this.describe_app_name.AutoSize = true;
            this.describe_app_name.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.describe_app_name.Location = new System.Drawing.Point(187, 27);
            this.describe_app_name.Name = "describe_app_name";
            this.describe_app_name.Size = new System.Drawing.Size(432, 31);
            this.describe_app_name.TabIndex = 2;
            this.describe_app_name.Text = "Классификатор загрязнённости воздуха";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.describe_app_name);
            this.Controls.Add(this.input_values_button);
            this.Controls.Add(this.edit_knowledge_base_button);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button edit_knowledge_base_button;
        private System.Windows.Forms.Button input_values_button;
        private System.Windows.Forms.Label describe_app_name;
    }
}

