public enum TriggerMode
{
    /// <summary>
    /// Never run this scenario script.
    /// </summary>
    Never,
    /// <summary>
    /// Run this scenario script only once.
    /// User has to remove save data to run this script again.
    /// </summary>
    Once,
    EveryOnceInScene,
    /// <summary>
    /// Run this scenario script whenever condition is met.
    /// </summary>
    Always
}