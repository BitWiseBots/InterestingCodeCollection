/***********************************************************
 This is a combination of methods that I developed in order
 to be able to test code that relied upon the  DbFunctions class
 for filtering search results.
 
 The example below is contrived, but even so the value of this is admittedly low
 since you aren't testing the prod implementation really.
***********************************************************/
public void Usage()
{
    _db.SomeEnumerable
                    .Where(x=> x.TeamCityProjectId == projectId)
                    .Where(GetDateRageExpression(7))
                    .ToList()
}

protected override Expression<Func<TestRun, int?>> DaysDiffExpr //Testable Class
{
    get { return testRun => (DateTime.Now - testRun.StartDateTime).Days; }
}

protected virtual Expression<Func<TestRun, int?>> DaysDiffExpr //Production Class
{
    get { return testRun => DbFunctions.DiffDays(testRun.StartDateTime, DateTime.Now); }
}

private Expression<Func<TestRun, bool>> GetDateRageExpression(int numOfDays)
{
    //Build the Date comparison logic for GetHistoricalTrend
    //Will output 'testRun => DbFunctions.DiffDays(testRun.StartDateTime, DateTime.Now) <= [numOfDays]' by default
    var left = DaysDiffExpr;
    var right = Expression.Constant(numOfDays, typeof(int?));

    Expression predicate = Expression.LessThanOrEqual(left.Body, right);

    return Expression.Lambda<Func<TestRun, bool>>(predicate, left.Parameters);
} 