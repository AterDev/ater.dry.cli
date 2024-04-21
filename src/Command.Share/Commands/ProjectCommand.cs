namespace Command.Share.Commands;

/// <summary>
/// 项目管理命令
/// </summary>
public class ProjectCommand
{
    public static void CreateService(string solutionPath, string serviceName)
    {
        string projectName = serviceName.ToPascalCase();
        var path = Path.Combine(solutionPath, Config.MicroservicePath, projectName);

        var studioPath = AssemblyHelper.GetStudioPath();

        var sourcePath = Path.Combine(studioPath, "Microservice");
        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine("🦘 Microservice template not found!");
            return;
        }

        if (Directory.Exists(path))
        {
            Console.WriteLine("🦘 Service already exists!");
            return;
        }
        else
        {
            Console.WriteLine("⛏️ Creating service...");
            Directory.CreateDirectory(path);
            IOHelper.CopyDirectory(sourcePath, path);
            // 替换名称
            IOHelper.ReplaceTemplate(path, "StandaloneService", projectName);
            Console.WriteLine("⛏️ Add to solution");
            // 添加到解决方案
            if (ProcessHelper.RunCommand("dotnet", $"sln {solutionPath} add {path}/{projectName}.csproj", out string error))
            {
                Console.WriteLine(error);
            }
            Console.WriteLine("🎊 Service created!");
        }
    }
}
