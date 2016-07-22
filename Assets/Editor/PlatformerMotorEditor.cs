using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(PlatformerMotor))]
public class PlatformerMotorEditor : Editor {
    public override void OnInspectorGUI() {
        PlatformerMotor motor = target as PlatformerMotor;

        DrawDefaultInspector ();

        string velocityString = (motor != null && motor.RB != null ? motor.RB.velocity.ToString () : "<unknown>");
        EditorGUILayout.LabelField("Velocity", velocityString);
    }
}
