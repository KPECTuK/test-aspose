using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace test_aspose
{
	internal interface ISalesGroupFactory
	{
		SalaryGroup Create(Repository repo, Index chef);
	}

	public abstract class ContextEmployee
	{
		public abstract string Name { get; }
		public abstract DateTime Accepted { get; }
		public abstract int SalaryBase { get; }
		public abstract int Level { get; }

		public bool Equals(ContextEmployee other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return
				Equals(Name, other.Name) &&
				Equals(Accepted, other.Accepted);
		}

		public override bool Equals(object @object)
		{
			return Equals(@object as ContextEmployee);
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}
	}

	public class ContextEmployeeMaterial : ContextEmployee
	{
		public override string Name { get; }
		public override DateTime Accepted { get; }
		public override int SalaryBase { get; }

		public override int Level { get; }

		public ContextEmployeeMaterial(string name, DateTime accepted, int salaryBase)
		{
			Name = name;
			Accepted = accepted;
			SalaryBase = salaryBase;
		}
	}

	public abstract class EmployeeCache<T> : ContextEmployeeMaterial, ISalesGroupFactory where T : SalaryGroup
	{
		public DateTime EvaluationCache;
		public int SalaryCache;

		protected EmployeeCache(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public abstract int GetSalaryOn(T group, DateTime date);

		SalaryGroup ISalesGroupFactory.Create(Repository repo, Index chef)
		{
			return Activator.CreateInstance(
				typeof(T),
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new object[] { repo, chef },
				CultureInfo.InvariantCulture,
				new object[] { }) as SalaryGroup;
		}
	}

	//

	public class EmployeeRegular : EmployeeCache<SalarySingle>
	{
		public EmployeeRegular(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(SalarySingle group, DateTime date)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(date) * 3).Clamp(0, 30) * SalaryBase).Div(100);
			// may or may not calc subordinates
			return @base + bonus;
		}
	}

	public class EmployeeManager : EmployeeCache<SalaryLevel>
	{
		public EmployeeManager(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(SalaryLevel group, DateTime date)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(date) * 5).Clamp(0, 40) * SalaryBase).Div(100);
			var interest = group.GetSalarySubOn(date).Sum(_ => (_ * 5).Div(1000));
			return @base + bonus + interest;
		}
	}

	public class EmployeeSales : EmployeeCache<SalarySub>
	{
		public EmployeeSales(string name, DateTime accepted, int salaryBase)
			: base(name, accepted, salaryBase) { }

		public override int GetSalaryOn(SalarySub group, DateTime date)
		{
			var @base = SalaryBase;
			var bonus = ((this.GetYears(date) * 1).Clamp(0, 35) * SalaryBase).Div(100);
			var interest = group.GetSalarySubOn(date).Sum(_ => (_ * 3).Div(1000));
			return @base + bonus + interest;
		}
	}

	//

	internal sealed class ContextEmployeeAttached : ContextEmployee
	{
		private readonly Repository _group;
		private readonly int _index;

		public ContextEmployeeAttached(Repository group, int index)
		{
			_group = group;
			_index = index;
		}

		//TODO: remove loop
		public override string Name => _group.Nodes[_index].Name;
		public override DateTime Accepted => _group.Nodes[_index].Accepted;
		public override int SalaryBase => _group.Nodes[_index].SalaryBase;
		public override int Level => _group.Nodes[_index].Level;
	}
}
