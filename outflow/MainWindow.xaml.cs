using System;
using System.Windows.Controls;

namespace Outflow
{
    public partial class MainWindow
    {

        public MainWindow() => InitializeComponent();

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) // обработчик события рендеринга строки в списке торрентов, который генерирует порядковый номер (индекс) строки
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void CloseWindowCommandHandler(object sender, EventArgs e) // обработчтик события закрытия главного окна приложения, который вызывает команду выхода из приложения
        {
            (DataContext as ApplicationViewModel)?.ExitCommand.Execute(null);
        }

        private void OpenWindowCommandHandler(object sender, EventArgs e)// обработчтик события закрытия главного окна приложения, который вызывает команду восстановления торрентов прошлой сессии
        {
            (DataContext as ApplicationViewModel)?.OpenCommand.Execute(null);
        }
    }
}
