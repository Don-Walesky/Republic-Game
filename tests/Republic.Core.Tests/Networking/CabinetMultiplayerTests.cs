namespace Republic.Core.Tests.Networking;

using System.Linq;
using Xunit;
using Republic.Core.Networking.Models;
using Republic.Core.Networking.Services;

public class CabinetMultiplayerTests
{
    [Fact]
    public void ConnectPlayer_Registers_Client_And_Broadcasts_Join_Message()
    {
        var server = new CabinetMultiplayerServer();
        var client = new CabinetMultiplayerClient("client-01", "Minister of Defense", server);

        bool connected = client.Connect();

        Assert.True(connected);
        Assert.True(client.IsConnected);
        Assert.Single(server.GetConnectedPlayers());
        Assert.Equal("Minister of Defense", server.GetConnectedPlayers()["client-01"]);
    }

    [Fact]
    public void ProposeDirective_And_VoteOnDirective_Broadcasts_NetworkMessages()
    {
        var server = new CabinetMultiplayerServer();
        var president = new CabinetMultiplayerClient("client-pres", "President", server);
        var defense = new CabinetMultiplayerClient("client-def", "Minister of Defense", server);

        president.Connect();
        defense.Connect();

        president.ProposeDirective("Mobilize Frontier Garrison", "Deploy 2,000 personnel");
        defense.VoteOnDirective("dir-101", true);

        var logs = server.GetMessageLog();

        Assert.Equal(4, logs.Count); // 2 joins + 1 proposal + 1 vote
        Assert.Contains(logs, m => m.Type == NetworkMessageType.ProposeDirective && m.SenderRole == "President");
        Assert.Contains(logs, m => m.Type == NetworkMessageType.VoteOnDirective && m.SenderRole == "Minister of Defense");
    }
}
