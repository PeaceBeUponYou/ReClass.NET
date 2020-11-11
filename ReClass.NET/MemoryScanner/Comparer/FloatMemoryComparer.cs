using System;
using System.Diagnostics;
using ReClassNET.Extensions;

namespace ReClassNET.MemoryScanner.Comparer
{
	public class FloatMemoryComparer : ISimpleScanComparer
	{
		public ScanCompareType CompareType { get; }
		public ScanRoundMode RoundType { get; }
		public float Value1 { get; }
		public float Value2 { get; }
		public int ValueSize => sizeof(float);

		private readonly int significantDigits;
		private readonly float minValue;
		private readonly float maxValue;

		public FloatMemoryComparer(ScanCompareType compareType, ScanRoundMode roundType, int significantDigits, float value1, float value2)
		{
			CompareType = compareType;

			RoundType = roundType;
			this.significantDigits = Math.Max(significantDigits, 1);
			Value1 = (float)Math.Round(value1, this.significantDigits, MidpointRounding.AwayFromZero);
			Value2 = (float)Math.Round(value2, this.significantDigits, MidpointRounding.AwayFromZero);

			var factor = (int)Math.Pow(10.0, this.significantDigits);

			minValue = value1 - 1.0f / factor;
			maxValue = value1 + 1.0f / factor;
		}

		private bool CheckRoundedEquality(float value) =>
			RoundType switch
		{
				ScanRoundMode.Strict => Value1.IsNearlyEqual((float)Math.Round(value, significantDigits, MidpointRounding.AwayFromZero), 0.0001f),
				ScanRoundMode.Normal => minValue < value && value < maxValue,
				ScanRoundMode.Truncate => (int)value == (int)Value1,
				_ => throw new ArgumentOutOfRangeException()
			};

		public bool Compare(byte[] data, int index, out ScanResult result)
		{
			result = null;

			var value = BitConverter.ToSingle(data, index);

			bool IsMatch() =>
				CompareType switch
			{
					ScanCompareType.Equal => CheckRoundedEquality(value),
					ScanCompareType.NotEqual => !CheckRoundedEquality(value),
					ScanCompareType.GreaterThan => value > Value1,
					ScanCompareType.GreaterThanOrEqual => value >= Value1,
					ScanCompareType.LessThan => value < Value1,
					ScanCompareType.LessThanOrEqual => value <= Value1,
					ScanCompareType.Between => Value1 < value && value < Value2,
					ScanCompareType.BetweenOrEqual => Value1 <= value && value <= Value2,
					ScanCompareType.Unknown => true,
					_ => throw new InvalidCompareTypeException(CompareType)
				};

			if (!IsMatch())
			{
				return false;
			}

			result = new FloatScanResult(value);

			return true;
		}

		public bool Compare(byte[] data, int index, ScanResult previous, out ScanResult result)
		{
#if DEBUG
			Debug.Assert(previous is FloatScanResult);
#endif

			return Compare(data, index, (FloatScanResult)previous, out result);
		}

		public bool Compare(byte[] data, int index, FloatScanResult previous, out ScanResult result)
		{
			result = null;

			var value = BitConverter.ToSingle(data, index);

			bool IsMatch() =>
				CompareType switch
			{
					ScanCompareType.Equal => CheckRoundedEquality(value),
					ScanCompareType.NotEqual => !CheckRoundedEquality(value),
					ScanCompareType.Changed => value != previous.Value,
					ScanCompareType.NotChanged => value == previous.Value,
					ScanCompareType.GreaterThan => value > Value1,
					ScanCompareType.GreaterThanOrEqual => value >= Value1,
					ScanCompareType.Increased => value > previous.Value,
					ScanCompareType.IncreasedOrEqual => value >= previous.Value,
					ScanCompareType.LessThan => value < Value1,
					ScanCompareType.LessThanOrEqual => value <= Value1,
					ScanCompareType.Decreased => value < previous.Value,
					ScanCompareType.DecreasedOrEqual => value <= previous.Value,
					ScanCompareType.Between => Value1 < value && value < Value2,
					ScanCompareType.BetweenOrEqual => Value1 <= value && value <= Value2,
					_ => throw new InvalidCompareTypeException(CompareType)
				};

			if (!IsMatch())
			{
				return false;
			}

			result = new FloatScanResult(value);

			return true;
		}
	}
}
