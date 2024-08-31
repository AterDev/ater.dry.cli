using System.Diagnostics.CodeAnalysis;

namespace Share;
public class PropertyEquality : IEqualityComparer<PropertyInfo>
{
    public bool Equals(PropertyInfo? x, PropertyInfo? y)
    {
        bool res = x?.Name == y?.Name
            && x?.Type == y?.Type
            && x?.IsRequired == y?.IsRequired
            && x?.IsNullable == y?.IsNullable
            && x?.CommentSummary == y?.CommentSummary
            && x?.AttributeText == y?.AttributeText;

        if (!res)
        {
            Console.WriteLine($"origin:{x?.Name},{x?.Type},{x?.IsRequired},{x?.IsNullable},{x?.CommentSummary},{x?.AttributeText}");
            Console.WriteLine($"current:{y?.Name},{y?.Type},{y?.IsRequired},{y?.IsNullable},{y?.CommentSummary},{x?.AttributeText}");
        }
        return res;
    }

    public int GetHashCode([DisallowNull] PropertyInfo obj)
    {
        return (obj.CommentSummary + obj.AttributeText + obj.Name + obj.Type + obj.IsRequired + obj.IsNullable).GetHashCode();
    }
}
