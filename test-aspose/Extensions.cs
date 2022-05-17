using System;
using System.Collections;
using System.Collections.Generic;

namespace test_aspose
{
	public static class GroupFactory
	{
		public static SalaryGroup Create(ContextEmployeeMaterial[] employees, Link[] links)
		{
			var group = new Repository(employees, links);
			return new SalaryCompany(group);
		}
	}

	internal static class Extensions
	{
		public static int Clamp(this int source, int left, int right)
		{
			// both are right if equals
			var max = left > right ? left : right;
			var min = left < right ? left : right;
			source = min > source ? min : source;
			source = max < source ? max : source;
			return source;
		}

		public static int Div(this int source, int divider)
		{
			var result = source / divider;
			var fract = source % divider * 2;
			result += fract > divider ? 1 : 0;
			return result;
		}

		public static int GetYears(this ContextEmployee source, DateTime from)
		{
			var result = 0;
			while(true)
			{
				var current = from.AddYears(-(result + 1));
				if(current < source.Accepted)
				{
					break;
				}
				result++;
			}

			return result;
		}

		public static void Enqueue<T>(this Queue<T> target, IEnumerable<T> source)
		{
			foreach(var item in source)
			{
				target.Enqueue(item);
			}
		}

		public static void Shift<T>(this T[] source, int slice)
		{
			
			var indexF = 0;
			var indexR = source.Length - 1;
			while(indexF < indexR)
			{
				source.Swap(indexF, indexR);
				indexF++;
				indexR--;
			}

			indexF = source.Length - slice;
			indexR = source.Length - 1;
			while(indexF < indexR)
			{
				source.Swap(indexF, indexR);
				indexF++;
				indexR--;
			}

			indexF = 0;
			indexR = source.Length - slice - 1;
			while(indexF < indexR)
			{
				source.Swap(indexF, indexR);
				indexF++;
				indexR--;
			}
		}

		public static void Swap<T>(this T[] source, int left, int right)
		{
			var temp = source[left];
			source[left] = source[right];
			source[right] = temp;
		}
	}

	public class Wrapper<T> : IEnumerable<T>
	{
		private readonly IEnumerator<T> _enumerator;

		public Wrapper(IEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		public IEnumerator<T> GetEnumerator()
		{
			//? once
			return _enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class Empty<T> : IEnumerator<T>
	{
		public void Dispose() { }

		public bool MoveNext()
		{
			return false;
		}

		public void Reset() { }

		public T Current { get; } = default;
		object IEnumerator.Current => Current;
	}

	public struct Link
	{
		public Index Chef;
		public Index Sub;
	}

	public struct Index
	{
		private readonly int _value;

		private Index(int value)
		{
			_value = value;
		}

		public static implicit operator Index(int value)
		{
			return new Index(value);
		}

		public static explicit operator int(Index index)
		{
			return index._value;
		}

		public override string ToString()
		{
			return $"[{_value}]";
		}
	}
}
