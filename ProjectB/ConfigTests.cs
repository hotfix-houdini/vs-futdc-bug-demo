using Microsoft.Extensions.Configuration;

namespace ProjectB
{
    public class ConfigTests
    {
        [Test]
        public void ConfigShouldBeFromProjectB()
        {
            // arrange 
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // act
            var actualConfigValue = config.GetSection("configValue").Value;

            // assert
            Assert.That(actualConfigValue, Is.EqualTo("From Project B"));
        }
    }
}