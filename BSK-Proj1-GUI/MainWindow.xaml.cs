using BSK_Proj1_Logic;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace BSK_Proj1_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _filename;

        private EncryptionWorker _encryptionWorker;
        private BackgroundWorker _backgroundWorker;

        public MainWindow()
        {
            InitializeComponent();
            DecryptButton.IsEnabled = false;
            EncryptButton.IsEnabled = false;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _encryptionWorker = new EncryptionWorker(_backgroundWorker);
        }

        private void ChooseFileButtom_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();

            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                _filename = fileDialog.FileName;
                FileNameLabel.Content = _filename;
                DecryptButton.IsEnabled = true;
                EncryptButton.IsEnabled = true;
            }
        }

        private void TriggerControlsState()
        {
            EncryptButton.IsEnabled = !EncryptButton.IsEnabled;
            DecryptButton.IsEnabled = !DecryptButton.IsEnabled;
            ChooseFileButtom.IsEnabled = !ChooseFileButtom.IsEnabled;
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerControlsState();

            var fileNameDialog = new FileNameDialog();
            fileNameDialog.ShowDialog();
            var outputName = string.Empty;
            if (fileNameDialog.FileName != string.Empty)
            {
                var filePath = Path.GetDirectoryName(_filename);
                var extension = Path.GetExtension(_filename);
                var newPath = Path.Combine(filePath, fileNameDialog.FileName);
                outputName = newPath + extension;
            }
            else
            {
                outputName = _filename;
            }

            var cipherMode = GetCipherModeFromGui();
            DoWorkEventHandler workTask = (s, args) => _encryptionWorker.EncryptFile(_filename, outputName, cipherMode);
            AssignBackgroundWorkerTasks(workTask);
            _backgroundWorker.RunWorkerAsync();
        }

        private CipherMode GetCipherModeFromGui()
        {
            var encryptionMode = EncryptionModeComboBox.Text;
            return HelperClass.GetCipherMode(encryptionMode);
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerControlsState();
            DoWorkEventHandler workTask = (s, args) => _encryptionWorker.DecryptXmlFile(_filename);
            AssignBackgroundWorkerTasks(workTask);
            _backgroundWorker.RunWorkerAsync();
        }

        private void AssignBackgroundWorkerTasks(DoWorkEventHandler doWorkEventHandler)
        {
            RunWorkerCompletedEventHandler workCompleted = null;
            workCompleted = (s, args) =>
            {
                MessageBox.Show("Done");

                // Remove this task from list, so when decrypt is called
                // right after encrypt it does not encrypt second time
                _backgroundWorker.DoWork -= doWorkEventHandler;
                _backgroundWorker.RunWorkerCompleted -= workCompleted;
                TriggerControlsState();
            };
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += doWorkEventHandler;
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressChanged);
            _backgroundWorker.RunWorkerCompleted += workCompleted;
        }

        private void UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessProgressBar.Value = (e.ProgressPercentage);
        }

        private void LoginTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void PasswordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
