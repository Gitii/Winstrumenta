using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System;

namespace Pfz.TypeBuilding;

internal interface _IFluentTypeBuilder
{
    Dictionary<
        KeyValuePair<FieldInfo, object>,
        KeyValuePair<MemberExpression, string>
    > Fields { get; }

    FluentMethodBuilder AddMethod(Type type, string name, bool isStatic, bool respectVisibility);

    Expression ThisExpression { get; }
    Expression DelegatesExpression { get; }

    bool IsCollectible { get; }
}
