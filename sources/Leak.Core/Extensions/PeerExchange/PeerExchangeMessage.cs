﻿using Leak.Core.Common;

namespace Leak.Core.Extensions.PeerExchange
{
    public class PeerExchangeMessage
    {
        private readonly PeerAddress[] added;

        public PeerExchangeMessage(PeerAddress[] added)
        {
            this.added = added;
        }

        public PeerAddress[] Added
        {
            get { return added; }
        }
    }
}