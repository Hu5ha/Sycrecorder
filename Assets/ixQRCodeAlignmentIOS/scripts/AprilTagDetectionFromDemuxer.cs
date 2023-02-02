using UnityEngine;
using System.Linq;
using UI = UnityEngine.UI;
using Klak.TestTools;
using System.Collections.Generic;
using Bibcam.Decoder;
using UnityEngine.UI;

public class AprilTagDetectionFromDemuxer: MonoBehaviour
{
    //[SerializeField] ImageSource _source = null;

    [SerializeField] int _decimation = 4;
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    [SerializeField] BibcamTextureDemuxer textureDemuxer;

    [SerializeField] GameObject MarkerMask;
    [SerializeField] GameObject ARCamera;
    [SerializeField] GameObject ARObjectOrigin;
    public bool enableAlignment = false;
    public bool enableMarkerDetection = true;
    public Toggle DetectionUI;

    public Dictionary<int, GameObject> tagDict = new Dictionary<int, GameObject>();

    AprilTag.TagDetector _detector;
    TagDrawer _drawer;
    //GameObject marker;

    void Start()
    {
        //marker = Instantiate(MarkerMask);
    }

    void OnDestroy()
    {
        if (_detector != null) _detector.Dispose();
        if (_drawer != null) _drawer.Dispose();
    }

    void LateUpdate()
    {
        if (!enableMarkerDetection) return;

        RenderTexture renderTexture = textureDemuxer.unDestortedColorTexture;

        // 
        if (!renderTexture)
        {
            Debug.Log("renderTexture not avaliable!");
            return;
        }

        // source image acquisition
        var image = renderTexture.AsSpan();
        if (image.IsEmpty)
        {
            return;
        }

        //
        if (_detector == null)
        {
            int w = renderTexture.width;
            int h = renderTexture.height;
            Debug.Log("image width & height: " + w + " ," + h);
            _detector = new AprilTag.TagDetector(w, h, _decimation);
            _drawer = new TagDrawer(_tagMaterial);
        }

        //
        if (_detector == null) 
        {
            Debug.Log("_detector is empty");
            return;
        }

        // AprilTag detection
        var fov = ARCamera.GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad; // the captured camera parameter 
        _detector.ProcessImage(image, fov, _tagSize);
        Debug.Log("_detector.DetectedTags = " + _detector.DetectedTags.Count);

        // detected tag visualization
        foreach (var tag in _detector.DetectedTags)
            _drawer.Draw(tag.ID, ARCamera.transform.TransformPoint(tag.Position), ARCamera.transform.rotation * tag.Rotation, _tagSize);

        //// visualize via marker mask 
        //if (_detector.DetectedTags.Count > 0)
        //{
        //    marker.transform.position = ARCamera.transform.TransformPoint(_detector.DetectedTags[0].Position);
        //    marker.transform.rotation = ARCamera.transform.rotation * _detector.DetectedTags[0].Rotation;
        //}

        // align 
        if (enableAlignment && _detector.DetectedTags.Count > 0)
        {
            var tag = _detector.DetectedTags[0];
            ARObjectOrigin.transform.position = ARCamera.transform.TransformPoint(tag.Position);
            ARObjectOrigin.transform.rotation = ARCamera.transform.rotation * tag.Rotation;
            enableAlignment = false;
            enableMarkerDetection = false;
            DetectionUI.isOn = false;
            Canvas.ForceUpdateCanvases();
        }

        // Profile data output (with 30 frame interval)
        //if (Time.frameCount % 30 == 0)
        //    _debugText.text = _detector.ProfileData.Aggregate
        //      ("Profile (usec)", (c, n) => $"{c}\n{n.name} : {n.time}");
    }
    public void toggleAlignmentEnabled()
    {
        enableAlignment = true;
    }
    public void toggleMarkerDetection(bool b)
    {
        enableMarkerDetection = b;
        Canvas.ForceUpdateCanvases();
    }
}
