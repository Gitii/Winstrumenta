using System.Linq.Expressions;

namespace Pfz.TypeBuilding;

internal interface _ILoop : _IBlockBuilder
{
    LabelTarget ContinueTarget { get; }
    LabelTarget BreakTarget { get; }
}
