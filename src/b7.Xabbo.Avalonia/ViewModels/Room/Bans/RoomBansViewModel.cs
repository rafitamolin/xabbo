﻿using System;
using System.Reactive;
using System.Threading.Tasks;

using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo;
using Xabbo.Core.Game;
using Xabbo.Extension;

using b7.Xabbo.Services;
using System.Reactive.Linq;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomBansViewModel : ViewModelBase
{
    private readonly IUiContext _uiCtx;
    private readonly IExtension _ext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<RoomBanViewModel, long> _banCache = new(x => x.Id);

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool CanLoad { get; set; } = true;
    [Reactive] public bool IsLoading { get; set; }

    public ReactiveCommand<Unit, Unit> UnbanCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadBansCommand { get; }

    public RoomBansViewModel(IUiContext uiContext, IExtension extension, RoomManager roomManager)
    {
        _uiCtx = uiContext;
        _ext = extension;
        _roomManager = roomManager;

        _roomManager.Left += OnLeftRoom;

        UnbanCommand = ReactiveCommand.CreateFromTask(UnbanAsync);
        LoadBansCommand = ReactiveCommand.CreateFromTask(LoadBansAsync);
    }

    private async Task UnbanAsync()
    {

    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        _uiCtx.Invoke(_banCache.Clear);
    }

    private async Task LoadBansAsync()
    {
        long currentRoomId = _roomManager.CurrentRoomId;
        if (currentRoomId <= 0) return;

        try
        {
            IsLoading = true;

            var receiver = _ext.ReceiveAsync(_ext.Messages.In.BannedUsersFromRoom, timeout: 3000, block: true);
            _ext.Send(_ext.Messages.Out.GetBannedUsersFromRoom, currentRoomId);
            var packet = await receiver;
            long roomId = packet.ReadLegacyLong();
            if (roomId != currentRoomId)
            {
                throw new Exception("Room ID mismatch");
            }

            short n = packet.ReadLegacyShort();
            var viewModels = new RoomBanViewModel[n];
            for (int i = 0; i < n; i++)
            {
                long userId = packet.ReadLegacyLong();
                string userName = packet.ReadString();

                viewModels[i] = new RoomBanViewModel(userId, userName);
            }

            _uiCtx.Invoke(() =>
            {
                foreach (var vm in viewModels)
                {
                    _banCache.AddOrUpdate(vm);
                }
            });
        }
        catch { }
        finally
        {
            IsLoading = false;
        }
    }
}
