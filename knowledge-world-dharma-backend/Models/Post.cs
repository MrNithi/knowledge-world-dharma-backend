using System;
namespace knowledge_world_dharma_backend.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string HashTag { get; set; }
        public int UserId { get; set; }
        public bool HideStatus { get; set; }
        public int Ref { get; set; }
        public Post()
        {
            this.HideStatus = false;
            this.Ref = 0;
        }
    }
}
