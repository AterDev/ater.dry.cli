using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datastore;
internal class EqualityComparables
{

}

public class PropertyEquality : IEqualityComparer<PropertyInfo>
{
    public bool Equals(PropertyInfo? x, PropertyInfo? y)
    {
        return x.Name == y.Name && x.Type == y.Type && x.IsRequired == y.IsRequired &&
            x.Comments == y.Comments && x.AttributeText == y.AttributeText;
    }

    public int GetHashCode([DisallowNull] PropertyInfo obj)
    {
        return (obj.Comments + obj.AttributeText + obj.Name + obj.Type + obj.IsRequired).GetHashCode();
    }
}
