namespace Woggle;

[Flags]
internal enum ArrayHandleFlags : byte
{
    None = 0,
    Disposed = 1 << 1,
    TypeIsReferenceOrContainsReferences = 1 << 2,
}
