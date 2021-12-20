
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CodeGenerator.Test;
public class EFBuilderWork
{
    [Fact]
    public void Test1()
    {
        var serviceCollection = new ServiceCollection();
        //serviceCollection.AddDbContext<TestDbContext>(d => d.UseInMemoryDatabase("test"));

        serviceCollection.AddEntityFrameworkInMemoryDatabase();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(serviceProvider);
        optionsBuilder.UseInMemoryDatabase("test");
        var dbcontext = new DbContext(optionsBuilder.Options);

        var contextServices = ((IInfrastructure<IServiceProvider>)dbcontext).Instance;

        //serviceCollection.AddDbContext<TestDbContext>(d => d.UseInMemoryDatabase("test"));

        //var serviceBuilder = new EntityFrameworkServicesBuilder(serviceCollection);
        //serviceBuilder.TryAddProviderSpecificServices(b => b.TryAddScoped(typeof(TestDbContext), typeof(TestDbContext)));
        //serviceBuilder.TryAddCoreServices();


        var modelCreationDependencies = contextServices.GetRequiredService<ModelCreationDependencies>();

        var conventions = modelCreationDependencies.ConventionSetBuilder.CreateConventionSet();
        var builder = new ModelBuilder(conventions,
            modelCreationDependencies.ModelDependencies,
            new ModelConfiguration());

        builder.Entity<Comments>();
        var entityType = builder.Model.FindEntityType(typeof(Comments));
        var content = entityType.FindProperty(nameof(Comments.Content));
    }
}
