namespace Republic.Core.Tests.Networking;

using Republic.Core.Networking.Models;
using Republic.Core.Networking.Services;
using Xunit;

public sealed class CabinetMultiplayerServerTests
{
    [Fact]
    public void ConnectPlayer_RegistersClientAndBroadcastsJoin()
    {
        var server = new CabinetMultiplayerServer();

        bool connected = server.ConnectPlayer("client-001", "Minister of Finance");

        Assert.True(connected);
        Assert.Single(server.GetConnectedPlayers());
        Assert.Equal("Minister of Finance", server.GetConnectedPlayers()["client-001"]);
        Assert.Single(server.GetMessageLog());
        Assert.Equal(NetworkMessageType.JoinLobby, server.GetMessageLog()[0].Type);
    }
}
