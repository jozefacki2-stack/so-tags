namespace TagsApi.Models
{
    public class TagEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Share { get; set; }
    }
}
