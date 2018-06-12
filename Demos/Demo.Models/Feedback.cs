using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Models
{
    public class Feedback
    {
        // Unique ID for feedback record
        public Guid Id { get; set; }

        // Feedback message
        public string Message { get; set; }

        // Score given by text analysis
        public int Score { get; set; }
    }
}
