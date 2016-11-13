using Akka.IO;
using Akka.Streams.Dsl;

namespace Akka.Stream.Alpakka.Amqp.Dsl
{
    public static class AmqpSink
    {
        /// <summary>
        /// Creates an <see cref="AmqpSink"/> that accepts <see cref="OutgoingMessage"/> elements.
        /// </summary>
        /// <param name="settings">The sink settings</param>
        /// <returns>an <see cref="AmqpSink"/> that accepts <see cref="OutgoingMessage"/> elements.</returns>
        public static Sink<OutgoingMessage, NotUsed> Create(AmqpSinkSettings settings)
        {
            return Sink.FromGraph(new AmqpSinkStage(settings));
        }

        /// <summary>
        /// Creates an <see cref="AmqpSink"/> that accepts <see cref="ByteString"/> elements.
        /// </summary>
        /// <param name="settings">the sink settings</param>
        /// <returns>an <see cref="AmqpSink"/> that accepts <see cref="ByteString"/> elements.</returns>
        public static Sink<ByteString, NotUsed> CreateSimple(AmqpSinkSettings settings)
        {
            return Create(settings).ContraMap<ByteString>(bytes => new OutgoingMessage(bytes, false, false));
        }
    }
}
