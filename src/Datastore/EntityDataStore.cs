namespace Datastore;
public class EntityDataStore
{
    private readonly ContextBase context;
    public EntityDataStore()
    {

        context = new ContextBase();
    }
}
