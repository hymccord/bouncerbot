using System.Net;

using BouncerBot.Rest;

namespace BouncerBot.Tests.Rest;

[TestClass]
public class RestExceptionTests
{
    [TestMethod]
    public void CloudflareChallengeException_PreservesStatusAndRayId()
    {
        var exception = new CloudflareChallengeException(
            HttpStatusCode.BadGateway,
            "a3dd37f62bb18b6d");

        Assert.AreEqual(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.AreEqual("a3dd37f62bb18b6d", exception.RayId);
        StringAssert.Contains(exception.Message, "Cloudflare managed challenge");
    }
}
