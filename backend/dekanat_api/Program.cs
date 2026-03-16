using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;

namespace dekanat_api
{
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

            app.MapGet("/groups",
                (
                [FromQuery] string? факультет,
                [FromQuery] string? формаОбучения,
                [FromQuery] string? уровеньОбучения,
                [FromQuery] string? учебныйГод,
                [FromQuery] string? курсы
                ) =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    using var connection = new SqlConnection(connectionString);
                    connection.Open();

                    var query = @"
                    SELECT 
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
                    WHERE 1=1";

                    var command = new SqlCommand();
                    command.Connection = connection;

                    if (!string.IsNullOrEmpty(факультет))
                    {
                        query += " AND Все_Группы.Код_Факультета = @Факультет";
                        command.Parameters.AddWithValue("@Факультет", факультет);
                    }

                    if (!string.IsNullOrEmpty(формаОбучения))
                    {
                        query += " AND Все_Группы.Форма_Обучения = @ФормаОбучения";
                        command.Parameters.AddWithValue("@ФормаОбучения", формаОбучения);
                    }

                    if (!string.IsNullOrEmpty(уровеньОбучения))
                    {
                        query += " AND Все_Группы.Уровень = @УровеньОбучения";
                        command.Parameters.AddWithValue("@УровеньОбучения", уровеньОбучения);
                    }

                    if (!string.IsNullOrEmpty(учебныйГод))
                    {
                        query += " AND Все_Группы.УчебныйГод = @УчебныйГод";
                        command.Parameters.AddWithValue("@УчебныйГод", учебныйГод);
                    }

                    int[]? courses = null;
                    if (!string.IsNullOrEmpty(курсы))
                    {
                        courses = курсы.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse)
                                 .ToArray();
                    }

                    if (courses != null && courses.Length > 0)
                    {
                        var courseParams = courses.Select((c, i) => $"@Курс{i}").ToList();
                        query += $" AND Все_Группы.Курс IN ({string.Join(",", courseParams)})";

                        for (int i = 0; i < courses.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@Курс{i}", courses[i]);
                        }
                    }

                    command.CommandText = query;

                    var groups = new List<object>();

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        groups.Add(new
                        {
                            НазваниеГруппы = reader["НазваниеГруппы"].ToString(),
                            Факультет = reader["Факультет"].ToString(),
                            Специальность = reader["Специальность"].ToString(),
                            Курс = Convert.ToInt32(reader["Курс"]),
                            ФормаОбучения = reader["ФормаОбучения"].ToString(),
                            УровеньОбучения = reader["УровеньОбучения"].ToString(),
                            УчебныйГод = reader["УчебныйГод"].ToString()
                        });
                    }

                    return Results.Json(groups);
                });

            app.MapGet("/filters", () =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var result = new
                {
                    Faculties = GetFaculties(connection),
                    Forms = GetForms(connection),
                    Levels = GetLevels(connection),
                    Courses = GetCourses(connection),
                    Years = GetYears(connection)
                };

                return Results.Json(result);
            });

            app.Run();
        }

        static List<object> GetFaculties(SqlConnection connection)
        {
            var query = "SELECT Код, Факультет FROM Факультеты ORDER BY Факультет";
            var command = new SqlCommand(query, connection);

            var faculties = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                faculties.Add(new
                {
                    value = Convert.ToInt32(reader["Код"]),
                    text = reader["Факультет"].ToString()
                });
            }
            return faculties;
        }

        static List<object> GetForms(SqlConnection connection)
        {
            var query = "SELECT Код, ФормаОбучения FROM ФормаОбучения ORDER BY Код";
            var command = new SqlCommand(query, connection);

            var forms = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                forms.Add(new
                {
                    value = Convert.ToInt32(reader["Код"]),
                    text = reader["ФормаОбучения"].ToString()
                });
            }
            return forms;
        }

        static List<object> GetLevels(SqlConnection connection)
        {
            var query = "SELECT Код_записи, Уровень FROM Уровень_образования ORDER BY Код_записи";
            var command = new SqlCommand(query, connection);

            var levels = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                levels.Add(new
                {
                    value = Convert.ToInt32(reader["Код_записи"]),
                    text = reader["Уровень"].ToString()
                });
            }
            return levels;
        }

        static List<object> GetCourses(SqlConnection connection)
        {
            var query = "SELECT DISTINCT Курс FROM Все_Группы WHERE Курс IS NOT NULL ORDER BY Курс";
            var command = new SqlCommand(query, connection);

            var courses = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new
                {
                    value = Convert.ToInt32(reader["Курс"]),
                    text = $"{reader["Курс"]} курс"
                });
            }
            return courses;
        }

        static List<object> GetYears(SqlConnection connection)
        {
            var query = "SELECT DISTINCT УчебныйГод FROM Все_Группы WHERE УчебныйГод IS NOT NULL ORDER BY УчебныйГод DESC";
            var command = new SqlCommand(query, connection);

            var years = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                years.Add(new
                {
                    value = reader["УчебныйГод"].ToString(),
                    text = reader["УчебныйГод"].ToString()
                });
            }
            return years;
        }

    }
}

