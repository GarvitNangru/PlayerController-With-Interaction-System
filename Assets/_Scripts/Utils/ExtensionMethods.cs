using UnityEngine;
namespace Utils.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// To Have debug log colored.
        /// </summary>
        /// <param name="text"> String to print </param>
        /// <param name="color"> Enter Any color , Hex,Color Name </param>
        static public void ColoredLog(string text, string color = "White")
        {
            Debug.Log($"<color={color}> {text} </color>");
        }

        /// <summary>
        /// To Have a string colored.
        /// </summary>
        /// <param name="text"> String to print </param>
        /// <param name="color"> Enter Any color , Hex,Color Name </param>
        static public string ToColor(this string text, string color = "White")
        {
            return $"<color={color}> {text} </color>";
        }
    }
}
