using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Extensions;

/// <summary>
/// Extension methods for endpoint type discovery and attribute checking.
/// </summary>
public static class EndpointExtensions
{
    private static readonly Dictionary<Type, bool> _commandCache = new();
    private static readonly Dictionary<Type, bool> _queryCache = new();
    private static readonly Dictionary<Type, CommandAttribute?> _commandAttributeCache = new();
    private static readonly Dictionary<Type, QueryAttribute?> _queryAttributeCache = new();

    /// <summary>
    /// Determines if the specified endpoint type is marked as a Command.
    /// </summary>
    /// <param name="endpointType">The endpoint type to check.</param>
    /// <returns>True if the endpoint is marked with [Command] attribute; otherwise, false.</returns>
    public static bool IsCommandEndpoint(Type endpointType)
    {
        if (_commandCache.TryGetValue(endpointType, out bool cached))
        {
            return cached;
        }

        bool isCommand = endpointType.GetCustomAttributes(typeof(CommandAttribute), inherit: true).Length > 0;
        _commandCache[endpointType] = isCommand;
        return isCommand;
    }

    /// <summary>
    /// Determines if the specified endpoint type is marked as a Query.
    /// </summary>
    /// <param name="endpointType">The endpoint type to check.</param>
    /// <returns>True if the endpoint is marked with [Query] attribute; otherwise, false.</returns>
    public static bool IsQueryEndpoint(Type endpointType)
    {
        if (_queryCache.TryGetValue(endpointType, out bool cached))
        {
            return cached;
        }

        bool isQuery = endpointType.GetCustomAttributes(typeof(QueryAttribute), inherit: true).Length > 0;
        _queryCache[endpointType] = isQuery;
        return isQuery;
    }

    /// <summary>
    /// Gets the Command attribute from the specified endpoint type, if present.
    /// </summary>
    /// <param name="endpointType">The endpoint type to check.</param>
    /// <returns>The CommandAttribute if present; otherwise, null.</returns>
    public static CommandAttribute? GetCommandAttribute(Type endpointType)
    {
        if (_commandAttributeCache.TryGetValue(endpointType, out CommandAttribute? cached))
        {
            return cached;
        }

        var attribute = endpointType.GetCustomAttributes(typeof(CommandAttribute), inherit: true)
            .FirstOrDefault() as CommandAttribute;
        _commandAttributeCache[endpointType] = attribute;
        return attribute;
    }

    /// <summary>
    /// Gets the Query attribute from the specified endpoint type, if present.
    /// </summary>
    /// <param name="endpointType">The endpoint type to check.</param>
    /// <returns>The QueryAttribute if present; otherwise, null.</returns>
    public static QueryAttribute? GetQueryAttribute(Type endpointType)
    {
        if (_queryAttributeCache.TryGetValue(endpointType, out QueryAttribute? cached))
        {
            return cached;
        }

        var attribute = endpointType.GetCustomAttributes(typeof(QueryAttribute), inherit: true)
            .FirstOrDefault() as QueryAttribute;
        _queryAttributeCache[endpointType] = attribute;
        return attribute;
    }
}

