using System.Linq.Expressions;
using Mapster;
namespace ${Namespace}.Utils;

public static partial class Extensions
{
    /// <summary>
    /// 将被合并对象中非空属性值，合并到源对象
    /// </summary>
    /// <typeparam name="TSource">源对象</typeparam>
    /// <typeparam name="TMerge">被合并对象</typeparam>
    /// <param name="source"></param>
    /// <param name="merge"></param>
    /// <param name="ignoreNull">是否忽略null</param>
    /// <returns></returns>
    public static TSource Merge<TSource, TMerge>(this TSource source, TMerge merge, bool ignoreNull = true)
    {
        if (ignoreNull)
        {
            TypeAdapterConfig<TMerge, TSource>
                .NewConfig()
                .IgnoreNullValues(true);
        }
        return merge.Adapt(source);
    }

    /// <summary>
    /// 类型转换
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TDestination">目标类型</typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TDestination MapTo<TSource, TDestination>(this TSource source) where TDestination : class
    {
        TypeAdapterConfig<TSource, TDestination>
           .NewConfig()
           .IgnoreNullValues(true);
        return source.Adapt<TSource, TDestination>();
    }

    /// <summary>
    /// 构造查询Dto
    /// 重要: dto中属性名称和类型必须与实体一致
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IQueryable<TResult> Select<TSource, TResult>(this IQueryable<TSource> source)
    {
        var sourceType = typeof(TSource);
        var resultType = typeof(TResult);
        var parameter = Expression.Parameter(sourceType, "e");

        // 只构造都存在的属性
        var sourceNames = sourceType.GetProperties()
            .Select(s => s.Name).ToList();
        var props = resultType.GetProperties().ToList();
        props = props.Where(p => sourceNames.Contains(p.Name)).ToList();
        //props = props.Intersect(sourceProps).ToList();

        var bindings = props.Select(p =>
             Expression.Bind(p, Expression.PropertyOrField(parameter, p.Name))
        ).ToList();
        var body = Expression.MemberInit(Expression.New(resultType), bindings);
        var selector = Expression.Lambda(body, parameter);
        return source.Provider.CreateQuery<TResult>(
            Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, resultType },
                source.Expression, Expression.Quote(selector)));
    }

    public static IQueryable<TResult> ProjectTo<TResult>(this IQueryable source)
    {
        return source.ProjectToType<TResult>();
    }

    /// <summary>
    /// 排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, Dictionary<string, bool> dic)
    {
        IOrderedQueryable<T> orderQuery = default!;
        var parameter = Expression.Parameter(typeof(T), "e");
        foreach (var item in dic)
        {
            var prop = Expression.PropertyOrField(parameter, item.Key);
            var body = Expression.MakeMemberAccess(parameter, prop.Member);
            var selector = Expression.Lambda(body, parameter);


            MethodCallExpression expression;
            if (item.Value)
            {
                expression = Expression.Call(typeof(Queryable),
                                          "OrderBy",
                                          new Type[] { typeof(T), body.Type },
                                          query.Expression,
                                          Expression.Quote(selector));
            }
            else
            {
                expression = Expression.Call(typeof(Queryable),
                                          "OrderByDescending",
                                          new Type[] { typeof(T), body.Type },
                                          query.Expression,
                                          Expression.Quote(selector));
            }
            orderQuery = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(expression);
            query = orderQuery;
        }
        return orderQuery;
    }

}
