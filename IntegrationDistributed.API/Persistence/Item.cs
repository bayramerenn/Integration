namespace IntegrationDistributed.API.Persistence
{
    public class Item
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;

        public override string ToString()
        {
            return $"{Id}:{Content}";
        }
    }
}