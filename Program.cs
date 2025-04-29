using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;


var hexstr = "ffff000200000015a066908ecd044d03ff0f38373635343332310000005efc00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";

var buffer = new Span<byte>(Convert.FromHexString(hexstr));
var len = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(4, 4));
var data = buffer.Slice(8, len);

var result = BytesTo<DF_X0_Result>(data.ToArray());
Console.WriteLine(result.ErrorCode);
Console.WriteLine(DateTimeOffset.FromUnixTimeMilliseconds(result.UnixTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"));


T BytesTo<T>(byte[] rawdatas) where T : struct
{
    var t = new T();
    int rawsize = Marshal.SizeOf<T>();
    IntPtr buffer = Marshal.AllocHGlobal(rawsize);
    Marshal.Copy(rawdatas, 0, buffer, rawsize);
    t = Marshal.PtrToStructure<T>(buffer);// warning IL2091: 'T' generic argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicConstructors', 'DynamicallyAccessedMemberTypes.NonPublicConstructors' in 'System.Runtime.InteropServices.Marshal.PtrToStructure<T>(nint)'. The generic parameter 'T' of 'BytesTo<T>(Byte[])' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to.
    Marshal.PtrToStructure(buffer, t);//The structure must not be a value class. (Parameter 'structure'
    Marshal.FreeHGlobal(buffer);
    return t;
}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct BigEndianUInt32 : IEquatable<BigEndianUInt32>
{
    public BigEndianUInt32()
    {
        data = new byte[size];
    }
    const int size = 4;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = size)]
    public byte[] data;
    public static implicit operator uint(BigEndianUInt32 d)
    {
        return BitConverter.IsLittleEndian ?
            BinaryPrimitives.ReadUInt32BigEndian(d.data.AsSpan()) :
            BinaryPrimitives.ReadUInt32LittleEndian(d.data.AsSpan());
    }
    public static implicit operator ulong(BigEndianUInt32 d) => (uint)d;
    public static implicit operator long(BigEndianUInt32 d) => (uint)d;

    public static implicit operator BigEndianUInt32(uint d)
    {
        BigEndianUInt32 bigEndianUInt32 = new BigEndianUInt32();
        bigEndianUInt32.data = new byte[size];
        if (BitConverter.IsLittleEndian)
        {
            BinaryPrimitives.WriteUInt32BigEndian(bigEndianUInt32.data.AsSpan(), d);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bigEndianUInt32.data.AsSpan(), d);
        }
        return bigEndianUInt32;
    }
    public readonly bool Equals(BigEndianUInt32 other) => data.Equals(other.data);
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is BigEndianUInt32 other && Equals(other);
    public readonly override int GetHashCode() => data.GetHashCode();
    public readonly override string? ToString() => Convert.ToHexStringLower(data);

    public static bool operator ==(BigEndianUInt32 a, BigEndianUInt32 b) => (UInt32)a == (UInt32)b;
    public static bool operator !=(BigEndianUInt32 a, BigEndianUInt32 b) => (UInt32)a != (UInt32)b;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct DF_X0_Result
{
    private const int _lane_hex_size = 5;
    private const int _CertificationInfo_size = 8;
    public DF_X0_Result()
    {
        LaneHex = new byte[_lane_hex_size];
        CertificationInfo = new byte[_CertificationInfo_size];
    }

    [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
    public byte CmdType;

    [MarshalAs(UnmanagedType.Struct, SizeConst = 1)]
    public BigEndianUInt32 UnixTime;


    [MarshalAs(UnmanagedType.ByValArray, SizeConst = _lane_hex_size)]
    public byte[] LaneHex;


    [MarshalAs(UnmanagedType.ByValArray, SizeConst = _CertificationInfo_size)]
    public byte[] CertificationInfo;

    [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
    public ushort Reserved;

    [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
    public byte ErrorCode;

}