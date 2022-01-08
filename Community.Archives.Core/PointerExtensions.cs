using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.Archives.Core
{
    public static class PointerExtensions
    {
        public static unsafe bool EqualsPointerArray(this byte[] expectedArray, byte* actualArray)
        {
            if (new IntPtr(actualArray) == IntPtr.Zero)
            {
                return false;
            }

            for (var i = 0; i < expectedArray.Length; i++)
            {
                if (expectedArray[i] != actualArray[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
