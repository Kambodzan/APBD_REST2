using System.Data;
using APBD_RESTAPI2.Models;
using APBD_RESTAPI2.Models.DTOs;
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
    public IActionResult AddAnimal([FromBody] AddAnimal addAnimal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        SqlCommand InsertQuery =
            new SqlCommand(
                "INSERT INTO Animal (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area);",
                connection);

        InsertQuery.Parameters.AddWithValue("@Name", addAnimal.Name);
        InsertQuery.Parameters.AddWithValue("@Description", addAnimal.Description ?? (object)DBNull.Value);
        InsertQuery.Parameters.AddWithValue("@Category", addAnimal.Category);
        InsertQuery.Parameters.AddWithValue("@Area", addAnimal.Area);

        int result = InsertQuery.ExecuteNonQuery();

        return Created();
    }

    [HttpPut("{idAnimal}")]
    public IActionResult UpdateAnimal(int idAnimal, [FromBody] UpdateAnimal updateAnimal)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        SqlCommand updateQuery = new SqlCommand(
            "UPDATE Animal SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal",
            connection);
        
        updateQuery.Parameters.AddWithValue("@Name", updateAnimal.Name);
        updateQuery.Parameters.AddWithValue("@Description", updateAnimal.Description ?? (object)DBNull.Value);
        updateQuery.Parameters.AddWithValue("@Category", updateAnimal.Category);
        updateQuery.Parameters.AddWithValue("@Area", updateAnimal.Area);
        updateQuery.Parameters.AddWithValue("@IdAnimal", SqlDbType.Int).Value = idAnimal;

        updateQuery.ExecuteReader();

        return Ok();

    }

    [HttpDelete("{idAnimal}")]
    public IActionResult RemoveAnimal(int idAnimal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        SqlCommand deleteQuery = new SqlCommand(
            "DELETE FROM Animal WHERE IdAnimal = @IdAnimal",
            connection);
        
        deleteQuery.Parameters.AddWithValue("@IdAnimal", SqlDbType.Int).Value = idAnimal;

        deleteQuery.ExecuteNonQuery();

        return Ok();
    }
}