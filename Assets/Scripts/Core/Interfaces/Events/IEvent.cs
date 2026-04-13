//I really wished this could be a class so the AddListener with IEmurable 
//was defined in this class... but atlass. Monobehaviour has to
//take priority unless I create the entire logic trough C# pure code and let listeners  

/// <summary>
/// A event that takes in listeners.
/// </summary>
/// <typeparam name="T">A type of listener</typeparam>
public interface IEvent<T> {

    /// <summary>
    /// Subscribes a listener to the event
    /// </summary>
    /// <param name="listener">A listener</param>
    public void AddListener(T listener);

    /// <summary>
    /// Removes a listener from the event
    /// </summary>
    /// <param name="listener">A listener</param>
    public void RemoveListener(T listener);
}