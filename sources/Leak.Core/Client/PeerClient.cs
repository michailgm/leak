﻿using Leak.Core.Collector;
using Leak.Core.Common;
using Leak.Core.Connector;
using Leak.Core.Listener;
using Leak.Core.Messages;
using Leak.Core.Metadata;
using Leak.Core.Network;
using Leak.Core.Repository;
using Leak.Core.Retriever;
using Leak.Core.Telegraph;
using System;
using System.Collections.Generic;

namespace Leak.Core.Client
{
    public class PeerClient : PeerClientExtensionContext
    {
        private readonly PeerCollector collector;
        private readonly PeerClientStorage storage;
        private readonly PeerClientConfiguration configuration;
        private readonly PeerClientCallback callback;
        private readonly PeerListener listener;
        private readonly FileHashCollection hashes;
        private readonly NetworkPool pool;

        public PeerClient(Action<PeerClientConfiguration> configurer)
        {
            configuration = configurer.Configure(with =>
            {
                with.Peer = PeerHash.Random();
                with.Destination = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
                with.Callback = new PeerClientCallbackNothing();
                with.Extensions = new PeerClientExtensionBuilder();
                with.Connector = new PeerClientConnectorBuilder();
                with.Listener = new PeerClientListenerBuilder();
            });

            storage = new PeerClientStorage(configuration, this);
            hashes = new FileHashCollection();
            callback = configuration.Callback;

            collector = new PeerCollector(with =>
            {
                with.Callback = new PeerClientToCollector(configuration, storage);
            });

            pool = new NetworkPool(with =>
            {
                with.Callback = collector.CreatePoolCallback();
            });

            if (configuration.Listener.Status == PeerClientListenerStatus.On)
            {
                listener = configuration.Listener.Build(with =>
                {
                    with.Callback = collector.CreateListenerCallback();
                    with.Peer = configuration.Peer;
                    with.Extensions = true;
                    with.Hashes = hashes;
                    with.Pool = pool;
                });

                listener.Start();
            }
        }

        public void Start(MetainfoFile metainfo)
        {
            Register(metainfo.Data);
            Schedule(metainfo.Data, metainfo.Trackers, new PeerAddress[0]);
        }

        public void Start(Action<PeerClientStartConfiguration> configurer)
        {
            Start(configurer.Configure(with =>
            {
                with.Trackers = new List<string>();
                with.Peers = new List<PeerAddress>();
            }));
        }

        private void Start(PeerClientStartConfiguration start)
        {
            string location = configuration.Destination;
            Metainfo metainfo = ResourceRepositoryToHash.Open(location, start.Hash);

            if (metainfo == null)
            {
                Register(start);
                Schedule(start);
            }
            else
            {
                Register(metainfo);
                Schedule(metainfo, start.Trackers.ToArray(), start.Peers.ToArray());
            }
        }

        private void Register(Metainfo metainfo)
        {
            storage.Register(metainfo, collector.CreateView());

            FileHash hash = metainfo.Hash;
            ResourceRepository repository = storage.GetRepository(hash);
            Bitfield bitfield = repository.Initialize();

            storage.WithBitfield(hash, bitfield);
            callback.OnInitialized(metainfo.Hash, new PeerClientMetainfo(bitfield));
            hashes.Add(metainfo.Hash);

            if (bitfield.Completed == bitfield.Length)
            {
                callback.OnCompleted(metainfo.Hash);
            }
        }

        private void Register(PeerClientStartConfiguration start)
        {
            storage.Register(start.Hash, collector.CreateView());
            hashes.Add(start.Hash);
        }

        private void Schedule(Metainfo metainfo, string[] trackers, PeerAddress[] peers)
        {
            PeerConnector connector = null;

            if (configuration.Connector.Status == PeerClientListenerStatus.On)
            {
                connector = configuration.Connector.Build(with =>
                {
                    with.Peer = configuration.Peer;
                    with.Hash = metainfo.Hash;
                    with.Callback = collector.CreateConnectorCallback();
                    with.Pool = pool;
                });

                foreach (PeerAddress peer in peers)
                {
                    if (storage.Contains(peer) == false)
                    {
                        callback.OnPeerConnecting(metainfo.Hash, peer);
                        connector.ConnectTo(peer);
                    }
                }
            }

            TrackerTelegraph telegraph = new TrackerTelegraph(with =>
            {
                if (connector != null)
                {
                    with.Callback = new PeerClientTelegraphConnect(configuration, metainfo.Hash, connector, storage);
                }
            });

            foreach (string tracker in trackers)
            {
                telegraph.Start(tracker, with =>
                {
                    with.Peer = configuration.Peer;
                    with.Hash = metainfo.Hash;
                });
            }
        }

        private void Schedule(PeerClientStartConfiguration start)
        {
            PeerConnector connector = null;

            if (configuration.Connector.Status == PeerClientListenerStatus.On)
            {
                connector = configuration.Connector.Build(with =>
                {
                    with.Hash = start.Hash;
                    with.Peer = configuration.Peer;
                    with.Callback = collector.CreateConnectorCallback();
                    with.Extensions = true;
                    with.Pool = pool;
                });
            }

            TrackerTelegraph telegraph = new TrackerTelegraph(with =>
            {
                if (connector != null)
                {
                    with.Callback = new PeerClientTelegraphConnect(configuration, start.Hash, connector, storage);
                }
            });

            foreach (string tracker in start.Trackers)
            {
                telegraph.Start(tracker, with =>
                {
                    with.Peer = configuration.Peer;
                    with.Hash = start.Hash;
                });
            }
        }

        FileHash PeerClientExtensionContext.GetHash(PeerHash peer)
        {
            return storage.GetHash(peer);
        }

        ResourceRetriever PeerClientExtensionContext.GetRetriever(PeerHash peer)
        {
            return storage.GetRetriever(peer);
        }

        PeerClientCallback PeerClientExtensionContext.GetCallback(PeerHash peer)
        {
            return storage.GetCallback(peer);
        }

        PeerConnector PeerClientExtensionContext.GetConnector(PeerHash peer)
        {
            return configuration.Connector.Build(with =>
            {
                with.Peer = configuration.Peer;
                with.Hash = storage.GetHash(peer);
                with.Callback = collector.CreateConnectorCallback();
                with.Pool = pool;
            });
        }

        bool PeerClientExtensionContext.IsConnected(PeerAddress remote)
        {
            return storage.Contains(remote);
        }
    }
}