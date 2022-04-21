namespace knowledge_world_dharma_backend.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Emoji { get; set; }
    }
}