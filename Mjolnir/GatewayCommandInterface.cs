using Discord;
using Discord.Gateway;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Payload.Events;
using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mjolnir {
    public class GatewayCommandInterface : CommandInterface {

        private MainGateway gateway;
        private long listeningChannelId;

        public GatewayCommandInterface(MainGateway gateway, long listeningChannelId) {
            this.listeningChannelId = listeningChannelId;
            this.gateway = gateway;

            gateway.AddEventHandler(EventType.MessageCreate, (o) => ProcessMessage((Message)o));
        }
    }
}
