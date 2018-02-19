using System;
using System.Windows.Controls;
using System.Windows.Input;
using MonoTorrent.Common;

namespace Outflow
{
    public partial class MainWindow
    {

        public MainWindow() => InitializeComponent();

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void CloseWindowCommandHandler(object sender, EventArgs e)
        {
            (DataContext as ApplicationViewModel)?.ExitCommand.Execute(null);
        }

        private void OpenWindowCommandHandler(object sender, EventArgs e)
        {
            (DataContext as ApplicationViewModel)?.OpenCommand.Execute(null);
        }
    }
}
