using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CrowdKnowledgeContribution.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; }

        [Required]
        public string Icon { get; set; }

        public virtual ICollection<Article>? Articles { get; set; }
    }
}
