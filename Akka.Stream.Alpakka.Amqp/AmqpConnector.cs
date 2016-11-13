using System;
using Akka.Streams;
using Akka.Streams.Stage;
using Akka.Util.Internal;
using RabbitMQ.Client;

namespace Akka.Stream.Alpakka.Amqp
{
    /// <summary>
    /// Internal API
    /// </summary>
    internal class AmqpConnector
    {
        public static IConnectionFactory ConnectionFactoryFrom(IAmqpConnectionSettings settings)
        {
            var factory = new ConnectionFactory();
            if (settings is AmqpConnectionUri)
            {
                var uriSettings = (AmqpConnectionUri) settings;
                factory.Uri = uriSettings.Uri;
            }
            if (settings is AmqpConnectionDetails)
            {
                var connectionDetails = (AmqpConnectionDetails) settings;
                factory.HostName = connectionDetails.Host;
                factory.Port = connectionDetails.Port;
                if (connectionDetails.Credentials != null)
                {
                    factory.UserName = connectionDetails.Credentials.Username;
                    factory.Password = connectionDetails.Credentials.Password;
                }
                if (connectionDetails.VirtualHost != null)
                {
                    factory.VirtualHost = connectionDetails.VirtualHost;
                }
            }
            //DefaultAmqpConnection => // leave it be as is

            return factory;
        }
    }

    /// <summary>
    /// Internal API
    /// </summary>
    internal abstract class AmqpConnectorLogic : GraphStageLogic
    {
        protected IConnection Connection;
        protected IModel Channel;
        protected Action<ShutdownEventArgs> ShutdownCallback;

        protected AmqpConnectorLogic(Shape shape) 
            : base(shape)
        {
            
        }

        public abstract IAmqpConnectorSettings Settings { get; }

        public abstract IConnectionFactory ConnectionFactoryFrom(IAmqpConnectionSettings settings);

        public abstract void WhenConnected();

        public override void PreStart()
        {
            var factory = ConnectionFactoryFrom(Settings.ConnectionSettings);
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            ShutdownCallback = GetAsyncCallback<ShutdownEventArgs>(args =>
            {
                if (args.Initiator != ShutdownInitiator.Application)
                    FailStage(ShutdownSignalException.FromArgs(args));
            });
            Connection.ConnectionShutdown += OnConnectionShutdown;
            Channel.ModelShutdown += OnChannelShutdown;

            Settings.Declarations.ForEach(declaration =>
            {
                if (declaration is QueueDeclaration)
                {
                    var queueDeclaration = (QueueDeclaration) declaration;
                    Channel.QueueDeclare(queueDeclaration.Name, queueDeclaration.Durable, queueDeclaration.Exclusive,
                        queueDeclaration.AutoDelete, queueDeclaration.Arguments);
                }
                else if (declaration is BindingDeclaration)
                {
                    var bindingDeclaration = (BindingDeclaration) declaration;
                    Channel.QueueBind(bindingDeclaration.Queue, bindingDeclaration.Exchange,
                        bindingDeclaration.RoutingKey??"", bindingDeclaration.Arguments);
                }
                else if (declaration is ExchangeDeclaration)
                {
                    var exchangeDeclaration = (ExchangeDeclaration) declaration;
                    Channel.ExchangeDeclare(exchangeDeclaration.Name, exchangeDeclaration.ExchangeType,
                        exchangeDeclaration.Durable, exchangeDeclaration.AutoDelete, exchangeDeclaration.Arguments);
                }
            });

            WhenConnected();
        }

        private void OnChannelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ShutdownCallback?.Invoke(shutdownEventArgs);
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ShutdownCallback?.Invoke(shutdownEventArgs);
        }

        /// <summary>
        /// remember to call if overriding!
        /// </summary>
        public override void PostStop()
        {
            if (Channel != null)
            {
                if(Channel.IsOpen)
                    Channel.Close();
                Channel.ModelShutdown -= OnChannelShutdown;
                Channel = null;
            }
            if (Connection != null)
            {
                if(Connection.IsOpen)
                    Connection.Close();
                Connection.ConnectionShutdown -= OnConnectionShutdown;
                Connection = null;
            }
        }
    }
}
