﻿using Utili.Database.Entities;

namespace Utili.Backend.Models;

public class MessagePinningConfigurationModel
{
    public string PinChannelId { get; set; }
    public bool PinMessages { get; set; }

    public void ApplyTo(MessagePinningConfiguration configuration)
    {
        configuration.PinChannelId = ulong.Parse(PinChannelId);
        configuration.PinMessages = PinMessages;
    }
}