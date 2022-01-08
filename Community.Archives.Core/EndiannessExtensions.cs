using System.Buffers.Binary;
using System.Reflection;

namespace Community.Archives.Core;

public static class EndiannessExtensions
{
    public static ref T ConvertByteOrder<T>(ref this T obj) where T : struct
    {
        var classAttr = typeof(T).GetCustomAttribute<Endianness>();
        if (classAttr != null)
        {
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                if (classAttr.ByteOrder == ByteOrder.BigEndian)
                {
                    if (fieldInfo.FieldType == typeof(short))
                    {
                        var typedRef = __makeref(obj);
                        short number = (short)fieldInfo.GetValueDirect(typedRef)!;
                        fieldInfo.SetValueDirect(
                            typedRef,
                            BinaryPrimitives.ReverseEndianness(number)
                        );
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        var typedRef = __makeref(obj);
                        int number = (int)fieldInfo.GetValueDirect(typedRef)!;
                        fieldInfo.SetValueDirect(
                            typedRef,
                            BinaryPrimitives.ReverseEndianness(number)
                        );
                    }
                }
            }
        }
        else
        {
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var endianness = fieldInfo.GetCustomAttribute<Endianness>();

                if (endianness != null)
                {
                    if (endianness.ByteOrder == ByteOrder.BigEndian)
                    {
                        if (fieldInfo.FieldType == typeof(short))
                        {
                            var typedRef = __makeref(obj);
                            short number = (short)fieldInfo.GetValueDirect(typedRef)!;
                            fieldInfo.SetValueDirect(
                                typedRef,
                                BinaryPrimitives.ReverseEndianness(number)
                            );
                        }
                        else if (fieldInfo.FieldType == typeof(int))
                        {
                            var typedRef = __makeref(obj);
                            int number = (int)fieldInfo.GetValueDirect(typedRef)!;
                            fieldInfo.SetValueDirect(
                                typedRef,
                                BinaryPrimitives.ReverseEndianness(number)
                            );
                        }
                        else
                        {
                            throw new Exception("Unsupported field type");
                        }
                    }
                }
            }
        }

        return ref obj;
    }
}
