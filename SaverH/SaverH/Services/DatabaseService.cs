using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SaverH.Models;

namespace SaverH.Services
{
    public class DatabaseService
    {
        private string connectionString = "Host=127.0.0.1;Database=hranitelpro;Username=postgres;Password=1";

        public static string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // ============ 1. СПОСОБ: Обычный SQL запрос для авторизации ============
        public bool LoginWithSqlQuery(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            string query = "SELECT COUNT(*) FROM users WHERE email = @email AND password_hash = @password";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // ============ 2. СПОСОБ: Хранимая процедура для авторизации ============
        public bool LoginWithStoredProcedure(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("sp_login_user", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_email", email);
                    cmd.Parameters.AddWithValue("@p_password_hash", hashedPassword);
                    return (bool)cmd.ExecuteScalar();
                }
            }
        }

        // ============ 3. СПОСОБ: ORM-подобный подход для авторизации ============
        public User GetUserByEmail(string email)
        {
            string query = "SELECT * FROM users WHERE email = @email";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                RegisteredAt = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool LoginWithOrmStyle(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            User user = GetUserByEmail(email);
            return user != null && user.PasswordHash == hashedPassword;
        }

        // ============ 1. СПОСОБ: Обычный SQL запрос для регистрации ============
        public bool RegisterWithSqlQuery(string email, string password)
        {
            string hashedPassword = HashPassword(password);

            string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @email";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", email);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (exists > 0) return false;
                }

                string insertQuery = "INSERT INTO users (email, password_hash, registered_at) VALUES (@email, @password, NOW())";
                using (NpgsqlCommand insertCmd = new NpgsqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@email", email);
                    insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                    return insertCmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ============ 2. СПОСОБ: Хранимая процедура для регистрации ============
        public bool RegisterWithStoredProcedure(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("sp_register_user", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_email", email);
                    cmd.Parameters.AddWithValue("@p_password_hash", hashedPassword);
                    return (bool)cmd.ExecuteScalar();
                }
            }
        }

        // ============ 3. СПОСОБ: ORM-подобный подход для регистрации ============
        public bool RegisterWithOrmStyle(string email, string password)
        {
            if (GetUserByEmail(email) != null) return false;

            string hashedPassword = HashPassword(password);
            string insertQuery = "INSERT INTO users (email, password_hash, registered_at) VALUES (@email, @password, NOW())";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public User GetUserById(int userId)
        {
            string query = "SELECT * FROM users WHERE id = @id";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                RegisteredAt = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int GetUserIdByEmail(string email)
        {
            string query = "SELECT id FROM users WHERE email = @email";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public List<Department> GetDepartments()
        {
            List<Department> departments = new List<Department>();
            string query = "SELECT id, name FROM departments ORDER BY id";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new Department
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return departments;
        }

        public List<DepartmentEmployee> GetEmployeesByDepartment(int departmentId)
        {
            List<DepartmentEmployee> employees = new List<DepartmentEmployee>();
            string query = "SELECT id, full_name, department_id FROM department_employees WHERE department_id = @deptId ORDER BY full_name";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new DepartmentEmployee
                            {
                                Id = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                DepartmentId = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return employees;
        }

        public Dictionary<int, string> GetStatuses()
        {
            Dictionary<int, string> statuses = new Dictionary<int, string>();
            string query = "SELECT id, name FROM statuses";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(reader.GetInt32(0), reader.GetString(1));
                        }
                    }
                }
            }
            return statuses;
        }

        // Создание заявки
        public bool CreateRequest(Request request, List<Visitor> visitors)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string requestQuery = @"
                            INSERT INTO requests (user_id, type, start_date, end_date, purpose, department_id, employee_id, status_id, created_at)
                            VALUES (@userId, @type, @startDate, @endDate, @purpose, @deptId, @empId, 1, NOW())
                            RETURNING id";

                        int requestId;
                        using (NpgsqlCommand cmd = new NpgsqlCommand(requestQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", request.UserId);
                            cmd.Parameters.AddWithValue("@type", request.Type);
                            cmd.Parameters.AddWithValue("@startDate", request.StartDate);
                            cmd.Parameters.AddWithValue("@endDate", request.EndDate);
                            cmd.Parameters.AddWithValue("@purpose", request.Purpose);
                            cmd.Parameters.AddWithValue("@deptId", request.DepartmentId);
                            cmd.Parameters.AddWithValue("@empId", request.EmployeeId);
                            requestId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        for (int i = 0; i < visitors.Count; i++)
                        {
                            Visitor visitor = visitors[i];

                            string visitorQuery = @"
                                INSERT INTO visitors (last_name, first_name, middle_name, phone, email, organization, note, birth_date, passport_series, passport_number, passport_scan_path)
                                VALUES (@lastName, @firstName, @middleName, @phone, @email, @organization, @note, @birthDate, @passportSeries, @passportNumber, @scanPath)
                                RETURNING id";

                            int visitorId;
                            using (NpgsqlCommand cmd = new NpgsqlCommand(visitorQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@lastName", visitor.LastName);
                                cmd.Parameters.AddWithValue("@firstName", visitor.FirstName);
                                cmd.Parameters.AddWithValue("@middleName", (object)visitor.MiddleName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@phone", (object)visitor.Phone ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@email", visitor.Email);
                                cmd.Parameters.AddWithValue("@organization", (object)visitor.Organization ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@note", visitor.Note ?? "");
                                cmd.Parameters.AddWithValue("@birthDate", visitor.BirthDate);
                                cmd.Parameters.AddWithValue("@passportSeries", visitor.PassportSeries);
                                cmd.Parameters.AddWithValue("@passportNumber", visitor.PassportNumber);
                                cmd.Parameters.AddWithValue("@scanPath", (object)visitor.PassportScanPath ?? DBNull.Value);
                                visitorId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            string relationQuery = @"
                                INSERT INTO request_visitors (request_id, visitor_id, order_number)
                                VALUES (@requestId, @visitorId, @orderNumber)";

                            using (NpgsqlCommand cmd = new NpgsqlCommand(relationQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@requestId", requestId);
                                cmd.Parameters.AddWithValue("@visitorId", visitorId);
                                cmd.Parameters.AddWithValue("@orderNumber", i + 1);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при создании заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }

        // Получение заявок пользователя
        public List<Request> GetUserRequests(int userId)
        {
            List<Request> requests = new List<Request>();
            string query = @"
                SELECT r.*, s.name as status_name, d.name as department_name, de.full_name as employee_name
                FROM requests r
                LEFT JOIN statuses s ON r.status_id = s.id
                LEFT JOIN departments d ON r.department_id = d.id
                LEFT JOIN department_employees de ON r.employee_id = de.id
                WHERE r.user_id = @userId
                ORDER BY r.created_at DESC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Request request = new Request
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Type = reader.GetString(2),
                                StartDate = reader.GetDateTime(3),
                                EndDate = reader.GetDateTime(4),
                                Purpose = reader.GetString(5),
                                DepartmentId = reader.GetInt32(6),
                                EmployeeId = reader.GetInt32(7),
                                StatusId = reader.GetInt32(8),
                                RejectionReason = reader.IsDBNull(9) ? null : reader.GetString(9),
                                CreatedAt = reader.GetDateTime(10),
                                StatusName = reader.GetString(11),
                                DepartmentName = reader.GetString(12),
                                EmployeeName = reader.GetString(13)
                            };
                            requests.Add(request);
                        }
                    }
                }
            }
            return requests;
        }

        // Получение посетителей заявки
        public List<Visitor> GetRequestVisitors(int requestId)
        {
            List<Visitor> visitors = new List<Visitor>();
            string query = @"
                SELECT v.*
                FROM visitors v
                JOIN request_visitors rv ON v.id = rv.visitor_id
                WHERE rv.request_id = @requestId
                ORDER BY rv.order_number";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Visitor visitor = new Visitor
                            {
                                Id = reader.GetInt32(0),
                                LastName = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.GetString(5),
                                Organization = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Note = reader.IsDBNull(7) ? null : reader.GetString(7),
                                BirthDate = reader.GetDateTime(8),
                                PassportSeries = reader.GetString(9),
                                PassportNumber = reader.GetString(10),
                                PhotoPath = reader.IsDBNull(11) ? null : reader.GetString(11),
                                PassportScanPath = reader.IsDBNull(12) ? null : reader.GetString(12)
                            };
                            visitors.Add(visitor);
                        }
                    }
                }
            }
            return visitors;
        }
    }
}