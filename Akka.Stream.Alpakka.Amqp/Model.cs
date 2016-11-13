using System.Collections.Generic;
using Akka.Util.Internal;

namespace Akka.Stream.Alpakka.Amqp
{

    public interface IAmqpConnectorSettings
    {
        IAmqpConnectionSettings ConnectionSettings { get; }

        IList<IDeclaration> Declarations { get; }
    }

    public interface IAmqpSourceSettings : IAmqpConnectorSettings
    {
    }

    public sealed class NamedQueueSourceSettings : IAmqpSourceSettings
    {
        private NamedQueueSourceSettings(IAmqpConnectionSettings connectionSettings, string queue,
            IList<IDeclaration> declarations = null, bool noLocal = false, bool exclusive = false,
            string consumerTag = null,
            IDictionary<string, object> arguments = null)
        {
            ConnectionSettings = connectionSettings;
            Queue = queue;
            Declarations = declarations ?? new List<IDeclaration>();
            NoLocal = noLocal;
            Exclusive = exclusive;
            ConsumerTag = consumerTag ?? "default";
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        public IAmqpConnectionSettings ConnectionSettings { get; }
        public string Queue { get; }
        public IList<IDeclaration> Declarations { get; }
        public bool NoLocal { get; }

        public bool Exclusive { get; }

        public string ConsumerTag { get; }

        public IDictionary<string, object> Arguments { get; }

        public static NamedQueueSourceSettings Create(IAmqpConnectionSettings connectionSettings, string queue)
        {
            return new NamedQueueSourceSettings(connectionSettings, queue);
        }

        public NamedQueueSourceSettings WithDeclarations(params IDeclaration[] declarations)
        {
            declarations.ForEach(Declarations.Add);
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, NoLocal, Exclusive, ConsumerTag,
                Arguments);
        }

        public NamedQueueSourceSettings WithNoLocal(bool noLocal)
        {
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, noLocal, Exclusive, ConsumerTag,
                Arguments);
        }

        public NamedQueueSourceSettings WithExclusive(bool exclusive)
        {
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, NoLocal, exclusive, ConsumerTag,
                Arguments);
        }

        public NamedQueueSourceSettings WithConsumerTag(string consumerTag)
        {
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, NoLocal, Exclusive, consumerTag,
                Arguments);
        }

        public NamedQueueSourceSettings WithArguments(params KeyValuePair<string, object>[] arguments)
        {
            arguments.ForEach(Arguments.Add);
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, NoLocal, Exclusive, ConsumerTag,
                Arguments);
        }

        public NamedQueueSourceSettings WithArguments(string key, object value)
        {
            Arguments.Add(key, value);
            return new NamedQueueSourceSettings(ConnectionSettings, Queue, Declarations, NoLocal, Exclusive, ConsumerTag,
                Arguments);
        }

        public override string ToString()
        {
            return
                $"NamedQueueSourceSettings(ConnectionSettings={ConnectionSettings}, Queue={Queue}, Declarations={Declarations.Count}, NoLocal={NoLocal}, Exclusive={Exclusive}, Arguments={Arguments.Count})";
        }
    }

    public sealed class TemporaryQueueSourceSettings : IAmqpSourceSettings
    {
        private TemporaryQueueSourceSettings(IAmqpConnectionSettings connectionSettings, string exchange,
            IList<IDeclaration> declarations = null, string routingKey = null)
        {
            ConnectionSettings = connectionSettings;
            Exchange = exchange;
            Declarations = declarations ?? new List<IDeclaration>();
            RoutingKey = routingKey;
        }

        public IAmqpConnectionSettings ConnectionSettings { get; }
        public string Exchange { get; }
        public IList<IDeclaration> Declarations { get; }
        public string RoutingKey { get; }

        public static TemporaryQueueSourceSettings Create(IAmqpConnectionSettings connectionSettings, string exchange)
        {
            return new TemporaryQueueSourceSettings(connectionSettings, exchange);
        }

        public TemporaryQueueSourceSettings WithRoutingKey(string routingKey)
        {
            return new TemporaryQueueSourceSettings(ConnectionSettings, Exchange, Declarations, routingKey);
        }

        public TemporaryQueueSourceSettings WithDeclarations(params IDeclaration[] declarations)
        {
            declarations.ForEach(Declarations.Add);
            return new TemporaryQueueSourceSettings(ConnectionSettings, Exchange, Declarations, RoutingKey);
        }

        public override string ToString()
        {
            return
                $"TemporaryQueueSourceSettings(ConnectionSettings={ConnectionSettings},Exchange={Exchange}, Declarations={Declarations.Count}, RoutingKey={RoutingKey})";
        }
    }

    public sealed class AmqpSinkSettings : IAmqpConnectorSettings
    {
        private AmqpSinkSettings(IAmqpConnectionSettings connectionSettings, string exchange = null,
            string routingKey = null, IList<IDeclaration> declarations = null)
        {
            ConnectionSettings = connectionSettings;
            Exchange = exchange;
            RoutingKey = routingKey;
            Declarations = declarations ?? new List<IDeclaration>();
        }

        public IAmqpConnectionSettings ConnectionSettings { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }
        public IList<IDeclaration> Declarations { get; }

        public static AmqpSinkSettings Create(IAmqpConnectionSettings connectionSettings)
        {
            return new AmqpSinkSettings(connectionSettings);
        }

        public AmqpSinkSettings WithExchange(string exchange)
        {
            return new AmqpSinkSettings(ConnectionSettings, exchange, RoutingKey, Declarations);
        }

        public AmqpSinkSettings WithRoutingKey(string routingKey)
        {
            return new AmqpSinkSettings(ConnectionSettings, Exchange, routingKey, Declarations);
        }

        public AmqpSinkSettings WithDeclarations(params IDeclaration[] declarations)
        {
            declarations.ForEach(Declarations.Add);
            return new AmqpSinkSettings(ConnectionSettings, Exchange, RoutingKey, Declarations);
        }

        public override string ToString()
        {

            return
                $"AmqpSinkSettings(ConnectionSettings={ConnectionSettings}, Exchange={Exchange}, RoutingKey={RoutingKey}, Delcarations={Declarations.Count})";
        }
    }

    /// <summary>
    /// Only for internal implementations
    /// </summary>
    public interface IAmqpConnectionSettings
    {
    }

    /// <summary>
    /// Connects to a local AMQP broker at the default port with no password.
    /// </summary>
    public class DefaultAmqpConnection : IAmqpConnectionSettings
    {
        public static IAmqpConnectionSettings Instance => new DefaultAmqpConnection();
    }

    public sealed class AmqpConnectionUri : IAmqpConnectionSettings
    {
        private AmqpConnectionUri(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; }

        public AmqpConnectionUri Create(string uri)
        {
            return new AmqpConnectionUri(uri);
        }

        public override string ToString()
        {
            return $"AmqpConnectionUri(Uri={Uri})";
        }
    }

    public sealed class AmqpConnectionDetails : IAmqpConnectionSettings
    {
        private AmqpConnectionDetails(string host, int port, AmqpCredentials credentials = null,
            string virtualHost = null)
        {
            Host = host;
            Port = port;
            Credentials = credentials;
            VirtualHost = virtualHost;
        }

        public string Host { get; }
        public int Port { get; }
        public AmqpCredentials Credentials { get; }
        public string VirtualHost { get; }

        public static AmqpConnectionDetails Create(string host, int port)
        {
            return new AmqpConnectionDetails(host, port);
        }

        public static AmqpConnectionDetails Create(string host, int port, AmqpCredentials credentials)
        {
            return new AmqpConnectionDetails(host, port, credentials);
        }

        public static AmqpConnectionDetails Create(string host, int port, AmqpCredentials credentials,
            string virtualHost)
        {
            return new AmqpConnectionDetails(host, port, credentials, virtualHost);
        }

        public static AmqpConnectionDetails Create(string host, int port, string virtualHost)
        {
            return new AmqpConnectionDetails(host, port, null, virtualHost);
        }

        public override string ToString()
        {
            return
                $"AmqpConnectionDetails(Host={Host}, Port={Port}, Credentials={Credentials}, VirtualHost={VirtualHost})";
        }
    }

    public sealed class AmqpCredentials
    {
        public string Username { get; }
        public string Password { get; }

        private AmqpCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public static AmqpCredentials Create(string username, string password)
        {
            return new AmqpCredentials(username, password);
        }

        public override string ToString()
        {
            return $"AmqpCredentials(Username={Username}, Password=********)";
        }
    }

    public interface IDeclaration
    {
    }

    public sealed class QueueDeclaration : IDeclaration
    {

        private QueueDeclaration(string name, bool durable = false, bool exclusive = false, bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            Name = name;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        public string Name { get; }

        public bool Durable { get; }

        public bool Exclusive { get; }

        public bool AutoDelete { get; }

        public IDictionary<string, object> Arguments { get; }

        public static QueueDeclaration Create(string name)
        {
            return new QueueDeclaration(name);
        }

        public QueueDeclaration WithDurable(bool durable)
        {
            return new QueueDeclaration(Name, durable, Exclusive, AutoDelete, Arguments);
        }

        public QueueDeclaration WithExclusive(bool exclusive)
        {
            return new QueueDeclaration(Name, Durable, exclusive, AutoDelete, Arguments);
        }

        public QueueDeclaration WithAutoDelete(bool autoDelete)
        {
            return new QueueDeclaration(Name, Durable, Exclusive, autoDelete, Arguments);
        }

        public QueueDeclaration WithArguments(KeyValuePair<string, object> argument)
        {
            Arguments.Add(argument);
            return new QueueDeclaration(Name, Durable, Exclusive, AutoDelete, Arguments);
        }

        public QueueDeclaration WithArguments(string key, object value)
        {
            Arguments.Add(key, value);
            return new QueueDeclaration(Name, Durable, Exclusive, AutoDelete, Arguments);
        }

        public QueueDeclaration WithArguments(params KeyValuePair<string, object>[] arguments)
        {
            arguments.ForEach(Arguments.Add);
            return new QueueDeclaration(Name, Durable, Exclusive, AutoDelete, Arguments);
        }

        public override string ToString()
        {
            return
                $"QueueDeclaration(Name={Name}, Durable={Durable}, Exclusive={Exclusive}, AutoDelete={AutoDelete}, Arguments={Arguments.Count})";
        }
    }

    public sealed class BindingDeclaration : IDeclaration
    {
        private BindingDeclaration(string queue, string exchange, string routingKey = null,
            IDictionary<string, object> arguments = null)
        {
            Queue = queue;
            Exchange = exchange;
            RoutingKey = routingKey;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        public string Queue { get; }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public IDictionary<string, object> Arguments { get; }

        public static BindingDeclaration Create(string queue, string exchange)
        {
            return new BindingDeclaration(queue, exchange);
        }

        public BindingDeclaration WithRoutingKey(string routingKey)
        {
            return new BindingDeclaration(Queue, Exchange, routingKey, Arguments);
        }

        public BindingDeclaration WithArguments(string key, object value)
        {
            Arguments.Add(key, value);
            return new BindingDeclaration(Queue, Exchange, RoutingKey, Arguments);
        }

        public BindingDeclaration WithArguments(KeyValuePair<string, object> argument)
        {
            Arguments.Add(argument);
            return new BindingDeclaration(Queue, Exchange, RoutingKey, Arguments);
        }

        public BindingDeclaration WithArguments(params KeyValuePair<string, object>[] arguments)
        {
            arguments.ForEach(Arguments.Add);
            return new BindingDeclaration(Queue, Exchange, RoutingKey, Arguments);
        }

        public override string ToString()
        {
            return
                $"BindingDeclaration(Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}, Arguments={Arguments.Count})";
        }
    }

    public sealed class ExchangeDeclaration : IDeclaration
    {
        private ExchangeDeclaration(string name, string exchangeType, bool durable = false, bool autoDelete = false,
            bool @internal = false, IDictionary<string, object> arguments = null)
        {
            Name = name;
            ExchangeType = exchangeType;
            Durable = durable;
            AutoDelete = autoDelete;
            Internal = @internal;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        public string Name { get; }

        public string ExchangeType { get; }

        public bool Durable { get; }

        public bool AutoDelete { get; }

        public bool Internal { get; }

        public IDictionary<string, object> Arguments { get; }

        public static ExchangeDeclaration Create(string name, string exchangeType)
        {
            return new ExchangeDeclaration(name, exchangeType);
        }

        public ExchangeDeclaration WithDurable(bool durable)
        {
            return new ExchangeDeclaration(Name, ExchangeType, durable, AutoDelete, Internal, Arguments);
        }

        public ExchangeDeclaration WithAutoDelete(bool autoDelete)
        {
            return new ExchangeDeclaration(Name, ExchangeType, Durable, autoDelete, Internal, Arguments);
        }

        public ExchangeDeclaration WithInternal(bool @internal)
        {
            return new ExchangeDeclaration(Name, ExchangeType, Durable, AutoDelete, @internal, Arguments);
        }

        public ExchangeDeclaration WithArguments(string key, object value)
        {
            Arguments.Add(key, value);
            return new ExchangeDeclaration(Name, ExchangeType, Durable, AutoDelete, Internal, Arguments);
        }

        public ExchangeDeclaration WithArguments(KeyValuePair<string, object> argument)
        {
            Arguments.Add(argument);
            return new ExchangeDeclaration(Name, ExchangeType, Durable, AutoDelete, Internal, Arguments);
        }

        public ExchangeDeclaration WithArguments(params KeyValuePair<string, object>[] arguments)
        {
            arguments.ForEach(Arguments.Add);
            return new ExchangeDeclaration(Name, ExchangeType, Durable, AutoDelete, Internal, Arguments);
        }

        public override string ToString()
        {
            return
                $"ExchangeDeclaration(Name={Name}, ExchangeType={ExchangeType}, Durable={Durable}, AutoDelete={AutoDelete}, Internal={Internal}, Arguments={Arguments.Count})";
        }
    }

}
