namespace PathTracer.UI;

public interface ICommandManager
{
    void SendCommand<T>(T command) where T : ICommand;
    void RegisterCommandHandler<T>(Action<T> commandHandler) where T : ICommand;

    void Update();
}