using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.DataAbstraction;
using EPiServer.Enterprise;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Xunit;

namespace IntegrationTestSample
{
    [CollectionDefinition(Name)]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestCollectionFixture>
    {
        public const string Name = "IntegrationTest";
        // Generate a "unique" database string for each time we run our tests
        internal static readonly string ConnectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Database=IntegrationTestSample.{Guid.NewGuid().ToString().Substring(0, 8)};Integrated Security=true;MultipleActiveResultSets=True";
    }

    public class IntegrationTestCollectionFixture : IDisposable
    {
        private readonly IDisposable _database;
        private readonly InitializationEngine _engine;

        public IntegrationTestCollectionFixture()
        {
            // Create a temporary database
            _database = DatabaseHelper.Temporary(IntegrationTestCollection.ConnectionString);

            // Initialize Episerver CMS - Including our IntegrationTestInitialization module
            _engine = new InitializationEngine((IEnumerable<IInitializableModule>)null, HostType.TestFramework, new AssemblyList(true).AllowedAssemblies);
            _engine.Initialize();

            // Configure site and import content
            SetupSite(_engine.Locate);
        }

        public void Dispose()
        {
            // Take down the CMS instance
            if (_engine.InitializationState == InitializationState.Initialized)
            {
                _engine.Uninitialize();
            }
            // Removes the temporary database
            _database.Dispose();
        }

        private static void SetupSite(ServiceLocationHelper locate)
        {
            // Ensure languages are enabled
            EnabledLanguages(
                locate.LanguageBranchRepository(),
                new[] { "en", "de" }
            );

            // Import content from our embedded export package
            var startPage = ImportEmbeddedPackage(
                locate.Advanced.GetInstance<IDataImporter>(),
                "IntegrationTestSample.TestPage.episerverdata"
            );

            // Create a site definition for our site
            DefineSite(
                locate.Advanced.GetInstance<ISiteDefinitionRepository>(),
                "TestSite",
                startPage,
                "localhost:8080"
            );
        }

        private static void EnabledLanguages(ILanguageBranchRepository languageRepository, IEnumerable<string> languages)
        {
            if (languages == null || !languages.Any())
                return;

            var shouldDisable = languageRepository.ListEnabled().Where(l => !languages.Contains(l.LanguageID, StringComparer.OrdinalIgnoreCase));

            foreach (var language in languages.Select(x => CultureInfo.GetCultureInfo(x)))
            {
                languageRepository.Enable(language);
            }

            // Disable language secondly to avoid exception thrown if no languages are enabled.
            foreach (var language in shouldDisable)
            {
                languageRepository.Disable(language.Culture);
            }
        }

        private static ContentReference ImportEmbeddedPackage(IDataImporter importer, string embeddedResourceName)
        {
            // Load content package from embedded resources
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName);

            var options = new ImportOptions
            {
                AutoCloseStream = true,
                KeepIdentity = true,
                TransferType = TypeOfTransfer.Importing,
                ValidateDestination = false
            };

            importer.Import(resources, ContentReference.RootPage, options);

            // Root of Imported pages will be our start page
            return importer.Status.ImportedRoot;
        }

        private static void DefineSite(ISiteDefinitionRepository siteDefinitionRepository, string siteName, ContentReference startPage, string siteUrl)
        {
            var existingSite = siteDefinitionRepository.Get(siteName);
            if (existingSite != null) return;

            // Define our site 
            var siteDefinition = new SiteDefinition
            {
                Name = siteName,
                SiteUrl = new Uri($"http://{siteUrl}/", UriKind.Absolute),
                StartPage = startPage
            };
            siteDefinitionRepository.Save(siteDefinition);
        }

    }
}
