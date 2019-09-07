using System.Collections.Generic;
using testMvc.Controllers;

namespace testMvc.Models
{
    public class Store
    {
        public static Dictionary<string, JwtToken> TokenStore = new Dictionary<string, JwtToken>();
    }
}