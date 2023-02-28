using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Utils
{
	public static string Vector3ToPosition(Vector3 position) {
        string newPosition = "position";
        /*
        newPosition += string.Format(" {0:0.##}", position.x);
        newPosition += string.Format(" {0:0.##}", position.y);
        newPosition += string.Format(" {0:0.##}", position.z);
        */
        newPosition += " " + FormatNumberWithDotAsDecimalSeparator(position.x);
        newPosition += " " + FormatNumberWithDotAsDecimalSeparator(position.y);
        newPosition += " " + FormatNumberWithDotAsDecimalSeparator(position.z);
		return newPosition;
	}

    public static string FormatNumberWithDotAsDecimalSeparator(double number) {
        // 0 => At least one 0.
        // # => If there is no number, write nothing.
        // Examples: 12.456 => 12.45; 0 => 0.
        return string.Format(CultureInfo.InvariantCulture, "{0:0.##}", number);
    }
}
