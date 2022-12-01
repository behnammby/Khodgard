namespace Khodgard.Utils;

public static class NumberHelper
{
    public static double ModifyDecimalPlaces(double number, int digits)
    {
        int decimalPlaces = CalculateDecimalPlaces(digits);
        double rounded = Convert.ToDouble(Convert.ToInt32(number * decimalPlaces)) / decimalPlaces;

        return rounded;
    }

    public static decimal ModifyDecimalPlaces(decimal number, int digits)
    {
        int decimalPlaces = CalculateDecimalPlaces(digits);
        decimal rounded = Convert.ToDecimal(Convert.ToInt32(number * decimalPlaces)) / decimalPlaces;

        return rounded;
    }

    public static int CalculateDecimalPlaces(int digits)
    {
        int decimalPlaces = (int)Math.Pow(10, digits);

        return decimalPlaces;
    }
}