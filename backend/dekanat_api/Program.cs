using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dekanat_api
{
    public record StudentUpdateDto(int Id, string Fio);

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors();
            var app = builder.Build();

            app.UseCors(policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod()
            );

            var configuration = app.Configuration;

            app.MapGet("/groups", (
                [FromQuery] string? факультет,
                [FromQuery] string? формаОбучения,
                [FromQuery] string? уровеньОбучения,
                [FromQuery] string? учебныйГод,
                [FromQuery] string? курсы,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var baseWhereClause = " WHERE Все_Группы.Удалена = 0 ";
                var command = new SqlCommand { Connection = connection };

                if (!string.IsNullOrEmpty(факультет))
                {
                    baseWhereClause += " AND Все_Группы.Код_Факультета = @Факультет";
                    command.Parameters.AddWithValue("@Факультет", факультет);
                }
                if (!string.IsNullOrEmpty(формаОбучения))
                {
                    baseWhereClause += " AND Все_Группы.Форма_Обучения = @ФормаОбучения";
                    command.Parameters.AddWithValue("@ФормаОбучения", формаОбучения);
                }
                if (!string.IsNullOrEmpty(уровеньОбучения))
                {
                    baseWhereClause += " AND Все_Группы.Уровень = @УровеньОбучения";
                    command.Parameters.AddWithValue("@УровеньОбучения", уровеньОбучения);
                }
                if (!string.IsNullOrEmpty(учебныйГод))
                {
                    baseWhereClause += " AND Все_Группы.УчебныйГод = @УчебныйГод";
                    command.Parameters.AddWithValue("@УчебныйГод", учебныйГод);
                }

                int[]? coursesArray = null;
                if (!string.IsNullOrEmpty(курсы))
                {
                    coursesArray = курсы.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    if (coursesArray.Length > 0)
                    {
                        var courseParams = coursesArray.Select((c, i) => $"@Курс{i}").ToList();
                        baseWhereClause += $" AND Все_Группы.Курс IN ({string.Join(",", courseParams)})";
                        for (int i = 0; i < coursesArray.Length; i++)
                            command.Parameters.AddWithValue($"@Курс{i}", coursesArray[i]);
                    }
                }

                command.CommandText = $"SELECT COUNT(*) FROM Все_Группы {baseWhereClause}";
                int totalCount = (int)command.ExecuteScalar();

                var query = $@"
                    SELECT 
                        Все_Группы.Код as GroupId,
                        Все_Группы.Название as НазваниеГруппы,
                        Факультеты.Факультет as Факультет,
                        Все_Группы.Специальность,
                        Все_Группы.Курс,
                        ФормаОбучения.ФормаОбучения as ФормаОбучения,
                        Уровень_образования.Уровень as УровеньОбучения,
                        Все_Группы.УчебныйГод
                    FROM Все_Группы 
                    LEFT JOIN Факультеты ON Факультеты.Код = Все_Группы.Код_Факультета
                    LEFT JOIN ФормаОбучения ON ФормаОбучения.Код = Все_Группы.Форма_Обучения
                    LEFT JOIN Уровень_образования ON Уровень_образования.Код_записи = Все_Группы.Уровень
                    {baseWhereClause}
                    ORDER BY Все_Группы.Код DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.CommandText = query;

                var groups = new List<object>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    groups.Add(new
                    {
                        GroupId = Convert.ToInt32(reader["GroupId"]),
                        НазваниеГруппы = reader["НазваниеГруппы"].ToString(),
                        Факультет = reader["Факультет"].ToString(),
                        Специальность = reader["Специальность"].ToString(),
                        Курс = Convert.ToInt32(reader["Курс"]),
                        ФормаОбучения = reader["ФормаОбучения"].ToString(),
                        УровеньОбучения = reader["УровеньОбучения"].ToString(),
                        УчебныйГод = reader["УчебныйГод"].ToString()
                    });
                }

                return Results.Json(new { Total = totalCount, Items = groups });
            });

            
            app.MapGet("/students", (
                [FromQuery] int groupId,
                [FromQuery] string? пол,
                [FromQuery] int? статус,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var baseWhereClause = " WHERE Код_Группы = @GroupId ";
                var command = new SqlCommand { Connection = connection };
                command.Parameters.AddWithValue("@GroupId", groupId);

                if (!string.IsNullOrEmpty(пол))
                {
                    baseWhereClause += " AND Пол = @Пол";
                    command.Parameters.AddWithValue("@Пол", пол);
                }

                if (статус.HasValue)
                {
                    baseWhereClause += " AND Статус = @Статус";
                    command.Parameters.AddWithValue("@Статус", статус.Value);
                }

                command.CommandText = $"SELECT COUNT(*) FROM Студенты {baseWhereClause}";
                int totalCount = (int)command.ExecuteScalar();

                var query = $@"
                    SELECT ID, ФИО, Пол, Дата_Рождения, Номер_Зачетки, Средний_Балл, Статус 
                    FROM Студенты
                    {baseWhereClause}
                    ORDER BY ФИО
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.CommandText = query;

                var students = new List<object>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new
                    {
                        Id = Convert.ToInt32(reader["ID"]),
                        Fio = reader["ФИО"].ToString(),
                        Gender = reader["Пол"].ToString(),
                        RecordBook = reader["Номер_Зачетки"].ToString(),
                        Gpa = Convert.ToDecimal(reader["Средний_Балл"]),
                        Status = Convert.ToInt32(reader["Статус"])
                    });
                }

                return Results.Json(new { Total = totalCount, Items = students });
            });

            app.MapPut("/students", async (List<StudentUpdateDto> updates, IConfiguration config) =>
            {
                if (updates == null || updates.Count == 0) return Results.Ok();

                var connectionString = config.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();
                try
                {
                    var command = new SqlCommand
                    {
                        Connection = connection,
                        Transaction = transaction,
                        CommandText = "UPDATE Студенты SET ФИО = @Fio WHERE ID = @Id"
                    };

                    var idParam = command.Parameters.Add("@Id", System.Data.SqlDbType.Int);
                    var fioParam = command.Parameters.Add("@Fio", System.Data.SqlDbType.NVarChar, 150);

                    foreach (var update in updates)
                    {
                        idParam.Value = update.Id;
                        fioParam.Value = update.Fio;
                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                    return Results.Ok(new { message = "Данные успешно сохранены" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/filters", () =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                return Results.Json(new
                {
                    Faculties = GetFaculties(connection),
                    Forms = GetForms(connection),
                    Levels = GetLevels(connection),
                    Courses = GetCourses(connection),
                    Years = GetYears(connection)
                });
            });

            app.Run();
        }

        static List<object> GetFaculties(SqlConnection connection)
        {
            var command = new SqlCommand("SELECT Код, Факультет FROM Факультеты ORDER BY Факультет", connection);
            var result = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) result.Add(new { value = Convert.ToInt32(reader["Код"]), text = reader["Факультет"].ToString() });
            return result;
        }

        static List<object> GetForms(SqlConnection connection)
        {
            var command = new SqlCommand("SELECT Код, ФормаОбучения FROM ФормаОбучения ORDER BY Код", connection);
            var result = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) result.Add(new { value = Convert.ToInt32(reader["Код"]), text = reader["ФормаОбучения"].ToString() });
            return result;
        }

        static List<object> GetLevels(SqlConnection connection)
        {
            var command = new SqlCommand("SELECT Код_записи, Уровень FROM Уровень_образования ORDER BY Код_записи", connection);
            var result = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) result.Add(new { value = Convert.ToInt32(reader["Код_записи"]), text = reader["Уровень"].ToString() });
            return result;
        }

        static List<object> GetCourses(SqlConnection connection)
        {
            var command = new SqlCommand("SELECT DISTINCT Курс FROM Все_Группы WHERE Курс IS NOT NULL ORDER BY Курс", connection);
            var result = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) result.Add(new { value = Convert.ToInt32(reader["Курс"]), text = $"{reader["Курс"]} курс" });
            return result;
        }

        static List<object> GetYears(SqlConnection connection)
        {
            var command = new SqlCommand("SELECT DISTINCT УчебныйГод FROM Все_Группы WHERE УчебныйГод IS NOT NULL ORDER BY УчебныйГод DESC", connection);
            var result = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) result.Add(new { value = reader["УчебныйГод"].ToString(), text = reader["УчебныйГод"].ToString() });
            return result;
        }
    }
}