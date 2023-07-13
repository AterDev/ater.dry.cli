using Microsoft.CodeAnalysis.MSBuild;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 解决方案解析帮助类
/// </summary>
public class SolutionHelper : IDisposable
{
    public MSBuildWorkspace Workspace { get; set; }
    public Solution Solution { get; set; }

    public SolutionHelper(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("解决方案文件不存在");
        }

        Workspace = MSBuildWorkspace.Create();
        Solution = Workspace.OpenSolutionAsync(path).Result;
    }



    public void Dispose()
    {
        Workspace.Dispose();
    }
}
