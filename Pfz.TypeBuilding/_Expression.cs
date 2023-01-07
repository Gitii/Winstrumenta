using System.Linq.Expressions;

namespace Pfz.TypeBuilding;

internal sealed class _Expression : _ICompilable
{
    private readonly Expression _expression;

    internal _Expression(Expression expression)
    {
        _expression = expression;
    }

    Expression _ICompilable._CompileToExpression()
    {
        return _expression;
    }
}
