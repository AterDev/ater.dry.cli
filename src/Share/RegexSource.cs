using System.Text.RegularExpressions;

namespace Share;
public partial class RegexSource
{
    [GeneratedRegex(@"/// <summary>([\s\S]*?)/// <\/summary>")]
    public static partial Regex SummaryCommentRegex();
}
