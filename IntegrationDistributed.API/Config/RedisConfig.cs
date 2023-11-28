namespace IntegrationDistributed.API.Config
{
    public sealed record RedisConfig
    {
        public string[] Instances { get; set; }
    };
}