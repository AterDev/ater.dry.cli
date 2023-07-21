using Command.Share;

namespace CommandLine.Test;
public class FunctionTest
{


    [Fact]
    public void Should_Analysis_Program()
    {
        var path = @"D:\codes\v7.0\src\Http.API";

        UpdateManager.UpdateProgram(path);
    }
}
