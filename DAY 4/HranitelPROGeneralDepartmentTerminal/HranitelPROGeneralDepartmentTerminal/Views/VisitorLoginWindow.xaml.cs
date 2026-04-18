using HranitelPROGeneralDepartmentTerminal.Data;
using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class VisitorLoginWindow : Window
    {
        public VisitorLoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль.");
                return;
            }

            // Хеширование пароля (MD5 как в дампе, но лучше использовать более стойкий алгоритм)
            string passwordHash = ComputeMd5Hash(password);

            // Проверка через хранимую процедуру (один из трёх способов)
            string sql = "SELECT sp_login_user(@email, @hash)";
            var parameters = new[]
            {
                new NpgsqlParameter("@email", email),
                new NpgsqlParameter("@hash", passwordHash)
            };
            var result = DatabaseHelper.ExecuteScalar(sql, parameters);

            if (result != null && Convert.ToBoolean(result))
            {
                // Получаем ID пользователя
                string userIdSql = "SELECT id FROM users WHERE email = @email";
                var userIdParam = new NpgsqlParameter("@email", email);
                int userId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(userIdSql, new[] { userIdParam }));

                // Открываем главное окно посетителя (у вас должно быть VisitorMainWindow)
                var visitorMain = new VisitorMainWindow(userId);
                visitorMain.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный email или пароль.");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно регистрации (должно быть реализовано)
            var registerWindow = new VisitorRegisterWindow();
            registerWindow.ShowDialog();
        }

        private string ComputeMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}