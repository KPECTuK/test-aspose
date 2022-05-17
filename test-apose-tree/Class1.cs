using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace test_apose_tree
{
	public interface ILink
	{
		ILink Chef { get; set; }
		int Level { get; set; }
		int GetSalaryOn(Repository over, DateTime on);
	}

	public abstract class EmployeeBase : ILink
	{
		public string Name;
		public DateTime Accepted;
		public int SalaryBase;
		//
		ILink ILink.Chef { get; set; }
		int ILink.Level { get; set; }

		public EmployeeBase(string name, DateTime accepted, int salaryBase)
		{
			Name = name;
			Accepted = accepted;
			SalaryBase = salaryBase;
		}

		public abstract int GetSalaryOn(Repository over, DateTime on);
	}

	public class EmployeeRegular : EmployeeBase
	{
		public EmployeeRegular(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(Repository over, DateTime on)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(on) * 3).Clamp(0, 30) * SalaryBase).Div(100);
			// may or may not calc subordinates
			return @base + bonus;
		}
	}

	public class EmployeeManager : EmployeeBase
	{
		public EmployeeManager(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(Repository over, DateTime on)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(on) * 5).Clamp(0, 40) * SalaryBase).Div(100);
			var group = over.FilterBy(_ => ReferenceEquals(_.Chef, this));
			var interest = group.Select(_ => _.GetSalaryOn(over, on)).Sum(_ => (_ * 5).Div(1000));
			return @base + bonus + interest;
		}
	}

	public class EmployeeSales : EmployeeBase
	{
		public EmployeeSales(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(Repository over, DateTime on)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(on) * 1).Clamp(0, 35) * SalaryBase).Div(100);
			var group = over.FilterBy(_ => over.IsReachable(_, this) != -1).OrderByDescending(_ => _.Level);
			var interest = group.Select(_ => _.GetSalaryOn(over, on)).Sum(_ => (_ * 3).Div(1000));
			return @base + bonus + interest;
		}
	}

	public class Repository
	{
		public readonly List<ILink> Tree = new List<ILink>();

		public ILink GetByName(string name)
		{
			foreach(var link in Tree)
			{
				var cast = link as EmployeeBase;
				if(string.Equals(cast?.Name, name))
				{
					return link;
				}
			}

			return null;
		}

		public int IsReachable(ILink from, ILink to)
		{
			var weight = 0;
			var current = from;
			while(!ReferenceEquals(null, current) && !ReferenceEquals(to, current))
			{
				current = current.Chef;
				weight++;
			}

			return ReferenceEquals(null, current) ? -1 : weight;
		}

		public void MarkupLevels(ILink from)
		{
			foreach(var link in Tree)
			{
				var level = IsReachable(link, from);
				if(level != -1)
				{
					link.Level = level;
				}
			}
		}

		public IEnumerable<EmployeeBase> FilterBy(Predicate<EmployeeBase> predicate)
		{
			return new Wrapper<EmployeeBase>(FilterRoll(predicate));
		}

		public IEnumerable<ILink> FilterBy(Predicate<ILink> predicate)
		{
			return new Wrapper<ILink>(FilterRoll(predicate));
		}

		private IEnumerator<T> FilterRoll<T>(Predicate<T> predicate) where T : class
		{
			var index = -1;
			predicate = predicate ?? (_ => true);
			while(++index < Tree.Count)
			{
				// cast to predicate on null
				var instance = Tree[index] as T;
				if(predicate(instance))
				{
					yield return instance;
				}
			}
		}
	}

	public class B
	{
		private readonly List<B> _builders = new List<B>();

		protected ILink Link;

		public static B<T> Of<T>(string name, DateTime accepted, int salary) where T : ILink
		{
			return new B<T>(name, accepted, salary);
		}

		public B Add(params B[] children)
		{
			_builders.AddRange(children);
			return this;
		}

		public virtual ILink Build(Repository repo)
		{
			foreach(var builder in _builders)
			{
				var instance = builder.Build(repo);
				instance.Chef = Link;
				
				if(ReferenceEquals(null, Link))
				{
					repo.MarkupLevels(instance);
				}
			}

			return Link;
		}
	}

	public class B<T> : B where T : ILink
	{
		private readonly string _name;
		private readonly DateTime _accepted;
		private readonly int _salary;

		public B(string name, DateTime accepted, int salary)
		{
			_name = name;
			_accepted = accepted;
			_salary = salary;
		}

		public override ILink Build(Repository repo)
		{
			var current = Activator.CreateInstance(
				typeof(T),
				BindingFlags.Instance | BindingFlags.Public,
				null,
				new object[] { _name, _accepted, _salary },
				CultureInfo.InvariantCulture,
				new object[] { });
			Link = current as ILink;
			repo.Tree.Add(Link);
			return base.Build(repo);
		}
	}

	public static class Extensions
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

		public static int GetYears(this EmployeeBase source, DateTime from)
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
}
