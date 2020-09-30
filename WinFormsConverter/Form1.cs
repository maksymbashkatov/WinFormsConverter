using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsConverter
{
    public partial class Form1 : Form
    {
        Button convertButton = new Button();
        Button cancelButton = new Button();
        TextBox pathInputTextBox = new TextBox();
        TextBox pathSaveTextBox = new TextBox();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        // Путь откуда берётся архив
        string pathToOriginalArchive;
        // Путь куда сохраняется архив
        string pathToConvertedArchive = "";
        string[] pathToOriginalArchiveArray;
        string archiveName = "";
        string zipDirectory = "";
        string directoryForExtraction;

        // Массив каталогов, из tempFolder каталога
        string[] arrayOfDirectories1;
        // Массив строк из файла
        string[] arrayOfStringsInFile;
        // Массив байтов из файла
        byte[] arrayOfFileBytes;

        string brokerName;
        string currCode;
        string year = "";
        string month = "";
        string day = "";
        string hour = "";
        string minute = "";
        string second = "";
        string unixTime;
        string bid;
        string ask;

        string fileName = "";
        // Массив для хранения строк файлов
        string[] arrayOfFileLines;
        DateTime dateTime;
        string tempFolder2 = "";
        // Триггер записи в файл с подобным именем
        bool writeToSameFile = true;
        // Хранит текст файла в виде строки
        string textFromFile = "";
        // Хранит список уникальных строк файла
        ArrayList listOfUniqueStringsInFile;

        BackgroundWorker bw = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Кнопка конвертации архива
            FillInButton(convertButton, "START", 60, 20, 10, 70, convertButton_Click);

            // Кнопка отмены конвертации архива
            FillInButton(cancelButton, "STOP", 60, 20, 80, 70, cancelButton_Click);

            // Поле для ввода пути к zip архиву с исходными файлами
            FillInTextBox(pathInputTextBox, "Enter path to file", 500, 20, 10, 10, pathInputTextBox_MouseClick);

            // Поле для ввода пути, где будет создан zip архив с результатами конвертации
            FillInTextBox(pathSaveTextBox, "Enter save path to file", 500, 20, 10, 40, pathSaveTextBox_MouseClick);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            // Распаковка архива в директорию
            pathToOriginalArchiveArray = pathToOriginalArchive.Split(new string[] { @"\" }, StringSplitOptions.None);
            archiveName = pathToOriginalArchiveArray[pathToOriginalArchiveArray.Length - 1];
            for (int i = 0; i < pathToOriginalArchiveArray.Length - 1; i++)
            {
                zipDirectory += (i < 1) ? pathToOriginalArchiveArray[i] : $@"\{pathToOriginalArchiveArray[i]}";
            }
            directoryForExtraction = $@"{zipDirectory}\tempFolder1";
            ZipFile.ExtractToDirectory(pathToOriginalArchive, directoryForExtraction);

            // Получить список каталогов, из tempFolder каталога
            arrayOfDirectories1 = Directory.GetDirectories(directoryForExtraction);

            foreach (string oneOfListOfDirectories1 in arrayOfDirectories1)
            {
                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                // Получить список каталогов, из католога
                string[] listOfDirectories2 = Directory.GetDirectories(oneOfListOfDirectories1);
                foreach (string oneOfListOfDirectories2 in listOfDirectories2)
                {
                    if (bw.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }
                    string[] arrayOldPathToFile = oneOfListOfDirectories2.Split(new string[] { @"\" }, StringSplitOptions.None);
                    brokerName = arrayOldPathToFile[arrayOldPathToFile.Length - 2];
                    currCode = arrayOldPathToFile[arrayOldPathToFile.Length - 1];
                    // Получить список файлов в конечном каталоге
                    string[] listOfFiles = Directory.GetFiles(oneOfListOfDirectories2);
                    foreach (string oneOflistOfFiles in listOfFiles)
                    {
                        if (bw.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }
                        FileInfo fileInfo = new FileInfo(oneOflistOfFiles);
                        string[] tempStrArr = oneOflistOfFiles.Split(new string[] { @"\" }, StringSplitOptions.None);
                        string[] tempFileNameStr = tempStrArr[tempStrArr.Length - 1].Split(new char[] { '_', '.' }, StringSplitOptions.None);
                        DateTime cutDataTime = UnixTimeToDateTime(Convert.ToDouble(tempFileNameStr[tempFileNameStr.Length - 2]));
                        // Сконвертированное имя файла
                        string newFilename = tempFileNameStr[0] + "_" + cutDataTime.Year +
                            (cutDataTime.Month < 10 ? "0" + cutDataTime.Month : cutDataTime.Month.ToString()) +
                            (cutDataTime.Day < 10 ? "0" + cutDataTime.Day : cutDataTime.Day.ToString()) +
                            "." + tempFileNameStr[tempFileNameStr.Length - 1];

                        // Сроверка на сопадение имени файла
                        if (newFilename != fileName)
                        {
                            fileName = newFilename;
                            writeToSameFile = false;
                        }
                        else
                        {
                            writeToSameFile = true;
                        }

                        // Чтение из файла
                        using (FileStream fileStream = File.OpenRead(oneOflistOfFiles))
                        {
                            if (bw.CancellationPending == true)
                            {
                                e.Cancel = true;
                                break;
                            }
                            arrayOfFileBytes = new byte[fileStream.Length];
                            // Асинхронное чтение файла
                            fileStream.Read(arrayOfFileBytes, 0, arrayOfFileBytes.Length);

                            if (writeToSameFile == false)
                            {
                                // Декодирование байтов в строку
                                textFromFile = System.Text.Encoding.Default.GetString(arrayOfFileBytes);
                            }
                            else
                            {
                                textFromFile = textFromFile + "\r\n" + System.Text.Encoding.Default.GetString(arrayOfFileBytes);
                            }
                            // Разделить текст всего файла на строки и поместить в массив
                            arrayOfStringsInFile = textFromFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < arrayOfStringsInFile.Length; i++)
                            {
                                if (bw.CancellationPending == true)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                                arrayOfFileLines = arrayOfStringsInFile[i].Split(new string[] { ";" }, StringSplitOptions.None);
                                unixTime = arrayOfFileLines[0];
                                bid = arrayOfFileLines[1];
                                ask = arrayOfFileLines[2];
                                dateTime = UnixTimeToDateTime(Convert.ToDouble(unixTime));
                                year = dateTime.Year.ToString();
                                month = dateTime.Month < 10 ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString();
                                day = dateTime.Day < 10 ? "0" + dateTime.Day.ToString() : dateTime.Day.ToString();
                                hour = dateTime.Hour < 10 ? "0" + dateTime.Hour.ToString() : dateTime.Hour.ToString();
                                minute = dateTime.Minute < 10 ? "0" + dateTime.Minute.ToString() : dateTime.Minute.ToString();
                                second = dateTime.Second < 10 ? "0" + dateTime.Second.ToString() : dateTime.Second.ToString();
                                // Запись сконвертированной строки в файл
                                arrayOfStringsInFile[i] = currCode + "," + year + month + day + "," + hour + minute + second + "," + ask + "," + bid;
                            }

                            listOfUniqueStringsInFile = new ArrayList();
                            listOfUniqueStringsInFile.Add(arrayOfStringsInFile[0]);

                            // Запись только уникальных строк из массива в список
                            for (int i = 0; i < arrayOfStringsInFile.Length; i++)
                            {
                                if (bw.CancellationPending == true)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                                if (!(listOfUniqueStringsInFile.Contains(arrayOfStringsInFile[i])))
                                {
                                    listOfUniqueStringsInFile.Add(arrayOfStringsInFile[i]);
                                }
                            }

                            // Сортируем список
                            listOfUniqueStringsInFile.Sort();
                        }
                        // Создание временной директории для сконвертированной структуры пути
                        tempFolder2 = zipDirectory + @"\tempFolder2\" + @"\" + brokerName + @"\" + year + @"\" + month;
                        Directory.CreateDirectory(tempFolder2);

                        // Запись в файл
                        using (FileStream fstream = new FileStream(tempFolder2 + @"\" + fileName, FileMode.OpenOrCreate))
                        {
                            if (bw.CancellationPending == true)
                            {
                                e.Cancel = true;
                                break;
                            }
                            arrayOfFileBytes = System.Text.Encoding.Default.GetBytes(String.Join("\r\n", (string[])listOfUniqueStringsInFile.ToArray(typeof(string))));
                            // Асинхронная запись массива байтов в файл
                            fstream.Write(arrayOfFileBytes, 0, arrayOfFileBytes.Length);
                        }

                    }
                }
            }

            if (bw.CancellationPending != true)
            {
                // Архивировать директорию (путь сохранения архива)
                pathToConvertedArchive = $@"{pathToConvertedArchive}\Converted_{archiveName}";
                ZipFile.CreateFromDirectory($@"{zipDirectory}\tempFolder2", pathToConvertedArchive);
            }
            else
            {
                e.Cancel = true;
            }

            // Удаление временных папок
            if (Directory.Exists(zipDirectory + @"\tempFolder1"))
            {
                Directory.Delete(zipDirectory + @"\tempFolder1", true);
            }
            if (Directory.Exists(zipDirectory + @"\tempFolder2"))
            {
                Directory.Delete(zipDirectory + @"\tempFolder2", true);
            }
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            //BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync();//собственно запускаем фоновый поток
            bw.WorkerSupportsCancellation = true;
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
                pathToOriginalArchive = pathInputTextBox.Text;
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
                pathToConvertedArchive = pathSaveTextBox.Text;
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

        // Функция преобразования unix time в обычное время
        private static DateTime UnixTimeToDateTime(double UnixTime)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
            return dateTime.AddSeconds(UnixTime);
        }
    }
}
