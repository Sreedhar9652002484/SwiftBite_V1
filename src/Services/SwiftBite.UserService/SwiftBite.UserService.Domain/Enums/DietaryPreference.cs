using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.UserService.Domain.Enums
{
    public enum DietaryPreference
    {
        None = 0,
        Vegetarian = 1,
        Vegan = 2,
        NonVegetarian = 3,   // ✅ ADD THIS
        Jain = 4,
        Keto = 5,
        GlutenFree = 6
    }
}
