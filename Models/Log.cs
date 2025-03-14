using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BankingServices.Models
{
    public class Log
    {
        public long Id { get; set; }
        
        [Required]
        public Guid RequestId { get; set; }
        
        [Required]
        [Column(TypeName = "jsonb")]
        public string RequestObject { get; set; } = string.Empty;
        
        [Required]
        public string RouteURL { get; set; } = string.Empty;
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Helper method to set RequestObject from any object
        public void SetRequestObject<T>(T requestObject)
        {
            if (requestObject != null)
            {
                RequestObject = JsonSerializer.Serialize(requestObject);
            }
        }
        
        // Helper method to get RequestObject as specific type
        public T? GetRequestObject<T>()
        {
            if (string.IsNullOrEmpty(RequestObject))
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(RequestObject);
        }
        
        // Helper method for Newtonsoft.Json
        public void SetRequestObjectNewtonsoft<T>(T requestObject)
        {
            if (requestObject != null)
            {
                RequestObject = JsonConvert.SerializeObject(requestObject);
            }
        }
        
        public T? GetRequestObjectNewtonsoft<T>()
        {
            if (string.IsNullOrEmpty(RequestObject))
            {
                return default;
            }
            
            return JsonConvert.DeserializeObject<T>(RequestObject);
        }
    }
}