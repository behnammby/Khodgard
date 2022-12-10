using System.Text;

namespace Khodgard.Utils;

public static class NumberHelper
{
    public static object ModifyDecimalPlaces(object number, int places)
    {
        StringBuilder sb = new();
        sb.Append("0.");
        for (int i = 0; i < places; i++)
            sb.Append("#");

        string format = sb.ToString();

        return number switch
        {
            float @float => float.Parse(@float.ToString(format)),
            double @double => double.Parse(@double.ToString(format)),
            decimal @decimal => decimal.Parse(@decimal.ToString(format)),

            _ => 0
        };
    }
}