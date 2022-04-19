using System;
namespace knowledge_world_dharma_backend.Models
{
    public class Like
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public string UserId { get; set; }

        public string Emoji { get; set; }

        public Like()
        {
        }
    }
}