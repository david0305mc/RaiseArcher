using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;


public enum EMessage
{
    Update_Heart,
    Update_Mission,
}


public struct EventParm<T1, T2>
{
    public T1 arg1;
    public T2 arg2;

    public EventParm(T1 arg1, T2 arg2)
    {
        this.arg1 = arg1;
        this.arg2 = arg2;
    }
}

public struct EventParm<T1, T2, T3>
{
    public T1 arg1;
    public T2 arg2;
    public T3 arg3;

    public EventParm(T1 arg1, T2 arg2, T3 arg3)
    {
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = arg3;
    }
}

public partial class MessageDispather
{
    public static void Publish(EMessage message)
    {
        Publish(message, Unit.Default);
    }

    public static UniTask PublishAsync(EMessage message, CancellationToken cancellationToken = default(CancellationToken))
    {
        return PublishAsync(message, Unit.Default, cancellationToken);
    }

    public static IObservable<Unit> Receive(EMessage message)
    {
        return Receive<EMessage, Unit>(message);
    }

    public static IObservable<T> Receive<T>(EMessage message)
    {
        return Receive<EMessage, T>(message);
    }
}