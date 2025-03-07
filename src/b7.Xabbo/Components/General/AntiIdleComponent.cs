﻿using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xabbo.Messages;
using Xabbo.Extension;

using Xabbo.Core;
using Xabbo.Core.Game;

namespace b7.Xabbo.Components;

public class AntiIdleComponent : Component
{
    private readonly IConfiguration _config;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private int _latencyCheckCount = -1;

    private bool _isAntiIdleOutActive;
    public bool IsAntiIdleOutActive
    {
        get => _isAntiIdleOutActive;
        set => SetProperty(ref _isAntiIdleOutActive, value);
    }

    public AntiIdleComponent(IExtension extension,
        IConfiguration config,
        IHostApplicationLifetime lifetime,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _config = config;
        _lifetime = lifetime;

        _profileManager = profileManager;
        _roomManager = roomManager;

        IsActive = config.GetValue("AntiIdle:Active", true);
        IsAntiIdleOutActive = config.GetValue("AntiIdle:AntiIdleOut", true);
    }

    protected override void OnDisconnected(object? sender, EventArgs e)
    {
        base.OnDisconnected(sender, e);

        _latencyCheckCount = -1;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(IsActive):
            case nameof(IsAntiIdleOutActive):
                {
                    SendAntiIdlePacket();
                }
                break;
            default:
                break;
        }
    }

    protected override void OnInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        base.OnInitialized(sender, e);

        IsActive = _config.GetValue("AntiIdle:Active", true);
        IsAntiIdleOutActive = _config.GetValue("AntiIdle:IdleOutActive", true);
    }

    [InterceptIn(nameof(Incoming.ClientLatencyPingResponse))]
    protected void HandleClientLatencyPingResponse(InterceptArgs e)
    {
        _latencyCheckCount = e.Packet.ReadInt();

        if (_latencyCheckCount > 0 &&
            _latencyCheckCount % 12 == 0)
        {
            SendAntiIdlePacket();
        }
    }

    private void SendAntiIdlePacket()
    {
        if (_latencyCheckCount <= 0) return;

        if (_profileManager.UserData is not null &&
            _roomManager.Room is not null &&
            _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IRoomUser? self))
        {
            if (self.IsIdle && IsAntiIdleOutActive)
            {
                Extension.Send(Out.Expression, 5);
            }
            else if (self.Dance != 0 && IsActive)
            {
                Extension.Send(Out.Move, 0, 0);
            }
            else if (IsActive)
            {
                Extension.Send(Out.Expression, 0);
            }
        }
        else
        {
            if (IsActive)
            {
                Extension.Send(Out.Expression, 0);
            }
        }
    }
}
