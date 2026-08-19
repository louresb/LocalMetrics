using System.Globalization;
using LocalMetrics.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.Api.Tests;

[TestClass]
public sealed class MetricsControllerTests
{
    [TestMethod]
    public void Get_ReturnsPrometheusTextUsingInvariantDecimalSeparator()
    {
        var expected = SystemMetricsServiceTests.CreateMetrics();
        var collector = new SystemMetricsServiceTests.StubSystemMetricsCollector(() => expected);
        var service = SystemMetricsServiceTests.CreateService(collector);
        var controller = new MetricsController(service);
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

            var result = controller.Get();

            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("text/plain; version=0.0.4; charset=utf-8", contentResult.ContentType);
            var content = contentResult.Content;
            Assert.IsNotNull(content);
            StringAssert.Contains(content, "# TYPE cpu_usage gauge");
            StringAssert.Contains(content, "cpu_usage 12.34");
            StringAssert.Contains(content, "ram_usage 56.78");
            StringAssert.Contains(content, "disk_usage 90.12");
            Assert.IsFalse(content.Contains("12,34", StringComparison.Ordinal));
            Assert.AreEqual(1, collector.CallCount);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
