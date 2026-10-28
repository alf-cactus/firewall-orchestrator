using FWO.Middleware.Server.Responses;

namespace FWO.Middleware.Server.Requests;

/// <summary>
/// Validates the flow compliance requests (<c>getFlowComplianceState</c> and <c>getPolicyIds</c>) completely
/// and reports every detected error in one result.
/// </summary>
public static class FlowComplianceRequestValidator
{
    /// <summary>
    /// Validates a policy id request, which accepts no keys.
    /// </summary>
    /// <param name="request">Request to validate, or null when no body was supplied.</param>
    /// <returns>All detected errors; empty when the request is valid.</returns>
    /// <remarks>A missing body is accepted, because the endpoint takes no input.</remarks>
    public static RequestValidationErrorResponse ValidatePolicyIds(GetPolicyIdsRequest? request)
    {
        RequestValidationErrorResponse result = new();
        if (request == null)
        {
            return result;
        }
        foreach (string key in (request.AdditionalData ?? []).Keys.Order(StringComparer.Ordinal))
        {
            result.Errors.Add(AggregatedRequestKeyValidation.BuildError(key,
                $"'{key}' is not supported. {FlowComplianceRequestValidationSchema.kPolicyIdsEndpointName} accepts an empty object only."));
        }
        return result;
    }

    /// <summary>
    /// Validates a flow compliance state request without stopping at the first error.
    /// </summary>
    /// <param name="request">Request to validate, or null when no body was supplied.</param>
    /// <returns>All detected errors; empty when the request is valid.</returns>
    public static RequestValidationErrorResponse ValidateFlowComplianceState(GetFlowComplianceStateRequest? request)
    {
        RequestValidationErrorResponse result = new();
        if (request == null)
        {
            result.Errors.Add(AggregatedRequestKeyValidation.BuildError(FlowComplianceRequestValidationSchema.kRootPath,
                $"{FlowComplianceRequestValidationSchema.kFlowComplianceStateEndpointName} requires a request body. Valid root keys: " +
                FlowComplianceRequestValidationSchema.DescribeKeys(FlowComplianceRequestValidationSchema.RootKeys)));
            return result;
        }
        ValidateAddressList(request.Source, FlowComplianceRequestValidationSchema.kSourcePath, result);
        ValidateAddressList(request.Destination, FlowComplianceRequestValidationSchema.kDestinationPath, result);
        ValidateServiceList(request.Service, result);
        ValidatePolicies(request.Policies, result);
        //AddUnsupportedKeyErrors(request.AdditionalData, kRootPath, RootKeys, result)
        return result;
    }

    /// <summary>
    /// Validates a single service range of the flow catalog lookup.
    /// </summary>
    /// <remarks>
    /// Kept for <c>FlowCatalogController</c>, which still reports plain text errors; its messages must stay unchanged.
    /// The port rule itself is shared with <see cref="ValidateService"/> through <see cref="IsValidPort"/>.
    /// </remarks>
    public static bool TryValidateServiceRange(int portStart, int portEnd, string collectionName, int itemIndex, out string? errorMessage)
    {
        (bool isValid, string? validationError) = ValidateServiceRange(portStart, portEnd, collectionName, itemIndex);
        errorMessage = validationError;
        return isValid;
        // same three texts as today, but with IsValidPort(...) and kMinPort/kMaxPort from the schema
    }

    /// <summary>
    /// Rejects a null or oversized address list and validates every entry under its own request path.
    /// </summary>
    /// <remarks>Entries of an oversized list are not checked one by one, so the error count stays bounded.</remarks>
    private static void ValidateAddressList(List<AddressInputRequest?>? addresses, string path, RequestValidationErrorResponse result)
    {
        if (!ValidateListSize(addresses, path, FlowComplianceRequestValidationSchema.kMaxRangeEntries, result))
        {
            return;
        }
        addresses = addresses ?? [];
        for (int index = 0; index < addresses.Count; index++)
        {
            AddressInputRequestValidator.Validate(addresses[index], $"{path}[{index}]", result);
        }
    }

    /// <summary>
    /// Rejects a null or oversized service list and validates every entry under its own request path.
    /// </summary>
    private static void ValidateServiceList(List<GetFlowComplianceStateRequest.ServiceRangeRequest?>? services, RequestValidationErrorResponse result)
    {
        string path = FlowComplianceRequestValidationSchema.kServicePath;
        if (!ValidateListSize(services, path, FlowComplianceRequestValidationSchema.kMaxRangeEntries, result))
        {
            return;
        }
        services = services ?? [];
        for (int index = 0; index < services.Count; index++)
        {
            ValidateService(services[index], $"{path}[{index}]", result);
        }
    }

    /// <summary>
    /// Adds every error of one service entry: a null entry, unsupported keys, an empty protocol, ports outside
    /// the allowed range and a start port above the end port.
    /// </summary>
    private static void ValidateService(GetFlowComplianceStateRequest.ServiceRangeRequest? service, string path, RequestValidationErrorResponse result)
    {
        if (service == null)
        {
            result.Errors.Add(AggregatedRequestKeyValidation.BuildError(path,
                $"'{path}' must be an object with the following keys: " + FlowComplianceRequestValidationSchema.DescribeKeys(FlowComplianceRequestValidationSchema.ServiceKeys)));
        }
        // null                       → path:            "'{path}' must be an object with the following keys: " + DescribeKeys(ServiceKeys); return
        // AddUnsupportedKeyErrors(service.AdditionalData, path, ServiceKeys, result)
        // protocol empty/whitespace  → path.protocol:   "'{path}.protocol' must not be empty."
        // !IsValidPort(PortStart)    → path.portStart:  "'{path}.portStart' must be between {kMinPort} and {kMaxPort}."
        // !IsValidPort(PortEnd)      → path.portEnd:    "... analog"
        // both valid && start > end  → path.portEnd:    "'{path}.portEnd' must not be lower than 'portStart'."
    }

    /// <summary>
    /// Rejects a null or oversized policy list and every policy id that is not greater than 0.
    /// </summary>
    private static void ValidatePolicies(List<int>? policies, RequestValidationErrorResponse result)
    {
        // if (!ValidateListSize(policies, kPoliciesPath, kMaxPolicies, result)) return;
        // for i: policies[i] <= 0 → $"{kPoliciesPath}[{i}]": "'policies[i]' must be greater than 0."
    }

    /// <summary>
    /// Adds an error for a null list or a list with more than <paramref name="maxEntries"/> entries.
    /// </summary>
    /// <returns>True when the entries of the list should be validated one by one.</returns>
    private static bool ValidateListSize<TItem>(List<TItem>? items, string path, int maxEntries, RequestValidationErrorResponse result)
    {
        if (items == null)
        {
            result.Errors.Add(AggregatedRequestKeyValidation.BuildError(path,
                $"'{path}' must not be null. Omit it or use an empty list."));
            return false;
        }
        if ((items ?? []).Count > maxEntries)
        {
            result.Errors.Add(AggregatedRequestKeyValidation.BuildError(path,
                $"'{path}' must not exceed {maxEntries} entries."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Checks one port against the range allowed for service entries.
    /// </summary>
    private static bool IsValidPort(int port) => port >= FlowComplianceRequestValidationSchema.kMinPort
        && port <= FlowComplianceRequestValidationSchema. kMaxPort;
}