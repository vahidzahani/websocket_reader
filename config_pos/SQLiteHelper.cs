using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.IO;

namespace config_pos
{
    internal class SQLiteHelper
    {
        private readonly string _connectionString;

        // سازنده کلاس برای تنظیم مسیر دیتابیس
        public SQLiteHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
        }
        public List<Dictionary<string, object>> GetAllTransactions(string tableName)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            string query = $"SELECT * FROM {tableName}";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                Console.WriteLine($"Error retrieving data: {ex.Message}");
            }

            return result;
        }

        // تابع برای درج رکورد در جدول به صورت داینامیک
        public long Insert(string tableName, Dictionary<string, object> columnData)
        {
            // ساختن بخش‌های پویا برای کوئری
            string columns = string.Join(",", columnData.Keys);
            string parameters = string.Join(",", columnData.Keys.Select(key => $"@{key}"));

            // دستور SQL داینامیک برای درج رکورد جدید
            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
            string lastInsertedIdQuery = "SELECT last_insert_rowid()"; // برای دریافت ID آخرین رکورد درج شده

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        // افزودن پارامترهای داینامیک به کوئری
                        foreach (var kvp in columnData)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }

                        // اجرای فرمان
                        int rowsAffected = command.ExecuteNonQuery();

                        // بررسی اینکه آیا درج با موفقیت انجام شد
                        if (rowsAffected > 0)
                        {
                            using (SQLiteCommand lastIdCommand = new SQLiteCommand(lastInsertedIdQuery, connection))
                            {
                                return (long)lastIdCommand.ExecuteScalar();
                            }
                        }
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile("error sqlite", ex.Message);
                return 0;
            }
        }

        // تابع برای جستجو در جدول به صورت داینامیک
        public string Find(string tableName, string searchField, string searchTerm)
        {
            string result = "";
            string query = $"SELECT * FROM {tableName} WHERE {searchField} LIKE @searchTerm";
            //string query = $"SELECT {searchField} FROM {tableName} WHERE {searchField} LIKE @searchTerm";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    result += reader[searchField].ToString() + Environment.NewLine;
                                }
                            }
                            else
                            {
                                result = "هیچ رکوردی یافت نشد.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = "خطا در هنگام جستجو: " + ex.Message;
            }

            return result;
        }
        public long FindRecordId(string tableName, Dictionary<string, object> searchParams)
        {
            long resultId = 0;
            string query = $"SELECT id FROM {tableName} WHERE 1=1";

            // اضافه کردن شروط اختیاری به کوئری
            foreach (var param in searchParams)
            {
                query += $" AND {param.Key} = @{param.Key} ";
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        // افزودن مقادیر به پارامترها از دیکشنری
                        foreach (var param in searchParams)
                        {
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    resultId = Convert.ToInt64(reader["id"]);
                                }
                            }
                            else
                            {
                                resultId = 0; // اگر رکوردی یافت نشد، 0 برمی‌گرداند
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("خطا در هنگام جستجو: " + ex.Message);
                resultId = 0; // در صورت بروز خطا، 0 برمی‌گرداند
            }

            return resultId;
        }


        public int Delete(string tableName, long id)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configpos.db");
            string _connectionString = $"Data Source={dbPath};Version=3;";

            // اگر id برابر با 0 باشد، تمام رکوردها حذف می‌شوند
            string query = id == 0
                ? $"DELETE FROM {tableName}"
                : $"DELETE FROM {tableName} WHERE id=@id";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        // اگر id برابر با 0 نباشد، پارامتر id به کوئری افزوده می‌شود
                        if (id != 0)
                        {
                            command.Parameters.AddWithValue("@id", id);
                        }

                        // اجرای فرمان حذف
                        int rowsAffected = command.ExecuteNonQuery();

                        // بررسی اینکه آیا رکوردی حذف شد
                        return rowsAffected;
                    }
                }
            }
            catch (Exception ex)
            {
                // مدیریت خطاها و گزارش آن
                LogToFile("error sqlite", ex.Message);
                return 0; // در صورت بروز خطا، false برمی‌گرداند
            }
        }


        public long Update(string tableName, long id, Dictionary<string, object> updatedData)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configpos.db");
            //string connectionString = $"Data Source={dbPath};Version=3;";

            // ساختن بخش‌های پویا برای کوئری بروزرسانی
            string setClause = string.Join(",", updatedData.Keys.Select(key => $"{key}=@{key}"));

            // دستور SQL داینامیک برای بروزرسانی رکورد
            string query = $"UPDATE {tableName} SET {setClause} WHERE id=@id";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        // افزودن پارامترهای داینامیک به کوئری برای بروزرسانی
                        foreach (var kvp in updatedData)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }

                        // افزودن پارامتر id به کوئری
                        command.Parameters.AddWithValue("@id", id);

                        // اجرای فرمان بروزرسانی
                        int rowsAffected = command.ExecuteNonQuery();

                        // بررسی اینکه آیا بروزرسانی با موفقیت انجام شد
                        if (rowsAffected > 0)
                        {
                            return id; // بازگشت ID رکورد بروزرسانی شده
                        }
                        else
                        {
                            return 0; // هیچ رکوردی آپدیت نشد
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // مدیریت خطاها و گزارش آن
                LogToFile("error sqlite", ex.Message);
                return 0;
            }
        }


        // تابع برای لاگ کردن خطاها (در اینجا صرفاً به عنوان نمونه آورده شده است)
        private void LogToFile(string title, string message)
        {
            // این تابع می‌تواند برای لاگ کردن خطاها در فایل یا هر جا که نیاز است استفاده شود
            Console.WriteLine($"{title}: {message}");
        }
    }

}
