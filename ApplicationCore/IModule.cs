namespace ApplicationCore
{
    public interface IModule
    {
        // Name of the Module
        string Name { get; }
        // Description of the Module
        string Description { get; }
        // Run task on each module
        void Run();
    }
}
