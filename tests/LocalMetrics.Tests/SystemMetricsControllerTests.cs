using LocalMetrics.App.Controllers;
using LocalMetrics.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.Tests;

[TestClass]
public sealed class SystemMetricsControllerTests
{
    [TestMethod]
    public void Get_ReturnsCurrentMetricsPayload()
    {
        var expected = SystemMetricsServiceTests.CreateMetrics();
        var collector = new SystemMetricsServiceTests.StubSystemMetricsCollector(() => expected);
        var service = SystemMetricsServiceTests.CreateService(collector);
        var controller = new SystemMetricsController(service);

        var result = controller.Get();

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(expected, okResult.Value);
        Assert.AreEqual(1, collector.CallCount);
    }
}
