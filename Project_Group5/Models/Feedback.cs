using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class Feedback
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? Content { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public int? RoomId { get; set; }
        public int? Rating { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Room? Room { get; set; }
    }
}
