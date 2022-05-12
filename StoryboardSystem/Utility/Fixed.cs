using System.Globalization;
using System.IO;

namespace StoryboardSystem; 

internal readonly struct Fixed {
    private const int DEC_BITS = 14;
    private const int ONE_BIT = 1 << DEC_BITS;
    private const int FLOOR_MASK = ~(ONE_BIT - 1);
    private const float ONE_F = ONE_BIT;
    private const float INV_ONE_F = 1f / ONE_F;

    public static Fixed Zero => new(0);
    
    private readonly int val;

    private Fixed(int val) => this.val = val;

    public void Serialize(BinaryWriter writer) => writer.Write(val);

    public override bool Equals(object obj) => obj is Fixed other && val == other.val;

    public override int GetHashCode() => val;

    public override string ToString() => ((float) this).ToString(CultureInfo.InvariantCulture);

    public static int FloorToInt(Fixed f) {
        if (f.val >= 0)
            return f.val >> DEC_BITS;

        return -(-f.val >> DEC_BITS);
    }

    public static Fixed Floor(Fixed f) {
        if (f.val >= 0)
            return new Fixed(f.val & FLOOR_MASK);

        return new Fixed(-(-f.val & FLOOR_MASK));
    }

    public static Fixed Deserialize(BinaryReader reader) => new(reader.ReadInt32());

    public static Fixed operator +(Fixed a, Fixed b) => new(a.val + b.val);

    public static Fixed operator -(Fixed a, Fixed b) => new(a.val - b.val);

    public static Fixed operator *(Fixed a, Fixed b) => new((int) (((long) a.val * b.val) >> DEC_BITS));

    public static Fixed operator *(Fixed a, int b) => new(a.val * b);

    public static Fixed operator *(Fixed a, float b) => a * (Fixed) b;

    public static Fixed operator *(int a, Fixed b) => new(a * b.val);

    public static Fixed operator *(float a, Fixed b) => (Fixed) a * b;

    public static Fixed operator /(Fixed a, Fixed b) => new((int) ((((long) a.val << 32) / b.val) >> (32 - DEC_BITS)));

    public static Fixed operator /(Fixed a, int b) => new(a.val / b);

    public static Fixed operator /(Fixed a, float b) => a / (Fixed) b;

    public static Fixed operator %(Fixed a, int b) => new(a.val % b);

    public static bool operator ==(Fixed a, Fixed b) => a.val == b.val;
    
    public static bool operator !=(Fixed a, Fixed b) => a.val != b.val;

    public static bool operator >(Fixed a, Fixed b) => a.val > b.val;

    public static bool operator >=(Fixed a, Fixed b) => a.val >= b.val;

    public static bool operator <(Fixed a, Fixed b) => a.val < b.val;

    public static bool operator <=(Fixed a, Fixed b) => a.val <= b.val;

    public static explicit operator int(Fixed f) => FloorToInt(f);

    public static explicit operator Fixed(int i) => new(i << DEC_BITS);

    public static explicit operator float(Fixed f) => INV_ONE_F * f.val;

    public static explicit operator Fixed(float f) => new((int) (ONE_F * f));
}

internal static class FixedIOExtensions {
    public static void Write(this BinaryWriter writer, Fixed f) => f.Serialize(writer);
    
    public static Fixed ReadFixed(this BinaryReader reader) => Fixed.Deserialize(reader);
}