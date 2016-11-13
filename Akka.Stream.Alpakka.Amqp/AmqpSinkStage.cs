using System;
using Akka.Streams;
using Akka.Streams.Stage;
using RabbitMQ.Client;

namespace Akka.Stream.Alpakka.Amqp
{
    /// <summary>
    /// Connects to an AMQP server upon materialization and sends incoming messages to the server.
    /// Each materialized sink will create one connection to the broker.
    /// </summary>
    public sealed class AmqpSinkStage : GraphStage<SinkShape<OutgoingMessage>>
    {
        public AmqpSinkSettings Settings { get;}

        public static Attributes DefaultAttributes =
            Attributes.CreateName("AmsqpSink")
                .And(ActorAttributes.CreateDispatcher("akka.stream.default-blocking-io-dispatcher"));

        public AmqpSinkStage(AmqpSinkSettings settings)
        {
            Settings = settings;
        }

        public Inlet<OutgoingMessage> In = new Inlet<OutgoingMessage>("AmqpSink.in"); 
        public override SinkShape<OutgoingMessage> Shape => new SinkShape<OutgoingMessage>(In);

        protected override Attributes InitialAttributes => DefaultAttributes;

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes)
        {
            return new AmqpSinkStageLogic(this, Shape);
        }

        private class AmqpSinkStageLogic : AmqpConnectorLogic
        {
            private readonly AmqpSinkStage _stage;
            private Action<ShutdownEventArgs> _shutdownCallback;
            
          
            public AmqpSinkStageLogic(AmqpSinkStage stage, Shape shape) : base(shape)
            {
                _stage = stage;
                SetHandler(_stage.In, () =>
                {
                    var elem = Grab(_stage.In);
                    Channel.BasicPublish(Exchange,
                        RoutingKey,
                        elem.Mandatory,
                        elem.Properties,
                        elem.Bytes.ToArray());
                    Pull(_stage.In);
                });
            }

            public override IAmqpConnectorSettings Settings => _stage.Settings;

            public string Exchange => _stage.Settings.Exchange ?? "";

            public string RoutingKey => _stage.Settings.RoutingKey ?? "";

            public override IConnectionFactory ConnectionFactoryFrom(IAmqpConnectionSettings settings)
            {
                return AmqpConnector.ConnectionFactoryFrom(settings);
            }

            public override void WhenConnected()
            {
                _shutdownCallback = GetAsyncCallback<ShutdownEventArgs>(args =>
                {
                    FailStage(ShutdownSignalException.FromArgs(args));
                });

                Channel.ModelShutdown += OnChannelShutdown;

                Pull(_stage.In);
            }

            private void OnChannelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
            {
                _shutdownCallback?.Invoke(shutdownEventArgs);
            }

            public override string ToString()
            {
                return "AmqpSink";
            }

            public override void PostStop()
            {
                Channel.ModelShutdown -= OnChannelShutdown;
                base.PostStop();//don't forget to call the base.PostStop()
            }
        }
    }
}
