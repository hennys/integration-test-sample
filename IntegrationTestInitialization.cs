using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Initialization;
using EPiServer.ServiceLocation;

namespace IntegrationTestSample
{
    [InitializableModule]
    [ModuleDependency(typeof(CmsCoreInitialization))]
    public class IntegrationTestInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            // Configure the database access
            context.Services.Configure<DataAccessOptions>(o =>
            {
                // Set the connection string to point to our temporary database
                o.SetConnectionString(IntegrationTestCollection.ConnectionString);
                // This will make Episerver CMS create it's own schema when the database is empty
                o.CreateDatabaseSchema = true;
            });
        }

        void IInitializableModule.Initialize(InitializationEngine context) { }
        void IInitializableModule.Uninitialize(InitializationEngine context) { }
    }
}
