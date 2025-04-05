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
    public partial class MainForm : Form
    {
        private KnowledgeBaseEditor knowledgeBaseEditor;
        private DataInputForm dataInputForm;
        public MainForm()
        {
            InitializeComponent();
        }

        private void edit_knowledge_base_button_Click(object sender, EventArgs e)
        {
            if (knowledgeBaseEditor == null || knowledgeBaseEditor.IsDisposed)
            {
                knowledgeBaseEditor = new KnowledgeBaseEditor(this); // Создаём экземпляр, передаём ссылку на MainForm
            }
            knowledgeBaseEditor.Show(); // Показываем KnowledgeBaseEditor
            this.Hide(); // Скрываем MainForm
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {}

        private void input_values_button_Click(object sender, EventArgs e)
        {
            if (dataInputForm == null || dataInputForm.IsDisposed)
            {
                dataInputForm = new DataInputForm(this); // Создаём экземпляр, передаём ссылку на MainForm
            }
            dataInputForm.Show(); // Показываем dataInputForm
            this.Hide(); // Скрываем MainForm
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
