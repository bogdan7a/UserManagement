using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
