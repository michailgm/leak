﻿using Leak.Core.Common;
using Leak.Core.Messages;
using Leak.Core.Omnibus;
using System.Collections.Generic;
using System.Linq;

namespace Leak.Core.Retriever
{
    public class ResourceQueueItemRequestSend : ResourceQueueItem
    {
        private readonly PeerHash peer;

        public ResourceQueueItemRequestSend(PeerHash peer)
        {
            this.peer = peer;
        }

        public void Handle(ResourceQueueContext context)
        {
            List<Request> requests = new List<Request>();
            OmnibusStrategy strategy = OmnibusStrategy.Sequential;
            OmnibusBlock[] blocks = context.Omnibus.Next(strategy, peer, 16).ToArray();

            foreach (OmnibusBlock block in blocks)
            {
                requests.Add(new Request(block.Piece, block.Offset, block.Size));
            }

            context.Collector.SendPieceRequest(peer, requests.ToArray());

            foreach (OmnibusBlock block in blocks)
            {
                context.Omnibus.Reserve(peer, block);
            }
        }
    }
}