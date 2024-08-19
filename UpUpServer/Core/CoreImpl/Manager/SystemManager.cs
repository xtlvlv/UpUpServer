using System.Collections.Concurrent;

namespace UpUpServer;

public class SystemManager
{
    private HashSet<ISystem> _systems { get; } = new HashSet<ISystem>();
    private Dictionary<Type, ISystem> _systemsIOC = new Dictionary<Type, ISystem>();
    private List<ISystem> _updateSystems = new List<ISystem>();

    public void Update()
    {
        for (int i = 0; i < _updateSystems.Count; i++)
        {
            _updateSystems[i].Update();
        }
    }
    
    public void RegistSystem(Type type, ISystem system)
    {
        system.Init();
        _systems.Add(system);
        _updateSystems.Add(system);
        _systemsIOC.Add(type, system);
    }

    public T? GetSystem<T>() where T : class
    {
        if (_systemsIOC.TryGetValue(typeof(T), out var system))
        {
            return system as T;
        }
        else
        {
            Log.Error($"no system type {typeof(T)}");            
        }

        return null;
    }

    public void Release()
    {
        
    }

    
}