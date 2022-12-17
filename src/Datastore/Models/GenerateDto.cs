namespace Datastore.Models;

public class GenerateDto
{
    public required string ProjectId { get; set; }
    public required string EntityPath { get; set; }
    public CommandType CommandType { get; set; }
}

public enum CommandType
{
    Dto,
    Manager,
    API
}
