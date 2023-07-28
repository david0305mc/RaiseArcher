using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using Cysharp.Threading.Tasks;
using MessagePipe;

/// <summary>이벤트 전달자</summary>
public partial class MessageDispather
{
    private static MessagePipeProvider _provider;
    public static readonly MessageDispather Default = new MessageDispather();

    public MessageDispather()
    {
        _provider = new MessagePipeProvider();
        _provider.AddMessagePipe();

        GlobalMessagePipe.SetProvider(_provider);
    }

    public static IPublisher<TMessage> GetPublisher<TMessage>()
    {
        return _provider.GetService<IPublisher<TMessage>>();
    }

    public static IPublisher<TKey, TMessage> GetPublisher<TKey, TMessage>()
    {
        return _provider.GetService<IPublisher<TKey, TMessage>>();
    }

    public static IAsyncPublisher<TMessage> GetAsyncPublisher<TMessage>()
    {
        return _provider.GetService<IAsyncPublisher<TMessage>>();
    }

    public static IAsyncPublisher<TKey, TMessage> GetAsyncPublisher<TKey, TMessage>()
    {
        return _provider.GetService<IAsyncPublisher<TKey, TMessage>>();
    }

    public static ISubscriber<TMessage> GetSubscriber<TMessage>()
    {
        var service = _provider.GetService<IPublisher<TMessage>>();
        if (service == null)
        {
            _provider.AddMessageBroker<TMessage>();
            service = _provider.GetService<IPublisher<TMessage>>();
        }

        return (ISubscriber<TMessage>)service;
    }

    public static ISubscriber<TKey, TMessage> GetSubscriber<TKey, TMessage>()
    {
        var service = _provider.GetService<IPublisher<TKey, TMessage>>();
        if (service == null)
        {
            _provider.AddMessageBroker<TKey, TMessage>();
            service = _provider.GetService<IPublisher<TKey, TMessage>>();
        }

        return (ISubscriber<TKey, TMessage>)service;
    }

    public static IAsyncSubscriber<TMessage> GetAsyncSubscriber<TMessage>()
    {
        var service = _provider.GetService<IAsyncSubscriber<TMessage>>();
        if (service == null)
        {
            _provider.AddMessageBroker<TMessage>();
            service = _provider.GetService<IAsyncSubscriber<TMessage>>();
        }

        return (IAsyncSubscriber<TMessage>)service;
    }

    public static IAsyncSubscriber<TKey, TMessage> GetAsyncSubscriber<TKey, TMessage>()
    {
        var service = _provider.GetService<IAsyncSubscriber<TKey, TMessage>>();
        if (service == null)
        {
            _provider.AddMessageBroker<TKey, TMessage>();
            service = _provider.GetService<IAsyncSubscriber<TKey, TMessage>>();
        }

        return (IAsyncSubscriber<TKey, TMessage>)service;
    }

    public static void Publish<TMessage>(TMessage message)
    {
        GetPublisher<TMessage>()?.Publish(message);
    }

    public static void Publish<TKey, TMessage>(TKey message, TMessage data)
    {
        GetPublisher<TKey, TMessage>()?.Publish(message, data);
    }

    public static UniTask PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken))
    {
        return GetAsyncPublisher<TMessage>()?.PublishAsync(message, cancellationToken) ?? UniTask.CompletedTask;
    }

    public static UniTask PublishAsync<TKey, TMessage>(TKey message, TMessage data, CancellationToken cancellationToken = default(CancellationToken))
    {
        return GetAsyncPublisher<TKey, TMessage>()?.PublishAsync(message, data, cancellationToken) ?? UniTask.CompletedTask;
    }
}

public partial class MessageDispather
{
    public static void Publish(string message)
    {
        Publish<string, Unit>(message, Unit.Default);
    }

    public static IObservable<TMessage> Receive<TMessage>()
    {
        return GetSubscriber<TMessage>().AsObservable();
    }

    public static IObservable<TMessage> Receive<TKey, TMessage>(TKey message)
    {
        return GetSubscriber<TKey, TMessage>().AsObservable(message);
    }

    public static IObservable<TMessage> Receive<TMessage>(string message)
    {
        return Receive<string, TMessage>(message);
    }

    public static IObservable<Unit> Receive(string message)
    {
        return Receive<string, Unit>(message);
    }
}
