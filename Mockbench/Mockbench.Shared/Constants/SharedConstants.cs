namespace Mockbench.Shared.Constants;

public class SharedConstants
{
    /// <summary>
    /// Current Mockbench version
    /// </summary>
    public const string MockbenchVersion = "V0.60.00-preview";

    /// <summary>
    /// Prefix for headers sent to mock endpoint (to avoid conflicts and behaviour overlap with Mockbench)
    /// </summary>
    public const string MockHeaderIsolationPrefix = "X-Proxied-";
    
    /// <summary>
    /// Prefix for incoming headers to define they should be applied to Mockbench logic
    /// </summary>
    public const string MockbenchHeaderPrefix = "X-Mockbench-";

    /// <summary>
    /// Key for the new property value on an entity (used in validation)
    /// </summary>
    public const string NewPropertyValueKey = "NewPropertyValue";
}
