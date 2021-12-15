namespace Droplet.CommandLine;

public class Test
{
    /// <summary>
    /// dto 生成测试
    /// </summary>
    /// <param name="entityPath"></param>
    public void TestDtoGen(string entityPath, string dtoPath)
    {
        var gen = new DtoGenerate(entityPath, dtoPath);
        gen.GenerateDtos();
    }


    public async Task TestNgGenAsync(string url, string output)
    {
        var cmd = new RootCommands();
        await cmd.GenerateNgAsync(url, output);
    }
}
