using System.Collections.Generic;
//I really wished this could be a class so the AddListener with IEmurable 
//was defined in this class... but atlass. Monobehaviour has to
//take priority unless I create the entire logic trough C# pure code and let listeners  

/// <summary>
/// A event that takes in listeners.
/// </summary>
/// <typeparam name="T">A type of listener</typeparam>
public interface IEvent<T> {

    /// <summary>
    /// Add a list of listener for the event
    /// </summary>
    /// <param name="listeners">The listeners that will substribe to the event</param>
    public void AddListener(IEnumerable<T> listeners);

    /// <summary>
    /// Subscribes a listener to the event
    /// </summary>
    /// <param name="listener">A listener</param>
    public void AddListener(T listener);

    
    /// <summary>
    /// Removes a list of listeners to the evnt
    /// </summary>
    /// <param name="listeners">The listeners</param>
    public void RemoveListener(IEnumerable<T> listeners);

    /// <summary>
    /// Removes a listener from the event
    /// </summary>
    /// <param name="listener">A listener</param>
    public void RemoveListener(T listener);
}