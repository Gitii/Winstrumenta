using System.Linq.Expressions;
using System;
using System.Collections.Generic;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Try() on a FluentMethodBuilder.Body or
/// on any of its sub-blocks. Instances of this class can be used to add the actions
/// the Try and to add finally and catch blocks.
/// </summary>
public sealed class FluentTryBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentTryBuilder<TParentBuilder>>
{
    private readonly List<FluentCatchBuilder<TParentBuilder>> _catches =
        new List<FluentCatchBuilder<TParentBuilder>>();

    internal FluentTryBuilder(TParentBuilder parent, FluentMethodBuilder method)
        : base(parent, method)
    {
    }

    /// <summary>
    /// Ends the actual Try and creates a Catch block.
    /// </summary>
    public FluentCatchBuilder<TParentBuilder> Catch<T>(Expression<Func<T>> expression)
        where T : Exception
    {
        _CheckModifiable();

        _method.AddLocal(expression);
        var result = new FluentCatchBuilder<TParentBuilder>(this, expression.Body);
        _catches.Add(result);
        return result;
    }

    private FluentFinallyBuilder<TParentBuilder>? _finally;

    /// <summary>
    /// Ends the actual Try and creates a finally block.
    /// </summary>
    public FluentFinallyBuilder<TParentBuilder> Finally()
    {
        _CheckModifiable();

        if (_finally == null)
        {
            _finally = new FluentFinallyBuilder<TParentBuilder>(this);
        }

        return _finally;
    }

    internal override Expression _CompileToExpression()
    {
        var compiledBody = base._CompileToExpression();

        int count = _catches.Count;
        if (count > 0)
        {
            var catches = new CatchBlock[count];
            for (int i = 0; i < count; i++)
            {
                var catchBlock = _catches[i];
                var compiledCatch = catchBlock._CompileToExpression();
                catches[i] = compiledCatch;
            }

            compiledBody = Expression.TryCatch(compiledBody, catches);
        }

        if (_finally != null)
        {
            var compiledFinally = _finally._CompileToExpression();
            compiledBody = Expression.TryFinally(compiledBody, compiledFinally);
        }

        return compiledBody;
    }
}
