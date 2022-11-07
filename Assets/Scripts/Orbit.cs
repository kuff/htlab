using UnityEngine;

/*
 * © jaredRenCode & Peter Leth https://github.com/renaissanceCoder/Simple-Scripting-Series/blob/master/Orbit.cs
 */
public class Orbit : MonoBehaviour {

    public float xSpread;
    public float zSpread;
    public float yOffset;
    public Transform centerPoint;

    public float rotSpeed;
    public bool rotateClockwise;

    private float timer = 0;
	
    private void Update () {
        timer += Time.deltaTime * rotSpeed;
        Rotate();
    }

    private void Rotate()
    {
        var x = (rotateClockwise ? -Mathf.Cos(timer) : Mathf.Cos(timer)) * xSpread;
        var z = Mathf.Sin(timer) * zSpread;
        var pos = new Vector3(x, yOffset, z);
        var centerPointPosition = centerPoint.position;
        transform.position = pos + new Vector3(centerPointPosition.x, 0, centerPointPosition.z);
        transform.LookAt(centerPoint);
    }
}