using System.Linq.Expressions;

namespace Pfz.TypeBuilding;

internal interface _ICompilable
{
    Expression _CompileToExpression();
}
