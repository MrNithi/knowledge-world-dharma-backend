using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace knowledge_world_dharma_backend.Models
{
    public class PostForm
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string HashTag { get; set; }
        public int HideStatus { get; set; }
    }
}
