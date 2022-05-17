using System;
using System.Collections.Generic;

namespace test_aspose
{
	public abstract class SalaryGroup
	{
		internal readonly Repository Repo;

		internal SalaryGroup(Repository repository)
		{
			Repo = repository;
		}

		public SalaryGroup GetSubGroup(Index forChef)
		{
			return Repo.Cahce[(int)forChef];
		}

		public abstract int GetSalaryOn(DateTime date);

		public abstract IEnumerable<int> GetSalarySubOn(DateTime date);
	}

	/// <summary> eval single </summary>
	public class SalarySingle : SalaryGroup
	{
		private readonly Index _chef;

		internal SalarySingle(Repository repo, Index chef) : base(repo)
		{
			_chef = chef;
		}

		public override int GetSalaryOn(DateTime date)
		{
			var desc = Repo.Nodes[(int)_chef] as EmployeeCache<SalarySingle>;
			if(ReferenceEquals(null, desc))
			{
				throw new Exception("method is not supported");
			}

			if(!date.Equals(desc.EvaluationCache))
			{
				desc.EvaluationCache = date;
				desc.SalaryCache = desc.GetSalaryOn(this, date);
			}

			return desc.SalaryCache;
		}

		public override IEnumerable<int> GetSalarySubOn(DateTime date)
		{
			return new Wrapper<int>(new Empty<int>());
		}
	}

	/// <summary> eval one level deep </summary>
	public class SalaryLevel : SalaryGroup
	{
		private readonly Index _chef;

		internal SalaryLevel(Repository repo, Index chef) : base(repo)
		{
			_chef = chef;
		}

		public override int GetSalaryOn(DateTime date)
		{
			var desc = Repo.Nodes[(int)_chef] as EmployeeCache<SalaryLevel>;
			if(ReferenceEquals(null, desc))
			{
				throw new Exception("method is not supported");
			}

			if(!date.Equals(desc.EvaluationCache))
			{
				desc.EvaluationCache = date;
				desc.SalaryCache = desc.GetSalaryOn(this, date);
			}

			return desc.SalaryCache;
		}

		public override IEnumerable<int> GetSalarySubOn(DateTime date)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary> eval branch </summary>
	public class SalarySub : SalaryGroup
	{
		private Index _chef;

		internal SalarySub(Repository repo, Index chef) : base(repo)
		{
			_chef = chef;
		}

		public override int GetSalaryOn(DateTime date)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<int> GetSalarySubOn(DateTime date)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary> eval branches for all roots found </summary>
	internal class SalaryCompany : SalaryGroup
	{
		public SalaryCompany(Repository group) : base(group) { }

		public override int GetSalaryOn(DateTime date)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<int> GetSalarySubOn(DateTime date)
		{
			throw new NotImplementedException();
		}
	}
}
