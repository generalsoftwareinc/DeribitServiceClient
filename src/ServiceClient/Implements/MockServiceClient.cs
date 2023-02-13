using ServiceClient.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Implements
{
    internal class MockServiceClient : IServiceClient
    {
        private readonly IDeribitChannelSubscription deribitChannelConnection;

        public MockServiceClient(IDeribitChannelSubscription deribitChannelConnection)
        {
            this.deribitChannelConnection = deribitChannelConnection;
        }

        private readonly IChannelDataTransfer channelDataTransfer;
        public event EventHandler OnTickerReceived;

        public Task<bool> DisconnectAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsDeribitAvailableAsync()
        {
            return Task.FromResult(true);
        }
    }
}
