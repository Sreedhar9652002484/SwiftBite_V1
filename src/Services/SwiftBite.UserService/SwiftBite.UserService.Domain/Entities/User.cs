using SwiftBite.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.UserService.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public UserRole Role { get; private set; }
        public List<Address> Addresses { get; private set; } = new();

        private User() { } // EF Core needs this

        public static User Create(string firstName, string lastName,
                                   string email, string phone)
        {
            return new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.ToLowerInvariant(),
                PhoneNumber = phone,
                Role = UserRole.Customer
            };
        }

        public void AddAddress(Address address) => Addresses.Add(address);
    }
}
