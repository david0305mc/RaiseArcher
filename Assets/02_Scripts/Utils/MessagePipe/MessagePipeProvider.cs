using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using UnityEngine;
using MessagePipe;
using Cysharp.Threading.Tasks;


class ServiceProviderType
{
    readonly Type type;
    readonly ConstructorInfo ctor;
    readonly ParameterInfo[] parameters;

    public ServiceProviderType(Type type)
    {
        var info = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new { ctor = x, parameters = x.GetParameters() })
            .OrderByDescending(x => x.parameters.Length) // MaxBy
            .FirstOrDefault();

        if (!type.IsValueType && info == null)
        {
            throw new InvalidOperationException("ConsturoctorInfo is not found, is stripped? Type:" + type.FullName);
        }

        this.type = type;
        this.ctor = info?.ctor;
        this.parameters = info?.parameters;
    }

    public object Instantiate(MessagePipeProvider provider, int depth)
    {
        if (ctor == null)
        {
            return Activator.CreateInstance(type);
        }

        if (parameters.Length == 0)
        {
            return ctor.Invoke(Array.Empty<object>());
        }
        if (depth > 15)
        {
            throw new InvalidOperationException("Parameter too recursively: " + type.FullName);
        }

        var p = new object[parameters.Length];
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = provider.GetService(parameters[i].ParameterType, depth + 1);
        }

        return ctor.Invoke(p);
    }
}

partial class MessagePipeProvider
{
    readonly Dictionary<Type, Lazy<object>> singletonInstances = new Dictionary<Type, Lazy<object>>();
    readonly Dictionary<Type, ServiceProviderType> transientTypes = new Dictionary<Type, ServiceProviderType>();

    public MessagePipeProvider AddMessagePipe()
    {
        return AddMessagePipe(_ => { });
    }

    public MessagePipeProvider AddMessagePipe(Action<MessagePipeOptions> configure)
    {
        ServiceCollectionExtensions.AddMessagePipe(this, configure);
        return this;
    }

    public void AddMessageBroker<TMessage>()
    {
        AddSingleton(typeof(MessageBrokerCore<TMessage>));
        AddSingleton(typeof(IPublisher<TMessage>), typeof(MessageBroker<TMessage>));
        AddSingleton(typeof(ISubscriber<TMessage>), typeof(MessageBroker<TMessage>));

        AddSingleton(typeof(AsyncMessageBrokerCore<TMessage>));
        AddSingleton(typeof(IAsyncPublisher<TMessage>), typeof(AsyncMessageBroker<TMessage>));
        AddSingleton(typeof(IAsyncSubscriber<TMessage>), typeof(AsyncMessageBroker<TMessage>));

        AddSingleton(typeof(BufferedMessageBrokerCore<TMessage>));
        AddSingleton(typeof(IBufferedPublisher<TMessage>), typeof(BufferedMessageBroker<TMessage>));
        AddSingleton(typeof(IBufferedSubscriber<TMessage>), typeof(BufferedMessageBroker<TMessage>));

        AddSingleton(typeof(BufferedAsyncMessageBrokerCore<TMessage>));
        AddSingleton(typeof(IBufferedAsyncPublisher<TMessage>), typeof(BufferedAsyncMessageBroker<TMessage>));
        AddSingleton(typeof(IBufferedAsyncSubscriber<TMessage>), typeof(BufferedAsyncMessageBroker<TMessage>));
    }

    public void AddMessageBroker<TKey, TMessage>()
    {
        AddSingleton(typeof(MessageBrokerCore<TKey, TMessage>));
        AddSingleton(typeof(IPublisher<TKey, TMessage>), typeof(MessageBroker<TKey, TMessage>));
        AddSingleton(typeof(ISubscriber<TKey, TMessage>), typeof(MessageBroker<TKey, TMessage>));

        AddSingleton(typeof(AsyncMessageBrokerCore<TKey, TMessage>));
        AddSingleton(typeof(IAsyncPublisher<TKey, TMessage>), typeof(AsyncMessageBroker<TKey, TMessage>));
        AddSingleton(typeof(IAsyncSubscriber<TKey, TMessage>), typeof(AsyncMessageBroker<TKey, TMessage>));
    }
}


partial class MessagePipeProvider : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        return GetService(serviceType, 0);
    }

    public object GetService(Type serviceType, int depth)
    {
        if (serviceType == typeof(IServiceProvider))
        {
            return this;
        }

        if (singletonInstances.TryGetValue(serviceType, out var value))
        {
            return value.Value;
        }

        if (transientTypes.TryGetValue(serviceType, out var providerType))
        {
            return providerType.Instantiate(this, depth);
        }

        return null;
    }

    public T GetService<T>()
    {
        var service = GetService(typeof(T));
        if (service == null)
            return default;

        return (T)service;
    }
}

partial class MessagePipeProvider : IServiceCollection
{
    public void AddSingleton<T>(T instance)
    {
        singletonInstances[typeof(T)] = new Lazy<object>(() => instance);
    }

    public void AddSingleton(Type type)
    {
        AddSingleton(type, type);
    }

    public void AddTransient(Type type)
    {
        transientTypes[type] = new ServiceProviderType(type);
    }

    public void TryAddTransient(Type type)
    {
        if (transientTypes.ContainsKey(type))
            return;

        AddTransient(type);
    }

    public void AddSingleton(Type serviceType, Type implementationType)
    {
        singletonInstances[serviceType] = new Lazy<object>(() => new ServiceProviderType(implementationType).Instantiate(this, 0));
    }

    public void Add(Type serviceType, Type implementationType, InstanceLifetime lifetime)
    {
        if (lifetime == InstanceLifetime.Scoped || lifetime == InstanceLifetime.Singleton)
        {
            AddSingleton(serviceType, implementationType);
        }
        else
        {
            transientTypes[serviceType] = new ServiceProviderType(implementationType);
        }
    }
}