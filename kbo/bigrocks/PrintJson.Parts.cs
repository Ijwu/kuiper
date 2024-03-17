namespace kbo.bigrocks;

public partial record PrintJson
{
    public record ItemSend : PrintJson
    {
        [JsonPropertyName("receiving")]
        public long Receiving { get; set; }

        [JsonPropertyName("item")]
        public NetworkItem Item { get; set; }

        public ItemSend(TextJsonMessagePart[] data, long receiving, NetworkItem item)
            : base(data)
        {
            Receiving = receiving;
            Item = item;
        }
    }

    public record ItemCheat : PrintJson
    {
        [JsonPropertyName("receiving")]
        public long Receiving { get; set; }

        [JsonPropertyName("item")]
        public NetworkItem Item { get; set; }

        [JsonPropertyName("team")]
        public long Team { get; set; }

        public ItemCheat(TextJsonMessagePart[] data, long receiving, NetworkItem item, long team)
            : base(data)
        {
            Receiving = receiving;
            Item = item;
            Team = team;
        }
    }

    public record Hint : PrintJson
    {
        [JsonPropertyName("receiving")]
        public long Receiving { get; set; }

        [JsonPropertyName("item")]
        public NetworkItem Item { get; set; }

        [JsonPropertyName("found")]
        public bool Found { get; set; }

        public Hint(TextJsonMessagePart[] data, long receiving, NetworkItem item, bool found)
            : base(data)
        {
            Receiving = receiving;
            Item = item;
            Found = found;
        }
    }

    public record Join : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        public Join(TextJsonMessagePart[] data, long team, long slot, string[] tags)
            : base(data)
        {
            Team = team;
            Slot = slot;
            Tags = tags;
        }
    }

    public record Part : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        public Part(TextJsonMessagePart[] data, long team, long slot)
            : base(data)
        {
            Team = team;
            Slot = slot;
        }
    }

    public record Chat : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

         [JsonPropertyName("slot")]
        public long Slot { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        public Chat(TextJsonMessagePart[] data, long team, long slot, string message)
            : base(data)
        {
            Team = team;
            Slot = slot;
            Message = message;
        }
    }

    public record ServerChat : PrintJson
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ServerChat(TextJsonMessagePart[] data, string message)
            : base(data)
        {
            Message = message;
        }
    }

    public record TagsChanged : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        public TagsChanged(TextJsonMessagePart[] data, long team, long slot, string[] tags)
            : base(data)
        {
            Team = team;
            Slot = slot;
            Tags = tags;
        }
    }

    public record CommandResult : PrintJson
    {
        public CommandResult(TextJsonMessagePart[] data) : base(data)
        {
        }
    }

    public record AdminCommandResult : PrintJson
    {
        public AdminCommandResult(TextJsonMessagePart[] data) : base(data)
        {
        }
    }

    public record Goal : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        public Goal(TextJsonMessagePart[] data, long team, long slot)
            : base(data)
        {
            Team = team;
            Slot = slot;
        }
    }

    public record Release : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        public Release(TextJsonMessagePart[] data, long team, long slot)
            : base(data)
        {
            Team = team;
            Slot = slot;
        }
    }

    public record Collect : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        public Collect(TextJsonMessagePart[] data, long team, long slot)
            : base(data)
        {
            Team = team;
            Slot = slot;
        }
    }

    public record Countdown : PrintJson
    {
        [JsonPropertyName("countdown")]
        public long CountdownTime { get; set; }

        public Countdown(TextJsonMessagePart[] data, long countdownTime)
            : base(data)
        {
            CountdownTime = countdownTime;
        }
    }
}
