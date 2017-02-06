namespace ApplicationCore
{
    public interface IModule
    {
        string Name { get; }
        // Description of the Module
        string Description { get; }
        // TODO : May be required to add some functions, after the code analysis
        void Run();
    }
}
