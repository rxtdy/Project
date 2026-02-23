using Microsoft.Data.SqlClient;

namespace InternshipProjectReal
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

            app.MapGet("/groups", (IConfiguration config) =>
            {
                string connectionString = config.GetConnectionString("DefaultConnection");

                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                string sql = "SELECT * FROM Все_Группы";

                using SqlCommand command = new SqlCommand(sql, connection);

                using SqlDataReader reader = command.ExecuteReader();

                var groups = new List<object>();

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
