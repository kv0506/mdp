using CSharpExtensions;

namespace MDP.OMDb;

public static class StringExtensions
{
    public static bool IsTrue(this string value)
    {
        return value.IsEquals(true.ToString(), StringComparison.InvariantCultureIgnoreCase);
    }
}