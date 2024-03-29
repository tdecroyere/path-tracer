using System.Globalization;
using BenchmarkDotNet.Helpers;
using JetBrains.Annotations;
using Perfolizer.Common;

namespace ResultsComparer;

public struct SizeValue
{
    public long Bytes { get; }

    public SizeValue(long bytes) => Bytes = bytes;

    public SizeValue(long bytes, SizeUnit unit) : this(bytes * unit.ByteAmount) { }

    public static readonly SizeValue B = SizeUnit.B.ToValue();
    public static readonly SizeValue KB = SizeUnit.KB.ToValue();
    public static readonly SizeValue MB = SizeUnit.MB.ToValue();
    public static readonly SizeValue GB = SizeUnit.GB.ToValue();
    public static readonly SizeValue TB = SizeUnit.TB.ToValue();

    public static SizeValue FromBytes(long value) => value * B;
    public static SizeValue FromKilobytes(long value) => value * KB;
    public static SizeValue FromMegabytes(long value) => value * MB;
    public static SizeValue FromGigabytes(long value) => value * GB;
    public static SizeValue FromTerabytes(long value) => value * TB;

    public static SizeValue operator *(SizeValue value, long k) => new SizeValue(value.Bytes * k);
    public static SizeValue operator *(long k, SizeValue value) => new SizeValue(value.Bytes * k);

    public string ToString(
        CultureInfo cultureInfo,
        string format = "0.##",
        UnitPresentation unitPresentation = null)
    {
        return ToString(null, cultureInfo, format, unitPresentation);
    }

    public string ToString(
        SizeUnit sizeUnit,
        CultureInfo cultureInfo,
        string format = "0.##",
       UnitPresentation unitPresentation = null)
    {
        sizeUnit = sizeUnit ?? SizeUnit.GetBestSizeUnit(Bytes);
        cultureInfo = cultureInfo ?? System.Globalization.CultureInfo.InvariantCulture;
        format = format ?? "0.##";
        unitPresentation = unitPresentation ?? UnitPresentation.Default;
        double unitValue = SizeUnit.Convert(Bytes, SizeUnit.B, sizeUnit);
        if (unitPresentation.IsVisible)
        {
            string unitName = sizeUnit.Name.PadLeft(unitPresentation.MinUnitWidth);
            return $"{unitValue.ToString(format, cultureInfo)} {unitName}";
        }

        return unitValue.ToString(format, cultureInfo);
    }
}