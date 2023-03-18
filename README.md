[![openupm](https://img.shields.io/npm/v/net.samurai.observatorium?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.samurai.observatorium/) ![GitHub](https://img.shields.io/github/license/FrostySamurai/Observatorium)

# Observatorium

A simple event system for Unity.

## Limitations

The event system is designed to have only one channel per data type. That means you can't for example have multiple event channels for string. Creating custom data types is advised.

## Event System

Entry point through which it is possible to (un)register to events and raise them. When creating an instance you can pass a folder name in Resource folder that contains scriptable object event channels. Alternatively you can pass assembly with POCO channels or both folder name and assembly.

You will still need to provide access to the event system. Easiest way is to put it into a singleton or create a static instance somewhere (unless you're using a more sophisticated framework).

## Channels

A channel is an object through which the event system raises events for given data type.

Channels can have key type specified. With this, when registering a callback to the event you can specify a key value, and the callback will only be called if the data contains given key.

Callback registration to an event channel returns an IDisposable that can be used to unregister the callback automatically, without using the Event System. Note that this is required to be used for lambda callbacks.

### POCO channels

Event channels that are pure C# classes. Event System creates them if you pass assembly to the systems constructor.

#### Example
```csharp
public struct OnHealthChanged
{
    public int Health;
    public int ChangeAmount;

    public int OnHealthChanged(int health, int changeAmount)
    {
        Health = health;
        ChangeAmount = changeAmount;
    }
}

public class OnHealthChangedChannel : EventChannel<OnHealthChanged> {}
```

#### Keyed Example
```csharp
public struct OnHealthChanged : IEventKeyProvider<int>
{
    public int Id;
    public int Health;
    public int ChangeAmount;

    public int EventKey => Id;

    public int OnHealthChanged(int id, int health, int changeAmount)
    {
        Id = id;
        Health = health;
        ChangeAmount = changeAmount;
    }
}

public class OnHealthChangedChannel : EventChannel<int, OnHealthChanged> {}
```

### Scriptable Object channels

Not everyone is comfortable with using assemblies. Therefore as an alternative it is possible to specify event channels as scriptable objects. These need to be put to a folder within Resource folder with name specified in the constructor of the Event System.

#### Example

For simplicity only keyed example is provided as these are implemented similarily to POCO channels.
```csharp
public struct OnHealthChanged : IEventKeyProvider<int>
{
    public int Id;
    public int Health;
    public int ChangeAmount;

    public int EventKey => Id;

    public int OnHealthChanged(int id, int health, int changeAmount)
    {
        Id = id;
        Health = health;
        ChangeAmount = changeAmount;
    }
}

[CreateAssetMenu(fileName = "OnHealthChangedChannel", menuName = "Events/OnHealthChanged")]
public class OnHealthChangedChannel : ScriptableEventChannel<int, OnHealthChanged> {}
```

## Usage

Once you have the Event System instance created and some event channel specified, you can register and raise your events. This example assumes one of the above keyed example channels was created and the Event System is a static instance in App class.


### Raising Event
```csharp
public class Provider : MonoBehaviour
{
    private void Start()
    {
        var eventData = new OnHealthChanged(1, 10, -5);

        // will raise the event for all matching keyed callbacks and all global callbacks
        App.EventSystem.Raise(eventData);

        // will only raise the event for matching keyed callbacks
        App.EventSystem.Raise<int, OnHealthChanged>(eventData);
    }
}
```

### Event Registration
```csharp
public class Observer : MonoBehaviour
{
    private List<IDisposable> _eventHandles = new();

    private void Awake()
    {
        // this will be called always when OnHealthChanged event is raised
        _eventHandles.Add(App.EventSystem.Register<OnHealthChanged>(LogHeatlhChanged));

        // invalid, given function is already registered so the registration will be ignored and only above callback is used.. returns Empty IDisposable that does nothing
        _eventHandles.Add(App.EventSystem.Register<int, OnHealthChanged>(1, LogHealthChanged));

        // this will only be called if data.Id == 1, only works with keyed event channels
        _eventHandles.Add(App.EventSystem.Register<int, OnHealthChanged>(1, LogHeatlhChangedKeyed));

        // lambdas can be used as well, but need to be unregistered through IDisposable (Unregister function won't work)
        _eventHandles.Add(App.EventSystem.Register<OnHealthChanged>(x => Debug.Log($"Lambda callback for entity with id {x.Id}.")));
    }

    private void OnDestroy()
    {
        _eventHandles.ForEach(x => x.Dispose());
    }

    private void LogHeatlhChanged(OnHealthChanged data)
    {
        Debug.Log($"Health for entity with id {data.Id} changed by {data.ChangeAmount} to {data.Health}.");
    }

    private void LogHeatlhChangedKeyed(OnHealthChanged data)
    {
        Debug.Log($"Health for entity with id {data.Id} changed by {data.ChangeAmount} to {data.Health}.");
    }
}
```

```csharp
// valid, this would also unregister keyed callback, but has more overhead
App.EventSystem.Unregister<OnHealthChanged>(LogHeatlhChanged);

// valid, preferred for keyed callbacks
App.EventSystem.Unregister<int, OnHealthChanged>(1, LogHealthChanged);

// invalid, lambda functions need to be unregistered through IDisposable returned from Register function
App.EventSystem.Unregister<OnHealthChanged>(x => Debug.Log($"Lambda callback for entity with id {x.Id}."));
```

## Debugging

If execution of event callback fails an error log is printed. Log contains information about event data, callback and a stack trace. For more information about event data it is recommended to override data ToString function

```csharp
public override string ToString()
{
    return $"{Id} | {Health} | {ChangeAmount}";
}
```

This doesn't cancel the execution of other callbacks.