﻿using Tutorial3.Models;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();

        List<Emp> result = emps
            .Where(e => e.Job == "SALESMAN")
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();

        List<Emp> result = emps
            .Where(e => e.DeptNo == 30)
            .OrderByDescending(e => e.Sal)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        List<int> deptNos = depts
            .Where(d => d.Loc == "CHICAGO")
            .Select(d => d.DeptNo)
            .ToList();

        List<Emp> result = emps
            .Where(e => deptNos.Contains(e.DeptNo))
            .ToList();

        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        List<(string EName, decimal Sal)> result = emps
            .Select(e => (e.EName, e.Sal))
            .ToList();

        Assert.All(result, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.EName));
            Assert.True(r.Sal > 0);
        });


    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        List<(string EName, string DName)> result = emps
            .Join(depts,
                e => e.DeptNo,
                d => d.DeptNo,
                (e, d) => (e.EName, d.DName))
            .ToList();

        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");

    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

        List<(int DeptNo, int Count)> result = emps
            .GroupBy(e => e.DeptNo)
            .Select(g => (DeptNo: g.Key, Count: g.Count()))
            .ToList();

        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);

    }

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

        List<Emp> result = emps
            .Where(e => e.Comm.HasValue)
            .ToList();

        Assert.All(result, r => Assert.NotNull(r.Comm));

    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        List<(string EName, int Grade)> result = grades
            .SelectMany(g => emps
                .Where(e => e.Sal >= g.Losal && e.Sal <= g.Hisal)
                .Select(e => (e.EName, g.Grade)))
            .ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);

    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();

        List<(int DeptNo, decimal AvgSal)> result = emps
            .GroupBy(e => e.DeptNo)
            .Select(g => (g.Key, g.Average(e => e.Sal)))
            .ToList();

        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
        
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();

        var avgPerDept = emps
            .GroupBy(e => e.DeptNo)
            .ToDictionary(g => g.Key, g => g.Average(e => e.Sal));

        List<string> result = emps
            .Where(e => avgPerDept.ContainsKey(e.DeptNo) && e.Sal > avgPerDept[e.DeptNo])
            .Select(e => e.EName)
            .ToList();

        Assert.Contains("ALLEN", result);
        
    }
}
