using System;
using Microsoft.UI.Xaml.Data;

namespace Numbers;

public class DescriptiveStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string stringValue)
        {
            return "<not a string>";
        }

        if (stringValue.Length > 1)
        {
            return stringValue;
        }

        return stringValue[0] switch
        {
            ' ' => "[Blank]",
            '\t' => "[Tab]",
            '|' => "| [Pipe]",
            ',' => ", [Comma]",
            ';' => "; [Semicolon]",
            _ => stringValue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
