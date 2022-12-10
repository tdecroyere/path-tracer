namespace PathTracer.UI;

public class CommandManager : ICommandManager
{
    // TODO: We need to make sure to avoid reflection here!
    private readonly IDictionary<Type, IList<Delegate>> _handlers;
    private readonly Queue<ICommand> _pendingCommands;

    public CommandManager()
    {
        _handlers = new Dictionary<Type, IList<Delegate>>();
        _pendingCommands = new Queue<ICommand>();
    }

    public void RegisterCommandHandler<T>(Action<T> commandHandler) where T : ICommand
    {
        if (!_handlers.ContainsKey(typeof(T)))
        {
            _handlers.Add(typeof(T), new List<Delegate>());
        }
    
        _handlers[typeof(T)].Add(commandHandler);
    }

    public void SendCommand<T>(T command) where T : ICommand
    {
        _pendingCommands.Enqueue(command);
    }

    public void Update()
    {
        while (_pendingCommands.Count > 0)
        {
            var command = _pendingCommands.Dequeue();

            // TODO: Avoid reflection!
            var commandType = command.GetType();

            if (_handlers.ContainsKey(commandType))
            {
                var handlerList = _handlers[commandType];

                for (var i = 0; i < handlerList.Count; i++)
                {
                    var delegateObject = handlerList[i];
                    delegateObject.DynamicInvoke(command);
                }
            }
        }
    }
}