﻿using System;
using Leak.Common;
using Leak.Extensions;
using Leak.Networking.Core;
using Leak.Peer.Receiver;
using Leak.Peer.Sender;

namespace Leak.Peer.Coordinator
{
    public class CoordinatorEntry
    {
        public long Identifier;

        public PeerHash Peer;

        public NetworkAddress Remote;

        public NetworkDirection Direction;

        public bool Extensions;

        public Bitfield Bitfield;

        public PeerState State;

        public DateTime Timestamp;

        public MoreContainer More;

        public Receiver.ReceiverService Loopy;

        public SenderService Commy;
    }
}