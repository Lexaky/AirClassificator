namespace AirClassificator
{
    partial class KnowledgeBaseEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.knowledge_base_editor_lable = new System.Windows.Forms.Label();
            this.back_button = new System.Windows.Forms.Button();
            this.options_list = new System.Windows.Forms.ListBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelDetails = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // knowledge_base_editor_lable
            // 
            this.knowledge_base_editor_lable.AutoSize = true;
            this.knowledge_base_editor_lable.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.knowledge_base_editor_lable.Location = new System.Drawing.Point(580, 0);
            this.knowledge_base_editor_lable.Name = "knowledge_base_editor_lable";
            this.knowledge_base_editor_lable.Size = new System.Drawing.Size(188, 23);
            this.knowledge_base_editor_lable.TabIndex = 1;
            this.knowledge_base_editor_lable.Text = "Редактор базы знаний";
            this.knowledge_base_editor_lable.UseMnemonic = false;
            // 
            // back_button
            // 
            this.back_button.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.back_button.Location = new System.Drawing.Point(13, 0);
            this.back_button.Name = "back_button";
            this.back_button.Size = new System.Drawing.Size(200, 23);
            this.back_button.TabIndex = 2;
            this.back_button.Text = "Назад";
            this.back_button.UseVisualStyleBackColor = true;
            this.back_button.Click += new System.EventHandler(this.back_button_Click);
            // 
            // options_list
            // 
            this.options_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.options_list.FormattingEnabled = true;
            this.options_list.ItemHeight = 16;
            this.options_list.Location = new System.Drawing.Point(0, 0);
            this.options_list.Name = "options_list";
            this.options_list.Size = new System.Drawing.Size(251, 631);
            this.options_list.TabIndex = 3;
            // 
            // splitContainer
            // 
            this.splitContainer.Location = new System.Drawing.Point(13, 29);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.options_list);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelDetails);
            this.splitContainer.Size = new System.Drawing.Size(755, 631);
            this.splitContainer.SplitterDistance = 251;
            this.splitContainer.TabIndex = 4;
            // 
            // panelDetails
            // 
            this.panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetails.Location = new System.Drawing.Point(0, 0);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(500, 631);
            this.panelDetails.TabIndex = 0;
            // 
            // KnowledgeBaseEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 672);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.back_button);
            this.Controls.Add(this.knowledge_base_editor_lable);
            this.Name = "KnowledgeBaseEditor";
            this.Text = "KnowledgeBaseEditor";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label knowledge_base_editor_lable;
        private System.Windows.Forms.Button back_button;
        private System.Windows.Forms.ListBox options_list;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelDetails;
    }
}