﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.WebJobs.ServiceBusIntegration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.ServiceBus.Core.Transport;
    using Contexts;
    using Logging;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.ServiceBus;
    using Transports;


    public class ServiceBusAttributeSendTransportProvider :
        ISendTransportProvider
    {
        readonly IBinder _binder;
        readonly ILog _log;
        readonly CancellationToken _cancellationToken;

        public ServiceBusAttributeSendTransportProvider(IBinder binder, ILog log, CancellationToken cancellationToken)
        {
            _binder = binder;
            _log = log;
            _cancellationToken = cancellationToken;
        }

        public async Task<ISendTransport> GetSendTransport(Uri address)
        {
            var queueOrTopicName = address.AbsolutePath.Trim('/');

            var serviceBusQueue = new ServiceBusAttribute(queueOrTopicName,EntityType.Queue);

            IAsyncCollector<Message> collector = await _binder.BindAsync<IAsyncCollector<Message>>(serviceBusQueue, _cancellationToken).ConfigureAwait(false);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Creating Send Transport: {0}", queueOrTopicName);

            var sendEndpointContext = new CollectorMessageSendEndpointContext(queueOrTopicName, _log, collector, _cancellationToken);

            var source = new CollectorSendEndpointContextSource(sendEndpointContext);

            return new ServiceBusSendTransport(source, address);
        }
    }
}