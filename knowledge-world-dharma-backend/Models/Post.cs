using System;
namespace knowledge_world_dharma_backend.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public int Ref { get; set; }
        public Post()
        {
        }
    }
}
