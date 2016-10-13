﻿using Leak.Core.Collector;
using Leak.Core.Core;
using Leak.Core.Metafile;
using Leak.Core.Metamine;
using System;

namespace Leak.Core.Metaget
{
    public class MetagetContext
    {
        private readonly MetagetConfiguration configuration;
        private readonly LeakQueue<MetagetContext> queue;
        private readonly MetafileService metafile;

        private MetamineBitfield metamine;

        public MetagetContext(Action<MetagetConfiguration> configurer)
        {
            configuration = configurer.Configure(with =>
            {
                with.Callback = new MetagetCallbackNothing();
            });

            metafile = new MetafileService(with =>
            {
                with.Hash = configuration.Hash;
                with.Destination = configuration.Destination + ".metainfo";
                with.Callback = new MetagetMetafile(this);
            });

            queue = new LeakQueue<MetagetContext>(this);
        }

        public MetamineBitfield Metamine
        {
            get { return metamine; }
            set { metamine = value; }
        }

        public MetagetConfiguration Configuration
        {
            get { return configuration; }
        }

        public PeerCollectorView View
        {
            get { return configuration.Collector; }
        }

        public MetagetCallback Callback
        {
            get { return configuration.Callback; }
        }

        public LeakQueue<MetagetContext> Queue
        {
            get { return queue; }
        }

        public MetafileService Metafile
        {
            get { return metafile; }
        }
    }
}