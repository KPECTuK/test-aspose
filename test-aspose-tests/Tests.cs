using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using test_aspose;

namespace test_aspose_tests
{
	[TestFixture]
	public class Tests
	{
		[TestCaseSource(typeof(SourceSalaryEmployee))]
		public void SalaryEmployee(SourceSalaryEmployee.Data data)
		{
			var company = GroupFactory.Create(data.Employees, data.Links);
			var result = company
				.GetSubGroup(data.IndexEmployee)
				.GetSalaryOn(Extensions.DateTo);
			Assert.AreEqual(data.Expected, result, $"on data set name: {data.DataSetName}");
			var node = Extensions.GraphNodes[data.IndexEmployee];
			TestContext.Out.WriteLine($"found salary for {node.Name}: {result}");
		}

		[TestCaseSource(typeof(SourceSalaryCompany))]
		public void SalaryCompany(SourceSalaryCompany.Data data)
		{
			var company = GroupFactory.Create(data.Employees, data.Links);
			var result = company
				.GetSalaryOn(Extensions.DateTo);
			Assert.AreEqual(data.Expected, result, $"on data set name: {data.DataSetName}");
		}

		[TestCaseSource(typeof(SourceEmployeeYears))]
		public void EmployeeYears(SourceEmployeeYears.Data data)
		{
			var employee = new ContextEmployeeMaterial(string.Empty, data.Accepted, 0);
			var result = employee.GetYears(data.From);
			Assert.AreEqual(data.Expected, result, $"on data set name: {data.DataSetName}");
		}

		[Test]
		public void RepoRoots()
		{
			var repo = new Repository(
				Extensions.GraphNodes,
				Extensions.GraphLinks);

			var components = repo.Connectivity.Distinct().Count();

			Assert.AreEqual(2, components, "components assertion");
			TestContext.Out.WriteLine($"components found: {components}");
			TestContext.Out.WriteLine($"components are: {string.Join(", ", repo.Connectivity)}");
			TestContext.Out.Flush();

			Assert.AreEqual(4, repo.Roots.Count, "root assertion");
			foreach(var root in repo.Roots)
			{
				var node = Extensions.GraphNodes[(int)root];
				TestContext.Out.WriteLine($"root found: {node.Name}");
			}
			TestContext.Out.Flush();

			Assert.AreEqual(Extensions.GraphCycles.Length, repo.Cycles.Length, "cycles count assertion");
			for(var index = 0; index < Extensions.GraphCycles.Length; index++)
			{
				Assert.True(
					Extensions.GraphCycles[index].SequenceEqual(repo.Cycles[index]),
					$"sequence assertion: {string.Join(", ", repo.Cycles[index])}");
				TestContext.Out.WriteLine($"cycle found: {string.Join(", ", repo.Cycles[index])}");
			}
			TestContext.Out.Flush();
		}

		[Test]
		public void RepoIsReachable()
		{
			var repo = new Repository(
				Extensions.GraphNodes,
				Extensions.GraphLinks);

			Assert.True(repo.IsReachable(9, 4) == -1, "must: unreachable from 09 to 04: with -1");
			Assert.True(repo.IsReachable(10, 9) == -1, "must: unreachable from 10 to 09: with -1");
			Assert.True(repo.IsReachable(9, 10) == 1, "must: reachable from 09 to 10: with 1");
			Assert.True(repo.IsReachable(0, 4) == 2, "must: reachable from 00 to 04: with 2");
			Assert.True(repo.IsReachable(0, 7) == 1, "must: reachable from 00 to 07: with 1");
		}
	}

	public static class Extensions
	{
		public const int SALARY_BASE = 1000;

		public static readonly DateTime DateTo = DateTime.Parse("16/05/2022", new CultureInfo("ru-RU"));

		public static readonly ContextEmployeeMaterial[] GraphNodes =
		{
			new EmployeeSales("sales_00", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeSales("sales_01", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeSales("sales_02", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeRegular("reg_03", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeManager("man_04", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeRegular("reg_05", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeRegular("reg_06", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeRegular("reg_07", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeSales("sales_08", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeManager("man_09", DateTo.YearsPast(2), SALARY_BASE),
			new EmployeeRegular("reg_10", DateTo.YearsPast(2), SALARY_BASE),
		};

		public static readonly Link[] GraphLinks =
		{
			// ch 0
			new Link { Chef = 0, Sub = 1 },
			new Link { Chef = 0, Sub = 2 },
			new Link { Chef = 0, Sub = 7 }, // downstream loop
			// ch 1
			new Link { Chef = 1, Sub = 3 },
			new Link { Chef = 1, Sub = 4 }, // share
			// ch 2
			new Link { Chef = 2, Sub = 4 }, // share
			new Link { Chef = 2, Sub = 7 },
			new Link { Chef = 2, Sub = 8 },
			// ch 4
			new Link { Chef = 4, Sub = 5 },
			new Link { Chef = 4, Sub = 6 },
			// ch 5
			new Link { Chef = 5, Sub = 1 }, // upstream loop (non rooted)
			// ch 8
			new Link { Chef = 8, Sub = 0 }, // upstream loop (rooted)
			// ch 9
			new Link { Chef = 9, Sub = 6 },  // component
			new Link { Chef = 9, Sub = 10 }, // component
		};

		public static readonly int[][] GraphCycles =
		{
			new[] { 0, 2, 8 },
			new[] { 1, 4, 5 },
		};

		public static DateTime YearsPast(this DateTime source, int count)
		{
			return source.AddYears(-count);
		}
	}

	public class SourceSalaryEmployee : IEnumerable
	{
		public class Data
		{
			public string DataSetName;
			public ContextEmployeeMaterial[] Employees;
			public Link[] Links;
			public int IndexEmployee;
			public int Expected;
		}

		public IEnumerator GetEnumerator()
		{
			yield return new Data
			{
				DataSetName = "regular_06",
				Employees = Extensions.GraphNodes,
				Links = Extensions.GraphLinks,
				IndexEmployee = 6,
				Expected =
					Extensions.SALARY_BASE +               // base
					(Extensions.SALARY_BASE * 6).Div(100), // bonus
			};
			yield return new Data
			{
				DataSetName = "manager_04",
				Employees = Extensions.GraphNodes,
				Links = Extensions.GraphLinks,
				IndexEmployee = 4,
				Expected =
					Extensions.SALARY_BASE +                                   // base
					(Extensions.SALARY_BASE * 10).Div(100) +                   // bonus
					2 * ((Extensions.SALARY_BASE * 6).Div(100) * 5).Div(1000), // interest
			};
		}
	}

	public class SourceSalaryCompany : IEnumerable
	{
		public class Data
		{
			public string DataSetName;
			public ContextEmployeeMaterial[] Employees;
			public Link[] Links;
			public int Expected;
		}

		public IEnumerator GetEnumerator()
		{
			yield return new Data
			{
				DataSetName = "main company",
				Employees = Extensions.GraphNodes,
				Links = Extensions.GraphLinks,
				Expected = 0,
			};
		}
	}

	public class SourceEmployeeYears : IEnumerable
	{
		public class Data
		{
			public string DataSetName;
			public DateTime Accepted;
			public DateTime From;
			public int Expected;
		}

		public IEnumerator GetEnumerator()
		{
			yield return new Data
			{
				DataSetName = "one year",
				From = new DateTime(2022, 2, 22),
				Accepted = new DateTime(2021, 2, 20),
				Expected = 1,
			};
			yield return new Data
			{
				DataSetName = "zero year",
				From = new DateTime(2022, 2, 22),
				Accepted = new DateTime(2021, 8, 12),
				Expected = 0,
			};
			yield return new Data
			{
				DataSetName = "leap cross",
				From = new DateTime(2022, 2, 22),
				Accepted = new DateTime(2019, 8, 12),
				Expected = 2,
			};
			yield return new Data
			{
				DataSetName = "leap bound",
				From = new DateTime(2020, 2, 29),
				Accepted = new DateTime(2016, 2, 29),
				Expected = 4,
			};
		}
	}
}
