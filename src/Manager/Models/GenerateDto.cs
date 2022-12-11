namespace Manager.Models;

public class GenerateDto
{
    public required int ProjectId { get; set; }
    public required string EntityPath { get; set; }
    public CommandType CommandType { get; set; }
}

public enum CommandType
{
    Dto,
    Manager,
    API
}
