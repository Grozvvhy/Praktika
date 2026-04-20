using HranitelPROGeneralDepartmentTerminal.Data;
using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class VisitorRegisterWindow : Window
    {
        public VisitorRegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль.");
                return;
            }

            string passwordHash = ComputeMd5Hash(password);

            // Используем хранимую процедуру sp_register_user
            string sql = "SELECT sp_register_user(@email, @hash)";
            var parameters = new[]
            {
                new NpgsqlParameter("@email", email),
                new NpgsqlParameter("@hash", passwordHash)
            };
            var result = DatabaseHelper.ExecuteScalar(sql, parameters);

            if (result != null && Convert.ToBoolean(result))
            {
                MessageBox.Show("Регистрация успешна. Теперь вы можете войти.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Пользователь с таким email уже существует.");
            }
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