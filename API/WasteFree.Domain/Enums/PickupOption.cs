namespace WasteFree.Domain.Enums;

/// <summary>
/// Pickup option, entry point for garbage order flow.
/// </summary>
public enum PickupOption
{
    /// <summary>
    /// Small pickup, by i.e. small car
    /// </summary>
    SmallPickup = 0,
    
    /// <summary>
    /// Regular pickup by bigger cars 
    /// </summary>
    Pickup = 1,
    
    /// <summary>
    /// Container to collect rubbish and be collected on selected date
    /// </summary>
    Container = 2,
    
    /// <summary>
    /// Special order with special flow
    /// </summary>
    SpecialOrder = 3
}