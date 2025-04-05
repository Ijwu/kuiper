namespace kbo.bigrocks
{
    public record UpdateHint : Packet
    {
        [JsonPropertyName("player")]
        public long Player { get; set; }

        [JsonPropertyName("location")]
        public long Location { get; set; }

        [JsonPropertyName("status")]
        public HintStatus? Status { get; set; }

        public UpdateHint(long player, long location, HintStatus? status)
        {
            Player = player;
            Location = location;
            Status = status;
        }
    }
}
