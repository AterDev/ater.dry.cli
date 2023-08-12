using System.CommandLine;
using System.Text;

namespace CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        ShowLogo();
        if (args.Length == 0)
        {
            return 0;
        }

        RootCommand root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }

    private static void ShowLogo()
    {
        var logo = """
             _____    _____   __     __
            |  __ \  |  __ \  \ \   / /
            | |  | | | |__) |  \ \_/ / 
            | |  | | |  _  /    \   /  
            | |__| | | | \ \     | |   
            |_____/  |_|  \_\    |_|

                   -- for freedom 🗽 --

            """;

        Console.WriteLine(logo);
    }
}
