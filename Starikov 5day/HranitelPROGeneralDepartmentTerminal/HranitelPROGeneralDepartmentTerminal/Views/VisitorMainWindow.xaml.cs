using System.Windows;
using System.Windows.Controls;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class VisitorMainWindow : Window
    {
        private readonly int _userId;

        public VisitorMainWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            // По умолчанию показываем список заявок пользователя
            MyRequests_Click(null, null);
        }

        private void IndividualRequest_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно индивидуальной заявки (у вас уже должно быть)
            var individualWindow = new IndividualRequestWindow(_userId);
            individualWindow.ShowDialog();
        }

        private void GroupRequest_Click(object sender, RoutedEventArgs e)
        {
            var groupWindow = new GroupRequestWindow(_userId);
            groupWindow.ShowDialog();
        }

        private void MyRequests_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем страницу со списком заявок (можно использовать Frame)
            var requestsPage = new MyRequestsPage(_userId);
            MainFrame.Content = requestsPage;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}