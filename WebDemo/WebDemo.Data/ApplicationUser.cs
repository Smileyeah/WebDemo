using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebDemo.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool Disabled { get; set; }

    }
}
