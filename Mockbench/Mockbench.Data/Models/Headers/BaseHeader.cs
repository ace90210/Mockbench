using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models.Headers
{
    public abstract class BaseHeader
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        public int Id { get; set; }

        [MaxLength(150)]
        public string Name { get; set; }
    }
}
