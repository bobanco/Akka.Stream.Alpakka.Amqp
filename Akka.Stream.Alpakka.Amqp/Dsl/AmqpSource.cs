﻿using Akka.Streams.Dsl;

namespace Akka.Stream.Alpakka.Amqp.Dsl
{
    public static class AmqpSource
    {
        public static Source<IncomingMessage, NotUsed> Create(IAmqpSourceSettings settings, int bufferSize)
        {
            return Source.FromGraph(new AmqpSourceStage(settings, bufferSize));
        } 
    }
}
