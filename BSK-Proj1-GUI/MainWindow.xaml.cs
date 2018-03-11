using BSK_Proj1_Logic;
using System.ComponentModel;
using System.Windows;

namespace BSK_Proj1_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _filename;

        private TempClass _tempClass;
        private BackgroundWorker _backgroundWorker;

        public MainWindow()
        {
            InitializeComponent();
            DecryptButton.IsEnabled = false;
            EncryptButton.IsEnabled = false;

            _backgroundWorker = new BackgroundWorker();
            _tempClass = new TempClass(_backgroundWorker);
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

        private async void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            DoWorkEventHandler workTask = (s, args) => _tempClass.EncryptFile(_filename);
            RunWorkerCompletedEventHandler workCompleted = null;
            workCompleted = (s, args) =>
            {
                MessageBox.Show("Done");

                // Remove this task from list, so when decrypt is called
                // right after encrypt it does not encrypt second time
                _backgroundWorker.DoWork -= workTask;
                _backgroundWorker.RunWorkerCompleted -= workCompleted;
            };
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += workTask;
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressChanged);
            _backgroundWorker.RunWorkerCompleted += workCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            DoWorkEventHandler workTask = (s, args) => _tempClass.DecryptFile(_filename);
            RunWorkerCompletedEventHandler workCompleted = null;
            workCompleted = (s, args) =>
            {
                MessageBox.Show("Done");

                // Remove this task from list, so when decrypt is called
                // right after encrypt it does not encrypt second time
                _backgroundWorker.DoWork -= workTask;
                _backgroundWorker.RunWorkerCompleted -= workCompleted;
            };
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += workTask;
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressChanged);
            _backgroundWorker.RunWorkerCompleted += workCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private void UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessProgressBar.Value = (e.ProgressPercentage);
        }
    }
}
