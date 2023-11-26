using UnityEngine;

namespace Utils
{
    public static class GameUtilities
    {
        public static float CalculateAngle(Vector2 start, Vector2 end)
        {
            float angle = Mathf.Atan2(end.y, end.x) - Mathf.Atan2(start.y, start.x);
            angle *= Mathf.Rad2Deg;
            return angle;
        }

        public static float CalculateDistance(Vector3 startPosition, Vector3 endPosition)
        {
            float distance = Vector2.Distance(startPosition, endPosition);
            return distance;
        }
    }
}