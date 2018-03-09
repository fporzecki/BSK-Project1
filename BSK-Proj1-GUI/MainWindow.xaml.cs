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
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (s, args) => _tempClass.EncryptFile();
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressChanged);
            _backgroundWorker.RunWorkerCompleted += (s, args) => MessageBox.Show("Done");
            _backgroundWorker.RunWorkerAsync();
        }

        private void UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessProgressBar.Value = (e.ProgressPercentage);
        }
    }
}
