namespace Droplet.CommandLine.Commands;

public class ViewCommand : CommandBase
{
    public string Type { get; set; } = "angular";
    public string EntityPath { get; set; } = default!;
    public string DtoPath { get; set; } = default!;
    public string OutputPath { get; set; } = default!;

    public NgPageGenerate Gen { get; set; }

    public ViewCommand(string entityPath, string dtoPath, string outputPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
        OutputPath = outputPath;
        Instructions.Add($"  🔹 generate module,routing and menu.");
        Instructions.Add($"  🔹 generate pages.");

        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }
        var entityName = Path.GetFileNameWithoutExtension(entityPath);
        Gen = new NgPageGenerate(entityName, dtoPath, outputPath);
    }
    public void Run()
    {
        Console.WriteLine(Instructions[0]);
        GenerateMenu();
        GenerateModuleWithRouting();
        Console.WriteLine(Instructions[1]);
        GeneratePages();
    }

    public void GenerateMenu()
    {

    }

    public void GenerateModuleWithRouting()
    {

    }

    public void GeneratePages()
    {

    }
}
