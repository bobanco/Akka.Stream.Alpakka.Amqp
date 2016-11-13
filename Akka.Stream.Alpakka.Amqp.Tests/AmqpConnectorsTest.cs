using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using Akka.Stream.Alpakka.Amqp.Dsl;
using Akka.Streams;
using Akka.Streams.Dsl;
using Xunit;

namespace Akka.Stream.Alpakka.Amqp.Tests
{
    
    public class AmqpConnectorsTest : IDisposable
    {
        private ActorSystem _system;
        private IMaterializer _materializer;
        public AmqpConnectorsTest()
        {
            _system = ActorSystem.Create(GetType().Name);
            _materializer = _system.Materializer();
            
        }

        [Fact]
        public void PublishAndConsume()
        {
            //queue declaration
            var queueName = "amqp-conn-it-spec-simple-queue-" + Environment.TickCount;
            var queueDeclaration = QueueDeclaration.Create(queueName).WithDurable(false).WithAutoDelete(true);

            //create sink
            var amqpSink = AmqpSink.CreateSimple(
                AmqpSinkSettings.Create(DefaultAmqpConnection.Instance)
                .WithRoutingKey(queueName)
                .WithDeclarations(queueDeclaration));

            //create source
            int bufferSize = 10;
            var amqpSource = AmqpSource.Create(
                NamedQueueSourceSettings.Create(DefaultAmqpConnection.Instance, queueName)
                    .WithDeclarations(queueDeclaration),
                bufferSize);

            //run sink
            var input = new List<string> { "one", "two", "three", "four", "five" };
            Source.From(input).Select(ByteString.FromString).RunWith(amqpSink, _materializer);

            //run source
            var result =
                amqpSource.Select(m => m.Bytes.DecodeString(Encoding.UTF8))
                    .Take(input.Count)
                    .RunWith(Sink.Seq<string>(), _materializer);

            result.Wait(TimeSpan.FromSeconds(3));

            Assert.Equal(input, result.Result);


        }


        public void Dispose()
        {
            _system.Terminate().Wait();
        }
    }
}
