using System.Data;
using APBD_RESTAPI2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace APBD_RESTAPI2.Controller;

[ApiController]

[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAnimals([FromQuery] string orderBy = "name")
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        string columnToOrderBy = orderBy.ToLower() switch
        {
            "description" => "Description",
            "name" => "Name",
            "category" => "Category",
            "area" => "Area",
            _ => "Name"
        };

        SqlCommand query = new SqlCommand();
        query.Connection = connection;
        query.CommandText = $"SELECT * FROM Animal ORDER BY {columnToOrderBy}";

        var reader = query.ExecuteReader();

        List<Animal> animals = new List<Animal>();

        int IdAnimalOrdinal = reader.GetOrdinal("IdAnimal");
        int NameOrdinal = reader.GetOrdinal("Name");
        int DescriptionOrdinal = reader.GetOrdinal("Description");
        int CategoryOrdinal = reader.GetOrdinal("Category");
        int AreaOrdinal = reader.GetOrdinal("Area");
        
        while (reader.Read())
        {
            animals.Add(new Animal()
            {
                IdAnimal = reader.GetInt32(IdAnimalOrdinal),
                Name = reader.GetString(NameOrdinal),
                Description = reader.GetString(DescriptionOrdinal),
                Category = reader.GetString(CategoryOrdinal),
                Area = reader.GetString(AreaOrdinal)
            });
        }
        
        return Ok(animals);
    }

    [HttpPost]
    public IActionResult AddAnimal([FromBody] Animal newAnimal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        SqlCommand InsertQuery =
            new SqlCommand(
                "INSERT INTO Animal (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area);",
                connection);

        InsertQuery.Parameters.Add("@Name", SqlDbType.NVarChar).Value = newAnimal.Name;
        InsertQuery.Parameters.Add("@Description", SqlDbType.NVarChar).Value = newAnimal.Description;
        InsertQuery.Parameters.Add("@Category", SqlDbType.NVarChar).Value = newAnimal.Category;
        InsertQuery.Parameters.Add("@Area", SqlDbType.NVarChar).Value = newAnimal.Area;

        int result = InsertQuery.ExecuteNonQuery();

        if (result > 0)
        {
            return Created();
        }
        else
        {
            return StatusCode(500);
        }
    }
}