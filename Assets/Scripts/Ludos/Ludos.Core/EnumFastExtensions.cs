using System;
using System.Runtime.CompilerServices;

public static class EnumFastExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlagFast<T>(this T value, T flag) where T : unmanaged, Enum
    {
        return sizeof(T) switch
        {
            4 => (*(int*)&value & *(int*)&flag) == *(int*)&flag,
            1 => (*(byte*)&value & *(byte*)&flag) == *(byte*)&flag,
            8 => (*(long*)&value & *(long*)&flag) == *(long*)&flag,
            _ => (*(short*)&value & *(short*)&flag) == *(short*)&flag
        };
    }
}