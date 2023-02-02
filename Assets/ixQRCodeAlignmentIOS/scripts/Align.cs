using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align : MonoBehaviour
{
    public AprilTagDetection detectionTest;

    Vector3 QRPosition;
    Quaternion QROrientation;
    // Start is called before the first frame update
    void Start()
    {
        QRPosition = new Vector3(0,1,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleAlignToQRCode(){
        detectionTest.enableAlignment = !detectionTest.enableAlignment;
    }
}
