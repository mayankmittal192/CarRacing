using UnityEngine;

[System.Serializable]
public class Lane
{
    private const float PASSING_LANE_DISTANCE = 3.0f;
    private const float NON_PASSING_LANE_DISTANCE = 9.0f;

    public enum Type { Passing, NonPassing }
    public Type type;


    public float distance(Type type)
    {
        return (type == Type.Passing) ? PASSING_LANE_DISTANCE : NON_PASSING_LANE_DISTANCE;
    }
}
