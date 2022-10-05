﻿using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Messages;

using b7.Xabbo.Commands;

namespace b7.Xabbo.Components;

public class ClickThroughComponent : Component
{
    protected readonly CommandManager _commandManager;

    public ClickThroughComponent(IExtension extension,
        IConfiguration config,
        CommandManager commandManager)
        : base(extension)
    {
        _commandManager = commandManager;
        _commandManager.Register(OnToggle, "ct", null);

        IsActive = config.GetValue("ClickThrough:Active", false);
    }

    protected override void OnInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        base.OnInitialized(sender, e);
    }

    private Task OnToggle(CommandArgs args)
    {
        IsActive = !IsActive;
        _commandManager.ShowMessage($"Click-through {(IsActive ? "enabled" : "disabled")}");

        return Task.CompletedTask;
    }

    [RequiredIn(nameof(Incoming.GameYouArePlayer))]
    public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.RaisePropertyChanged(propertyName);

        if (propertyName.Equals(nameof(IsActive)))
        {
            Extension.Send(In.GameYouArePlayer, IsActive);
        }
    }

    [InterceptIn(nameof(Incoming.RoomEntryInfo))]
    [RequiredIn(nameof(Incoming.GameYouArePlayer))]
    private void OnEnterRoom(InterceptArgs e)
    {
        if (IsActive)
        {
            Extension.Send(In.GameYouArePlayer, true);
        }
    }
}
