namespace kbo.bigrocks;

public partial record PrintJson
{
    public record ItemSend : PrintJson
    {
        [JsonPropertyName("receiving")]
        public long Receiving { get; set; }

        [JsonPropertyName("item")]
        public NetworkItem Item { get; set; }

        public ItemSend(BaseJsonMessagePart[] data, long receiving, NetworkItem item)
            : base(data, nameof(ItemSend))
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

        public ItemCheat(BaseJsonMessagePart[] data, long receiving, NetworkItem item, long team)
            : base(data, nameof(ItemCheat))
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

        public Hint(BaseJsonMessagePart[] data, long receiving, NetworkItem item, bool found)
            : base(data, nameof(Hint))
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

        public Join(BaseJsonMessagePart[] data, long team, long slot, string[] tags)
            : base(data, nameof(Join))
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

        public Part(BaseJsonMessagePart[] data, long team, long slot)
            : base(data, nameof(Part))
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

        public Chat(BaseJsonMessagePart[] data, long team, long slot, string message)
            : base(data, nameof(Chat))
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

        public ServerChat(BaseJsonMessagePart[] data, string message)
            : base(data, nameof(ServerChat))
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

        public TagsChanged(BaseJsonMessagePart[] data, long team, long slot, string[] tags)
            : base(data, nameof(TagsChanged))
        {
            Team = team;
            Slot = slot;
            Tags = tags;
        }
    }

    public record Goal : PrintJson
    {
        [JsonPropertyName("team")]
        public long Team { get; set; }

        [JsonPropertyName("slot")]
        public long Slot { get; set; }

        public Goal(BaseJsonMessagePart[] data, long team, long slot)
            : base(data, nameof(Goal))
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

        public Release(BaseJsonMessagePart[] data, long team, long slot)
            : base(data, nameof(Release))
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

        public Collect(BaseJsonMessagePart[] data, long team, long slot)
            : base(data, nameof(Collect))
        {
            Team = team;
            Slot = slot;
        }
    }

    public record Countdown : PrintJson
    {
        [JsonPropertyName("countdown")]
        public long CountdownTime { get; set; }

        public Countdown(BaseJsonMessagePart[] data, long countdownTime)
            : base(data, nameof(CountdownTime))
        {
            CountdownTime = countdownTime;
        }
    }
}
