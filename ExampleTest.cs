using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using Xunit;

namespace IntegrationTestSample
{
    [Collection(IntegrationTestCollection.Name)]
    public class ExampleTest
    {
        [Fact]
        public void RootPageChildren_ShouldContainTestPage()
        {
            const string pageName = "TestPage";

            var repository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var children = repository.GetChildren<IContent>(ContentReference.RootPage);

            Assert.Contains(children, x => x.Name == pageName);
        }
    }

    [ContentType(GUID = "689b85b1-0dc9-4cd7-8363-29aa61757c1a")]
    public class TestData : PageData { }
}
