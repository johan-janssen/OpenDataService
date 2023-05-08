using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace OpenDataService.Web.Extensions;

public static class IQueryableExtensions
{
    private static MethodInfo whereFunc;
    static IQueryableExtensions()
    {
        var tQueryable = typeof(Queryable);
        whereFunc = tQueryable.GetMethods().Where(m => m.Name == "Where" && m.GetParameters()[1].ParameterType.GenericTypeArguments[0].GenericTypeArguments.Length == 2).Single();
    }

    public static IQueryable Filter(this IQueryable q, string propertyName, ExpressionType comparison, object value)
    {
        var type = q.ElementType;
        var whereFuncGeneric = whereFunc.MakeGenericMethod(type);
        var itemExpression = Expression.Parameter(type, "s");
        var propertyExpression = Expression.Property(itemExpression, propertyName);
        var filterExpression = Expression.Lambda(Expression.MakeBinary(comparison, propertyExpression, Expression.Constant(value)), itemExpression);
        var expression = Expression.Call(whereFuncGeneric, q.Expression, filterExpression);
        return q.Provider.CreateQuery(expression);
    }

    
}