using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsConverter
{
    public partial class Form1 : Form
    {
        Button convertFileButton = new Button();
        TextBox pathInputTextBox = new TextBox();
        TextBox pathSaveTextBox = new TextBox();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        string inputPath = "";
        string outputPath = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Кнопка конвертации архива
            FillInButton(convertFileButton, "Convert File", 60, 20, 10, 70, convertFileButton_Click);

            // Поле для ввода пути к zip архиву с исходными файлами
            FillInTextBox(pathInputTextBox, "Enter path to file", 500, 20, 10, 10, pathInputTextBox_MouseClick);

            // Поле для ввода пути, где будет создан zip архив с результатами конвертации
            FillInTextBox(pathSaveTextBox, "Enter save path to file", 500, 20, 10, 40, pathSaveTextBox_MouseClick);
        }

        private void convertFileButton_Click(object sender, EventArgs e)
        {

        }

        // Принять путь к архиву
        private void pathInputTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                pathInputTextBox.Text = openFileDialog.FileName;
                inputPath = pathInputTextBox.Text;
            }
        }

        // Принять путь к сохранению архива
        private void pathSaveTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                pathSaveTextBox.Text = folderBrowserDialog.SelectedPath;
                outputPath = pathSaveTextBox.Text;
            }
        }

        // Функция для динамического присвоения параметров TextBox
        private void FillInTextBox(TextBox textBox, string text, int width, int height, int horizontalPoint, int verticalPoint, MouseEventHandler mouseEventHandler)
        {
            textBox.Text = text;
            textBox.Size = new Size(width, height);
            textBox.Location = new Point(horizontalPoint, verticalPoint);
            textBox.MouseClick += mouseEventHandler;
            Controls.Add(textBox);
        }

        // Функция для динамического присвоения параметров Button
        private void FillInButton(Button button, string text, int width, int height, int horizontalPoint, int verticalPoint, EventHandler eventHandler)
        {
            button.Text = text;
            button.Size = new Size(width, height);
            button.Location = new Point(horizontalPoint, verticalPoint);
            button.Click += new EventHandler(eventHandler);
            Controls.Add(button);
        }
    }
}
