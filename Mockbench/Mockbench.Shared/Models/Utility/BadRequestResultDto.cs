using System.Collections.Generic;
using System.Linq;

namespace Mockbench.Shared.Models.Utility
{
    public class BadRequestResultDto
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string TraceId { get; set; }

        public Dictionary<string, List<string>> Errors { get; set; }

        public IEnumerable<string> GetErrors(string key)
        {
            if(Errors == null || Errors.Count == 0)
                return Enumerable.Empty<string>();
            
            if (Errors.TryGetValue(key, out List<string> errors))
            {
                return errors;
            }

            return Enumerable.Empty<string>();
        }
    }
}
