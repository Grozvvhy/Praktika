using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SaverH.Models;
using SaverH.Services;

namespace SaverH.Forms
{
    public partial class MainForm : Form
    {
        private DatabaseService dbService;
        private int currentUserId;
        private string currentUserEmail;

        public MainForm()
        {
            InitializeComponent();
            dbService = new DatabaseService();
            this.Text = "Система управления заявками посетителей";
            this.Size = new System.Drawing.Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            ShowAuthPanel();
        }

        private void ShowAuthPanel()
        {
            // Очищаем форму
            this.Controls.Clear();

            // Создаем панель авторизации
            System.Windows.Forms.Panel authPanel = new System.Windows.Forms.Panel();
            authPanel.Dock = DockStyle.Fill;
            this.Controls.Add(authPanel);

            System.Windows.Forms.Label lblTitle = new System.Windows.Forms.Label();
            lblTitle.Text = "Добро пожаловать в систему управления заявками";
            lblTitle.Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold);
            lblTitle.Size = new System.Drawing.Size(600, 40);
            lblTitle.Location = new System.Drawing.Point((this.Width - 600) / 2, 50);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            authPanel.Controls.Add(lblTitle);

            System.Windows.Forms.TabControl authTabControl = new System.Windows.Forms.TabControl();
            authTabControl.Size = new System.Drawing.Size(450, 400);
            authTabControl.Location = new System.Drawing.Point((this.Width - 450) / 2, 120);
            authPanel.Controls.Add(authTabControl);

            // Вкладка входа
            TabPage loginTab = new TabPage("Вход");
            authTabControl.TabPages.Add(loginTab);

            System.Windows.Forms.Label lblLoginEmail = new System.Windows.Forms.Label() { Text = "Email:", Location = new System.Drawing.Point(50, 50), Size = new System.Drawing.Size(100, 25) };
            System.Windows.Forms.TextBox txtLoginEmail = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(150, 50), Size = new System.Drawing.Size(250, 25) };
            System.Windows.Forms.Label lblLoginPassword = new System.Windows.Forms.Label() { Text = "Пароль:", Location = new System.Drawing.Point(50, 90), Size = new System.Drawing.Size(100, 25) };
            System.Windows.Forms.TextBox txtLoginPassword = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(150, 90), Size = new System.Drawing.Size(250, 25), PasswordChar = '*' };

            System.Windows.Forms.ComboBox cmbLoginMethod = new System.Windows.Forms.ComboBox()
            {
                Location = new System.Drawing.Point(150, 130),
                Size = new System.Drawing.Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLoginMethod.Items.AddRange(new object[] { "SQL запрос", "Хранимая процедура", "ORM-подход" });
            cmbLoginMethod.SelectedIndex = 0;

            System.Windows.Forms.Label lblMethod = new System.Windows.Forms.Label() { Text = "Способ:", Location = new System.Drawing.Point(50, 130), Size = new System.Drawing.Size(100, 25) };

            System.Windows.Forms.Button btnLogin = new System.Windows.Forms.Button() { Text = "Войти", Location = new System.Drawing.Point(150, 180), Size = new System.Drawing.Size(100, 35), BackColor = System.Drawing.Color.LightGreen };

            loginTab.Controls.AddRange(new System.Windows.Forms.Control[] { lblLoginEmail, txtLoginEmail, lblLoginPassword, txtLoginPassword, lblMethod, cmbLoginMethod, btnLogin });

            // Вкладка регистрации
            TabPage registerTab = new TabPage("Регистрация");
            authTabControl.TabPages.Add(registerTab);

            System.Windows.Forms.Label lblRegEmail = new System.Windows.Forms.Label() { Text = "Email:", Location = new System.Drawing.Point(50, 50), Size = new System.Drawing.Size(100, 25) };
            System.Windows.Forms.TextBox txtRegEmail = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(150, 50), Size = new System.Drawing.Size(250, 25) };
            System.Windows.Forms.Label lblRegPassword = new System.Windows.Forms.Label() { Text = "Пароль:", Location = new System.Drawing.Point(50, 90), Size = new System.Drawing.Size(100, 25) };
            System.Windows.Forms.TextBox txtRegPassword = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(150, 90), Size = new System.Drawing.Size(250, 25), PasswordChar = '*' };
            System.Windows.Forms.Label lblRegConfirm = new System.Windows.Forms.Label() { Text = "Подтверждение:", Location = new System.Drawing.Point(50, 130), Size = new System.Drawing.Size(100, 25) };
            System.Windows.Forms.TextBox txtRegConfirm = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(150, 130), Size = new System.Drawing.Size(250, 25), PasswordChar = '*' };

            System.Windows.Forms.ComboBox cmbRegMethod = new System.Windows.Forms.ComboBox()
            {
                Location = new System.Drawing.Point(150, 170),
                Size = new System.Drawing.Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRegMethod.Items.AddRange(new object[] { "SQL запрос", "Хранимая процедура", "ORM-подход" });
            cmbRegMethod.SelectedIndex = 0;

            System.Windows.Forms.Label lblRegMethod = new System.Windows.Forms.Label() { Text = "Способ:", Location = new System.Drawing.Point(50, 170), Size = new System.Drawing.Size(100, 25) };

            System.Windows.Forms.Button btnRegister = new System.Windows.Forms.Button() { Text = "Зарегистрироваться", Location = new System.Drawing.Point(150, 220), Size = new System.Drawing.Size(150, 35), BackColor = System.Drawing.Color.LightBlue };

            registerTab.Controls.AddRange(new System.Windows.Forms.Control[] { lblRegEmail, txtRegEmail, lblRegPassword, txtRegPassword, lblRegConfirm, txtRegConfirm, lblRegMethod, cmbRegMethod, btnRegister });

            // Обработчики
            btnLogin.Click += (s, e) =>
            {
                string email = txtLoginEmail.Text.Trim();
                string password = txtLoginPassword.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = false;
                string method = cmbLoginMethod.SelectedItem.ToString();

                switch (method)
                {
                    case "SQL запрос":
                        success = dbService.LoginWithSqlQuery(email, password);
                        break;
                    case "Хранимая процедура":
                        success = dbService.LoginWithStoredProcedure(email, password);
                        break;
                    case "ORM-подход":
                        success = dbService.LoginWithOrmStyle(email, password);
                        break;
                }

                if (success)
                {
                    currentUserId = dbService.GetUserIdByEmail(email);
                    currentUserEmail = email;
                    MessageBox.Show($"Вход выполнен успешно! (Способ: {method})", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowMainApp();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnRegister.Click += (s, e) =>
            {
                string email = txtRegEmail.Text.Trim();
                string password = txtRegPassword.Text;
                string confirm = txtRegConfirm.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!email.Contains("@"))
                {
                    MessageBox.Show("Введите корректный email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = false;
                string method = cmbRegMethod.SelectedItem.ToString();

                switch (method)
                {
                    case "SQL запрос":
                        success = dbService.RegisterWithSqlQuery(email, password);
                        break;
                    case "Хранимая процедура":
                        success = dbService.RegisterWithStoredProcedure(email, password);
                        break;
                    case "ORM-подход":
                        success = dbService.RegisterWithOrmStyle(email, password);
                        break;
                }

                if (success)
                {
                    MessageBox.Show($"Регистрация выполнена успешно! (Способ: {method})\nТеперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtRegEmail.Clear();
                    txtRegPassword.Clear();
                    txtRegConfirm.Clear();
                    authTabControl.SelectedTab = loginTab;
                }
                else
                {
                    MessageBox.Show("Пользователь с таким email уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void ShowMainApp()
        {
            this.Controls.Clear();

            System.Windows.Forms.TabControl mainTabControl = new System.Windows.Forms.TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            this.Controls.Add(mainTabControl);

            TabPage createTab = new TabPage("Создание заявки");
            mainTabControl.TabPages.Add(createTab);
            SetupCreateRequestTab(createTab);

            TabPage viewTab = new TabPage("Мои заявки");
            mainTabControl.TabPages.Add(viewTab);
            SetupViewRequestsTab(viewTab);

            TabPage logoutTab = new TabPage("Выход");
            mainTabControl.TabPages.Add(logoutTab);

            System.Windows.Forms.Button btnLogout = new System.Windows.Forms.Button()
            {
                Text = "Выйти из системы",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point((logoutTab.Width - 200) / 2, 200),
                BackColor = System.Drawing.Color.LightCoral
            };
            btnLogout.Click += (s, e) =>
            {
                currentUserId = 0;
                currentUserEmail = "";
                ShowAuthPanel();
            };
            logoutTab.Controls.Add(btnLogout);

            logoutTab.Resize += (s, e) => { btnLogout.Location = new System.Drawing.Point((logoutTab.Width - 200) / 2, 200); };
        }

        private void SetupCreateRequestTab(TabPage tab)
        {
            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel() { Dock = DockStyle.Fill, AutoScroll = true };
            tab.Controls.Add(panel);

            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 300;
            int leftMargin = 50;

            System.Windows.Forms.Label lblType = new System.Windows.Forms.Label() { Text = "Тип заявки:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.ComboBox cmbType = new System.Windows.Forms.ComboBox()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new object[] { "personal", "group" });
            cmbType.SelectedIndex = 0;
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblType, cmbType });
            yPos += 35;

            System.Windows.Forms.Label lblStartDate = new System.Windows.Forms.Label() { Text = "Дата начала:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            DateTimePicker dtpStartDate = new DateTimePicker()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                MinDate = DateTime.Today
            };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblStartDate, dtpStartDate });
            yPos += 35;

            System.Windows.Forms.Label lblEndDate = new System.Windows.Forms.Label() { Text = "Дата окончания:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            DateTimePicker dtpEndDate = new DateTimePicker()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                MinDate = DateTime.Today
            };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblEndDate, dtpEndDate });
            yPos += 35;

            System.Windows.Forms.Label lblPurpose = new System.Windows.Forms.Label() { Text = "Цель посещения:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.TextBox txtPurpose = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(leftMargin + labelWidth, yPos), Size = new System.Drawing.Size(controlWidth, 25) };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblPurpose, txtPurpose });
            yPos += 35;

            System.Windows.Forms.Label lblDepartment = new System.Windows.Forms.Label() { Text = "Отдел:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.ComboBox cmbDepartment = new System.Windows.Forms.ComboBox()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            List<Department> departments = dbService.GetDepartments();
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.DataSource = departments;
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblDepartment, cmbDepartment });
            yPos += 35;

            System.Windows.Forms.Label lblEmployee = new System.Windows.Forms.Label() { Text = "Сотрудник:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.ComboBox cmbEmployee = new System.Windows.Forms.ComboBox()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblEmployee, cmbEmployee });

            cmbDepartment.SelectedIndexChanged += (s, e) =>
            {
                if (cmbDepartment.SelectedItem != null)
                {
                    int deptId = (int)cmbDepartment.SelectedValue;
                    List<DepartmentEmployee> employees = dbService.GetEmployeesByDepartment(deptId);
                    cmbEmployee.DisplayMember = "FullName";
                    cmbEmployee.ValueMember = "Id";
                    cmbEmployee.DataSource = employees;
                }
            };
            cmbDepartment.SelectedIndex = 0;
            yPos += 35;

            System.Windows.Forms.GroupBox groupBox = new System.Windows.Forms.GroupBox()
            {
                Text = "Информация о посетителях",
                Location = new System.Drawing.Point(leftMargin, yPos + 10),
                Size = new System.Drawing.Size(500, 250)
            };
            panel.Controls.Add(groupBox);

            DataGridView dgvVisitors = new DataGridView()
            {
                Location = new System.Drawing.Point(10, 25),
                Size = new System.Drawing.Size(475, 150),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvVisitors.Columns.Add("LastName", "Фамилия");
            dgvVisitors.Columns.Add("FirstName", "Имя");
            dgvVisitors.Columns.Add("MiddleName", "Отчество");
            dgvVisitors.Columns.Add("Email", "Email");
            dgvVisitors.Columns.Add("PassportSeries", "Серия паспорта");
            dgvVisitors.Columns.Add("PassportNumber", "Номер паспорта");
            groupBox.Controls.Add(dgvVisitors);

            System.Windows.Forms.Button btnAddVisitor = new System.Windows.Forms.Button()
            {
                Text = "Добавить посетителя",
                Location = new System.Drawing.Point(10, 185),
                Size = new System.Drawing.Size(150, 30)
            };
            System.Windows.Forms.Button btnRemoveVisitor = new System.Windows.Forms.Button()
            {
                Text = "Удалить посетителя",
                Location = new System.Drawing.Point(170, 185),
                Size = new System.Drawing.Size(150, 30)
            };
            groupBox.Controls.AddRange(new System.Windows.Forms.Control[] { btnAddVisitor, btnRemoveVisitor });

            List<Visitor> tempVisitors = new List<Visitor>();

            btnAddVisitor.Click += (s, e) =>
            {
                VisitorForm visitorForm = new VisitorForm();
                if (visitorForm.ShowDialog() == DialogResult.OK)
                {
                    tempVisitors.Add(visitorForm.Visitor);
                    dgvVisitors.Rows.Clear();
                    foreach (var v in tempVisitors)
                    {
                        dgvVisitors.Rows.Add(v.LastName, v.FirstName, v.MiddleName, v.Email, v.PassportSeries, v.PassportNumber);
                    }
                }
            };

            btnRemoveVisitor.Click += (s, e) =>
            {
                if (dgvVisitors.SelectedRows.Count > 0)
                {
                    int index = dgvVisitors.SelectedRows[0].Index;
                    tempVisitors.RemoveAt(index);
                    dgvVisitors.Rows.Clear();
                    foreach (var v in tempVisitors)
                    {
                        dgvVisitors.Rows.Add(v.LastName, v.FirstName, v.MiddleName, v.Email, v.PassportSeries, v.PassportNumber);
                    }
                }
            };

            yPos += 270;

            System.Windows.Forms.Button btnSubmit = new System.Windows.Forms.Button()
            {
                Text = "Создать заявку",
                Location = new System.Drawing.Point(leftMargin + 150, yPos),
                Size = new System.Drawing.Size(150, 40),
                BackColor = System.Drawing.Color.LightGreen
            };
            panel.Controls.Add(btnSubmit);

            btnSubmit.Click += (s, e) =>
            {
                if (tempVisitors.Count == 0)
                {
                    MessageBox.Show("Добавьте хотя бы одного посетителя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtPurpose.Text))
                {
                    MessageBox.Show("Укажите цель посещения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dtpStartDate.Value > dtpEndDate.Value)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Request request = new Request
                {
                    UserId = currentUserId,
                    Type = cmbType.SelectedItem.ToString(),
                    StartDate = dtpStartDate.Value,
                    EndDate = dtpEndDate.Value,
                    Purpose = txtPurpose.Text,
                    DepartmentId = (int)cmbDepartment.SelectedValue,
                    EmployeeId = (int)cmbEmployee.SelectedValue
                };

                if (dbService.CreateRequest(request, tempVisitors))
                {
                    MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowMainApp(); // Обновляем отображение
                }
            };
        }

        private void SetupViewRequestsTab(TabPage tab)
        {
            tab.Controls.Clear();

            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel() { Dock = DockStyle.Fill };
            tab.Controls.Add(panel);

            DataGridView dgvRequests = new DataGridView()
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            panel.Controls.Add(dgvRequests);

            List<Request> requests = dbService.GetUserRequests(currentUserId);

            dgvRequests.Columns.Clear();
            dgvRequests.Columns.Add("Id", "№");
            dgvRequests.Columns.Add("Type", "Тип");
            dgvRequests.Columns.Add("StartDate", "Дата начала");
            dgvRequests.Columns.Add("EndDate", "Дата окончания");
            dgvRequests.Columns.Add("Purpose", "Цель");
            dgvRequests.Columns.Add("Department", "Отдел");
            dgvRequests.Columns.Add("Employee", "Сотрудник");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("CreatedAt", "Создана");

            foreach (var req in requests)
            {
                dgvRequests.Rows.Add(
                    req.Id,
                    req.Type == "personal" ? "Личная" : "Групповая",
                    req.StartDate.ToShortDateString(),
                    req.EndDate.ToShortDateString(),
                    req.Purpose,
                    req.DepartmentName,
                    req.EmployeeName,
                    req.StatusName,
                    req.CreatedAt.ToShortDateString()
                );
            }

            dgvRequests.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    int requestId = Convert.ToInt32(dgvRequests.Rows[e.RowIndex].Cells["Id"].Value);
                    List<Visitor> visitors = dbService.GetRequestVisitors(requestId);
                    string visitorInfo = "Посетители:\n\n";
                    for (int i = 0; i < visitors.Count; i++)
                    {
                        var v = visitors[i];
                        visitorInfo += $"{i + 1}. {v.LastName} {v.FirstName} {v.MiddleName}\n";
                        visitorInfo += $"   Email: {v.Email}\n";
                        visitorInfo += $"   Телефон: {v.Phone}\n";
                        visitorInfo += $"   Паспорт: {v.PassportSeries} {v.PassportNumber}\n\n";
                    }
                    MessageBox.Show(visitorInfo, $"Детали заявки №{requestId}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            System.Windows.Forms.Label lblInfo = new System.Windows.Forms.Label()
            {
                Text = "Двойной клик по заявке - просмотр списка посетителей",
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = System.Drawing.Color.LightYellow
            };
            panel.Controls.Add(lblInfo);
        }
    }
}