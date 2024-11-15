using System.ComponentModel;

namespace Categories;

// You can use any enumeration to define argument categories. The order of the enumeration defines
// the order in which the categories will be displayed in the usage help.
//
// Use the DescriptionAttribute to specify the text that will be displayed in the header for each
// category. If no DescriptionAttribute is present, the name of the enumeration member is used.
//
// You can use any custom enumeration to define categories. The only restriction is that all
// arguments in a class must use the same enumeration type.
enum ArgumentCategory
{
    [Description("Installation options")]
    Install,
    [Description("User account options")]
    UserAccounts,
    [Description("Domain options")]
    Domain,
    [Description("Other options")]
    Other,
}
