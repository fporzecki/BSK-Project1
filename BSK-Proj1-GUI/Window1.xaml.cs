using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BSK_Proj1_GUI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FileNameDialog : Window
    {
        // passing filename through prop is probably bad practise, but who cares now anyway
        public string FileName { get; set; }
        public FileNameDialog()
        {
            InitializeComponent();
        }

        // include some form of input sterilization
        private void button_Click(object sender, RoutedEventArgs e)
        {
            FileName = OutputFileNameTextBox.Text;
            this.Close();
        }
    }
}
