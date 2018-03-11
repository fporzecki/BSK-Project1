using BSK_Proj1_Logic;
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
            _encryptionWorker = new EncryptionWorker();
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

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
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

            var cipherMode = GetCipherMode();
            DoWorkEventHandler workTask = (s, args) => _encryptionWorker.EncryptFile(_filename, outputName, cipherMode);
            _backgroundWorker = AssignBackgroundWorkerTasks(workTask);
            _encryptionWorker.BackgroundWorker = _backgroundWorker;
            _backgroundWorker.RunWorkerAsync();
        }

        private CipherMode GetCipherMode()
        {
            var encryptionMode = EncryptionModeComboBox.Text;
            var cipherMode = CipherMode.ECB;
            if (encryptionMode == "CBC")
                cipherMode = CipherMode.CBC;
            if (encryptionMode == "OFB")
                cipherMode = CipherMode.OFB;
            if (encryptionMode == "CFB")
                cipherMode = CipherMode.CFB;

            return cipherMode;
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            DoWorkEventHandler workTask = (s, args) => _encryptionWorker.DecryptFile(_filename);
            _backgroundWorker = AssignBackgroundWorkerTasks(workTask);
            _encryptionWorker.BackgroundWorker = _backgroundWorker;
            _backgroundWorker.RunWorkerAsync();
        }

        private BackgroundWorker AssignBackgroundWorkerTasks(DoWorkEventHandler doWorkEventHandler)
        {
            var backgroundWorker = new BackgroundWorker();
            RunWorkerCompletedEventHandler workCompleted = null;
            workCompleted = (s, args) =>
            {
                MessageBox.Show("Done");

                // Remove this task from list, so when decrypt is called
                // right after encrypt it does not encrypt second time
                backgroundWorker.DoWork -= doWorkEventHandler;
                backgroundWorker.RunWorkerCompleted -= workCompleted;
            };
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += doWorkEventHandler;
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressChanged);
            backgroundWorker.RunWorkerCompleted += workCompleted;

            return backgroundWorker;
        }

        private void UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessProgressBar.Value = (e.ProgressPercentage);
        }
    }
}
