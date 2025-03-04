﻿using System;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

using b7.Xabbo.Avalonia.Helpers;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomInfoViewModel : ViewModelBase
{
    private readonly RoomManager _roomManager;

    [Reactive] public bool IsInRoom { get; set; }
    [Reactive] public IRoomData? Data { get; set; }


    private long _currentThumbnailId = -1;
    private Lazy<Task<Bitmap?>> _loadImage = new(() => LoadImage(-1));
    public Task<Bitmap?> Thumbnail => _loadImage.Value;

    private static Task<Bitmap?> LoadImage(long id)
    {
        if (id <= 0)
            return Task.FromResult<Bitmap?>(null);
        return ImageHelper.LoadFromWeb(new Uri($"https://habbo-stories-content.s3.amazonaws.com/navigator-thumbnail/hhus/{id}.png"));
    }

    public RoomInfoViewModel(IExtension extension, RoomManager roomManager)
    {
        _roomManager = roomManager;

        extension.Disconnected += OnGameDisconnected;

        roomManager.Entered += OnEnteredRoom;
        roomManager.RoomDataUpdated += OnRoomDataUpdated;
        roomManager.Left += OnLeftRoom;
    }

    private void UpdateThumbnail(long roomId)
    {
        if (_currentThumbnailId == roomId)
            return;

        _currentThumbnailId = roomId;
        _loadImage = new Lazy<Task<Bitmap?>>(() => LoadImage(roomId));
        this.RaisePropertyChanged(nameof(Thumbnail));
    }

    private void OnEnteredRoom(object? sender, EventArgs e)
    {
        Data = _roomManager.Data;

        if (Data is not null)
        {
            UpdateThumbnail(Data.Id);
        }
    }

    private void OnRoomDataUpdated(object? sender, RoomDataEventArgs e)
    {
        Data = e.Data;
        IsInRoom = true;

        UpdateThumbnail(e.Data.Id);
    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        UpdateThumbnail(0);
        IsInRoom = false;
        Data = null;
    }

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Data = null;
    }
}
