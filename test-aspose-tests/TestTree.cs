using System;
using System.Globalization;
using NUnit.Framework;
using test_apose_tree;

namespace test_aspose_tests
{
	[TestFixture]
	public class TestTree
	{
		public const int SALARY_BASE = 1000;

		public static readonly DateTime DateTo = DateTime.Parse("16/05/2022", new CultureInfo("ru-RU"));

		[Test]
		public void Test()
		{
			var repo = new Repository();
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
				B.Of<EmployeeSales>("sal_12", DateTo.YearsPast(2), SALARY_BASE).Add(
					B.Of<EmployeeRegular>("reg_13", DateTo.YearsPast(2), SALARY_BASE)))
			.Build(repo);
			// ReSharper restore HeapView.ObjectAllocation

			var link = repo.GetByName("reg_04");
			var expected =
				ExtensionsDCG.SALARY_BASE +               // base
				(ExtensionsDCG.SALARY_BASE * 6).Div(100); // bonus
			Assert.AreEqual(expected, link.GetSalaryOn(repo, DateTo));
			link = repo.GetByName("man_05");
			expected =
				ExtensionsDCG.SALARY_BASE +                                   // base
				(ExtensionsDCG.SALARY_BASE * 10).Div(100) +                   // bonus
				2 * ((ExtensionsDCG.SALARY_BASE * 6).Div(100) * 5).Div(1000); // interest
			Assert.AreEqual(expected, link.GetSalaryOn(repo, DateTo));
		}
	}
}
