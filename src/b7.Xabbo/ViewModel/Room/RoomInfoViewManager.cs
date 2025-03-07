﻿using System;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace b7.Xabbo.ViewModel;

public class RoomInfoViewManager : ComponentViewModel
{
    private readonly RoomManager _roomManager;

    private bool _isInRoom;
    public bool IsInRoom
    {
        get => _isInRoom;
        set => SetProperty(ref _isInRoom, value);
    }

    private IRoomData? _data;
    public IRoomData? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public RoomInfoViewManager(IExtension extension,
        RoomManager roomManager)
        : base(extension)
    {
        _roomManager = roomManager;

        extension.Disconnected += OnGameDisconnected;

        roomManager.Entered += OnEnteredRoom;
        roomManager.RoomDataUpdated += OnRoomDataUpdated;
        roomManager.Left += OnLeftRoom;
    }

    private void OnEnteredRoom(object? sender, EventArgs e)
    {
        Data = _roomManager.Data;
    }

    private void OnRoomDataUpdated(object? sender, RoomDataEventArgs e)
    {
        Data = e.Data;
        IsInRoom = true;
    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Data = null;
    }

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Data = null;
    }
}
