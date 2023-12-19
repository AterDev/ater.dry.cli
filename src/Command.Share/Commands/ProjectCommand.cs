
namespace Command.Share.Commands;

/// <summary>
/// 项目管理命令
/// </summary>
public class ProjectCommand
{
    public static void CreateService(string serviceName)
    {
        var path = Path.Combine(Config.MicroservicePath, serviceName.ToPascalCase());

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
            Console.WriteLine("🦘 Creating service...");
            Directory.CreateDirectory(path);
            IOHelper.CopyDirectory(sourcePath, path);
            // 替换名称
            IOHelper.ReplaceTemplate(path, "StandaloneService", serviceName);
            Console.WriteLine("🦘 Service created!");
        }
    }
}
