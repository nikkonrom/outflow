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
using MahApps.Metro.Controls;

namespace Outflow
{
    /// <summary>
    /// Interaction logic for FolderDialogWIndow.xaml
    /// </summary>
    public partial class FolderDialogWIndow : MetroWindow
    {
        public FolderDialogWIndow(string torrentName)
        {
            InitializeComponent();
            this.Title = torrentName;
            var path = System.IO.Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            DownloadFolderPath.Text = string.IsNullOrEmpty(path) ? "Choose a path..." : System.IO.Path.Combine(path, "Downloads");
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                DownloadFolderPath.Text = dialog.SelectedPath;
            }
        }

        private void FolderDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void FolderDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void startTorentTextLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startCheckBox.IsChecked = startCheckBox.IsChecked != true;
        }
    }
}
