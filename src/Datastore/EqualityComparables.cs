using System.Diagnostics.CodeAnalysis;

namespace Datastore;
internal class EqualityComparables
{

}

public class PropertyEquality : IEqualityComparer<PropertyInfo>
{
    public bool Equals(PropertyInfo? x, PropertyInfo? y)
    {
        return x?.Name == y?.Name
            && x?.Type == y?.Type
            && x?.IsRequired == y?.IsRequired
            && x?.IsNullable == y?.IsNullable
            && x?.CommentXml == y?.CommentXml
            && x?.AttributeText == y?.AttributeText;
    }

    public int GetHashCode([DisallowNull] PropertyInfo obj)
    {
        return (obj.CommentXml + obj.AttributeText + obj.Name + obj.Type + obj.IsRequired + obj.IsNullable).GetHashCode();
    }
}
