﻿using Leak.Core.Common;
using System;
using System.Linq;

namespace Leak.Core.Omnibus
{
    /// <summary>
    /// Manages the global bitfield for a given FileHash. It manages all peers
    /// which announced their bitfield, tracks all requested pieces/blocks and
    /// monitors completed pieces.
    /// </summary>
    public class OmnibusService
    {
        private readonly OmnibusContext context;

        public OmnibusService(Action<OmnibusConfiguration> configurer)
        {
            context = new OmnibusContext(configurer);
        }

        /// <summary>
        /// Registers a bitfield belonging to the peer.
        /// </summary>
        /// <param name="peer">The peer affected by a bitfield.</param>
        /// <param name="bitfield">The bitfield of requested hash.</param>
        public void Add(PeerHash peer, Bitfield bitfield)
        {
            lock (context.Synchronized)
            {
                context.Bitfields.Add(peer, bitfield);
            }
        }

        /// <summary>
        /// Reports completeness of the received block.
        /// </summary>
        /// <param name="block">The received block structure.</param>
        /// <returns>The value indicated whether the block completed also the piece.</returns>
        public bool Complete(OmnibusBlock block)
        {
            lock (context.Synchronized)
            {
                context.Reservations.Complete(block);

                return context.Pieces.Complete(block.Piece, block.Offset / context.Metainfo.Properties.BlockSize);
            }
        }

        /// <summary>
        /// Invalidates the entire piece.
        /// </summary>
        /// <param name="piece">The piece index to invalidate.</param>
        public void Invalidate(int piece)
        {
            lock (context.Synchronized)
            {
                context.Pieces.Invalidate(piece);
            }
        }

        public bool IsComplete()
        {
            lock (context.Synchronized)
            {
                return context.Pieces.IsComplete();
            }
        }

        public bool IsComplete(int piece)
        {
            lock (context.Synchronized)
            {
                return context.Pieces.IsComplete(piece);
            }
        }

        public OmnibusBlock[] Next(OmnibusStrategy strategy, PeerHash peer, int count)
        {
            lock (context.Synchronized)
            {
                if (context.Bitfields.Contains(peer))
                {
                    return strategy.Next(context, peer, count).ToArray();
                }
            }

            return new OmnibusBlock[0];
        }

        /// <summary>
        /// Reserves the block to be downloaded by the given peer.
        /// </summary>
        /// <param name="peer">The peer which reserves the block.</param>
        /// <param name="request">The block structure describing the reservation.</param>
        /// <returns>The optional peer which had assigned reservation.</returns>
        public PeerHash Reserve(PeerHash peer, OmnibusBlock request)
        {
            lock (context.Synchronized)
            {
                return context.Reservations.Add(peer, request);
            }
        }
    }
}