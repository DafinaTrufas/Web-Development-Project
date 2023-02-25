using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrowdKnowledgeContribution.Models
{
    public class ProtectedArticle
    {
        [ForeignKey("Article")]
        public int ProtectedArticleId { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public virtual Article? Article { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}