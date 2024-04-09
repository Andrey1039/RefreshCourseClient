using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefreshCourseClient.Models
{
    internal class AuthResponseModel
    {
        public string? Token { get; set; }
        public string? PublicKey { get; set; }
    }
}
