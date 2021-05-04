using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class WhotUser : IdentityUser
    {
        public DateTime? LastActive { get; set; } = DateTime.UtcNow;
    }
}