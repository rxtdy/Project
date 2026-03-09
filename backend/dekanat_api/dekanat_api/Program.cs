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

                var query = "SELECT * FROM Все_Группы WHERE 1=1";
                var command = new SqlCommand();
                command.Connection = connection;

                if (!string.IsNullOrEmpty(факультет))
                {
                    query += " AND Код_Факультета = @Факультет";
                    command.Parameters.AddWithValue("@Факультет", факультет);
                }

                if (!string.IsNullOrEmpty(формаОбучения))
                {
                    query += " AND Форма_Обучения = @ФормаОбучения";
                    command.Parameters.AddWithValue("@ФормаОбучения", формаОбучения);
                }

                if (!string.IsNullOrEmpty(уровеньОбучения))
                {
                    query += " AND Уровень = @УровеньОбучения";
                    command.Parameters.AddWithValue("@УровеньОбучения", уровеньОбучения);
                }

                if (!string.IsNullOrEmpty(учебныйГод))
                {
                    query += " AND УчебныйГод = @УчебныйГод";
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

                    query += $" AND Курс IN ({string.Join(",", courseParams)})";

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
                        НазваниеГруппы = reader["Название"].ToString(),
                        Факультет = reader["Код_Факультета"].ToString() switch
                        {
                            "1" => "Факультет информатики", 
                            "2" => "Факультет экономики",
                            "3" => "Факультет медицины",
                            "4" => "Факультет гуманитарных наук",
                            "5" => "Факультет физики",
                            _ => ""
                        },
                        Специальность = reader["Специальность"].ToString(),
                        Курс = Convert.ToInt32(reader["Курс"]),
                        ФормаОбучения = reader["Форма_Обучения"].ToString() switch
                        {
                            "1" => "Очная форма", 
                            "2" => "Заочная форма",
                            "3" => "Очно-заочная форма",
                            _ => ""
                        },
                        УровеньОбучения = reader["Уровень"].ToString() switch
                        {
                            "1" => "Специалитет",
                            "2" => "Бакалавриат",
                            "3" => "Магистратура",
                            "4" => "Аспирантура",
                            "5" => "Ординатура",
                            _ => ""
                        },
                        УчебныйГод = reader["УчебныйГод"].ToString()
                    });
                }

                return Results.Json(groups);
            });

            app.Run();
        }
    }
}
