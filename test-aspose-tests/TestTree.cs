using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using test_apose_tree;

namespace test_aspose_tests
{
	[TestFixture]
	public class TestTree
	{

		[TestCaseSource(typeof(TestsDataSource))]
		public void Test(TestsDataSource.Data data)
		{
			TestContext.Out.WriteLine($"test case for employee name: {data.Name ?? "company gross"}");

			var repo = new Repository();
			data.BuildTree(repo);
			var link = repo.GetByName(data.Name);
			if(ReferenceEquals(null, link))
			{
				var total = repo.Tree.Select(_ => _.GetSalaryOn(repo, data.DateTo)).Sum();
				Assert.AreEqual(data.Expected, total);
			}
			else
			{
				Assert.AreEqual(data.Expected, link.GetSalaryOn(repo, data.DateTo));
			}
		}
	}

	public class TestsDataSource : IEnumerable
	{

		public class Data
		{
			public const int SALARY_BASE = 1000;

			public static readonly DateTime DateToBase = DateTime.Parse("16/05/2022", new CultureInfo("ru-RU"));

			public string Name;
			public DateTime DateTo;
			public int Expected;

			public void BuildTree(Repository repo)
			{
				// ReSharper disable HeapView.ObjectAllocation
				new B().Add(
						B.Of<EmployeeManager>("man_01", DateTo.YearsPast(2), SALARY_BASE).Add(
							B.Of<EmployeeSales>("sal_02", DateTo.YearsPast(2), SALARY_BASE).Add(
								B.Of<EmployeeManager>("man_03", DateTo.YearsPast(2), SALARY_BASE).Add(
									B.Of<EmployeeRegular>("reg_04", DateTo.YearsPast(2), SALARY_BASE)),
								B.Of<EmployeeManager>("man_05", DateTo.YearsPast(2), SALARY_BASE).Add(
									B.Of<EmployeeRegular>("reg_06", DateTo.YearsPast(2), SALARY_BASE),
									B.Of<EmployeeRegular>("reg_07", DateTo.YearsPast(2), SALARY_BASE))),
							B.Of<EmployeeSales>("sal_08", DateTo.YearsPast(2), SALARY_BASE).Add(
								B.Of<EmployeeRegular>("reg_09", DateTo.YearsPast(2), SALARY_BASE),
								B.Of<EmployeeRegular>("reg_10", DateTo.YearsPast(2), SALARY_BASE)),
							B.Of<EmployeeRegular>("reg_11", DateTo.YearsPast(2), SALARY_BASE)),
						B.Of<EmployeeSales>("sal_12", DateTo.YearsPast(1), SALARY_BASE).Add(
							B.Of<EmployeeRegular>("reg_13", DateTo.YearsPast(1), SALARY_BASE)))
					.Build(repo);
				// ReSharper restore HeapView.ObjectAllocation
			}

			public static int GetTotalCompanyGross()
			{
				//TODO: need to count
				return 0;
			}
		}

		public IEnumerator GetEnumerator()
		{
			// salary for regular
			yield return new Data
			{
				Name = "reg_04",
				DateTo = Data.DateToBase,
				Expected =
					ExtensionsDCG.SALARY_BASE +              // base
					(ExtensionsDCG.SALARY_BASE * 6).Div(100) // bonus
			};

			// salary for manager
			yield return new Data
			{
				Name = "man_05",
				DateTo = Data.DateToBase,
				Expected =
					ExtensionsDCG.SALARY_BASE +                                                                // base
					(ExtensionsDCG.SALARY_BASE * 10).Div(100) +                                                // bonus
					2 * ((ExtensionsDCG.SALARY_BASE + (ExtensionsDCG.SALARY_BASE * 6).Div(100)) * 5).Div(1000) // interest
			};

			// salary for sales
			yield return new Data
			{
				Name = "sal_12",
				DateTo = Data.DateToBase,
				Expected =
					ExtensionsDCG.SALARY_BASE +                                                            // base
					ExtensionsDCG.SALARY_BASE.Div(100) +                                                   // bonus
					((ExtensionsDCG.SALARY_BASE + (ExtensionsDCG.SALARY_BASE * 6).Div(100)) * 3).Div(1000) // interest
			};

			// total company gross
			yield return new Data
			{
				Name = null,
				DateTo = Data.DateToBase,
				Expected = Data.GetTotalCompanyGross()
			};
		}
	}
}
