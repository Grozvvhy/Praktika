using System.Windows;
using HranitelPROGeneralDepartmentTerminal.Views;

namespace HranitelPROGeneralDepartmentTerminal
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void VisitorLogin_Click(object sender, RoutedEventArgs e)
        {
            // Замените VisitorLoginWindow на реальное имя вашего окна авторизации посетителя
            var visitorLogin = new VisitorLoginWindow();
            visitorLogin.Show();
            this.Close();
        }

        private void GeneralDeptLogin_Click(object sender, RoutedEventArgs e)
        {
            var generalLogin = new GeneralDepartmentLoginWindow();
            generalLogin.Show();
            this.Close();
        }
    }
}