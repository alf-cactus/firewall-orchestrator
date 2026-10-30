using System.Text.RegularExpressions;
using FWO.Api.Client;
using FWO.Api.Client.Queries;
using FWO.Data;
using FWO.Services;
using NUnit.Framework;

namespace FWO.Test;

/// <summary>
/// Tests that the routing based path analysis passes the source and destination of a path to the matching
/// arguments of the <c>devices_in_path</c> database function.
/// </summary>
[TestFixture]
internal class NetworkAnalysisQueriesTest
{
    /// <summary>Verifies the query maps <c>$src</c> to <c>c_source</c> and <c>$dst</c> to <c>c_destination</c>.</summary>
    [Test]
    public void PathAnalysisQuery_MapsSourceAndDestinationToMatchingArguments()
    {
        string query = Regex.Replace(NetworkAnalysisQueries.pathAnalysis, @"\s+", string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(query, Does.Contain("c_source:$src"));
            Assert.That(query, Does.Contain("c_destination:$dst"));
        });
    }

    /// <summary>
    /// Verifies the analyzer sends the first address of the source as <c>src</c> and the first address of the
    /// destination as <c>dst</c>, and returns the names of the devices found.
    /// </summary>
    [Test]
    public async Task GetDeviceNamesForSinglePath_SendsSourceAsSrcAndDestinationAsDst()
    {
        PathAnalysisApiConn apiConnection = new();

        string deviceNames = await RoutingBasedPathAnalyzer.GetDeviceNamesForSinglePath(
            "10.1.0.0/24", "10.2.0.5", apiConnection);

        Assert.Multiple(() =>
        {
            Assert.That(apiConnection.SentQuery, Is.EqualTo(NetworkAnalysisQueries.pathAnalysis));
            Assert.That(apiConnection.SentSource, Is.EqualTo("10.1.0.0"));
            Assert.That(apiConnection.SentDestination, Is.EqualTo("10.2.0.5"));
            Assert.That(deviceNames, Is.EqualTo("fw-core, fw-edge"));
        });
    }

    private sealed class PathAnalysisApiConn : SimulatedApiConnection
    {
        public string? SentQuery { get; private set; }

        public string? SentSource { get; private set; }

        public string? SentDestination { get; private set; }

        public override Task<QueryResponseType> SendQueryAsync<QueryResponseType>(string query, object? variables = null,
            string? operationName = null, QueryChunkingOptions? chunkingOptions = null)
        {
            SentQuery = query;
            SentSource = variables?.GetType().GetProperty("src")?.GetValue(variables)?.ToString();
            SentDestination = variables?.GetType().GetProperty("dst")?.GetValue(variables)?.ToString();
            List<Device> devices =
            [
                new Device { Id = 1, Name = "fw-core" },
                new Device { Id = 2, Name = "fw-edge" }
            ];
            return Task.FromResult((QueryResponseType)(object)devices);
        }
    }
}
