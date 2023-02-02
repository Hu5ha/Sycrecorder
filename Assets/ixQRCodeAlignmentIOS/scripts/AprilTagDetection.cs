using UnityEngine;
using System.Linq;
using UI = UnityEngine.UI;
using Klak.TestTools;
using System.Collections.Generic;
using Bibcam.Decoder;

public class AprilTagDetection : MonoBehaviour
{
    //[SerializeField] ImageSource _source = null;

    [SerializeField] int _decimation = 4;
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    //public ManomotionManager manoManager;
    //[SerializeField] BibcamTextureDemuxer textureDemuxer;
    [SerializeField] GameObject MarkerMask;

    //[SerializeField] UI.RawImage _webcamPreview = null;
    //[SerializeField] UI.Text _debugText = null;

    [SerializeField] GameObject ARCamera;
    [SerializeField] GameObject ARObjectOrigin;
    public bool enableAlignment = false;

    public Dictionary<int, GameObject> tagDict = new Dictionary<int, GameObject>();

    AprilTag.TagDetector _detector;
    TagDrawer _drawer;
    GameObject marker;

    void Start()
    {
        //int w = textureDemuxer.ColorTexture.width;
        //int h = textureDemuxer.ColorTexture.height;
        //Debug.Log("image width height from manomotion: " + w + " ," + h);
        //_detector = new AprilTag.TagDetector(w, h, _decimation);
        //_drawer = new TagDrawer(_tagMaterial);

        marker = Instantiate(MarkerMask);
    }

    void OnDestroy()
    {
        if (_detector != null) _detector.Dispose();
        if (_drawer != null) _drawer.Dispose();
    }

    void LateUpdate()
    {
        if (!GetComponent<cameraExtractorARFundation>().m_Texture)
        {
            Debug.Log("GetComponent<cameraExtractorARFundation>().m_Texture not avaliable!");
            return;
        }

        // source image acquisition
        var image = GetComponent<cameraExtractorARFundation>().m_Texture.AsSpan(); if (image.IsEmpty) return;

        if (_detector == null)
        {
            int w = GetComponent<cameraExtractorARFundation>().m_Texture.width;
            int h = GetComponent<cameraExtractorARFundation>().m_Texture.height;
            Debug.Log("image width height: " + w + " ," + h);
            _detector = new AprilTag.TagDetector(w, h, _decimation);
            _drawer = new TagDrawer(_tagMaterial);
        }

        if (_detector == null) return;

        // AprilTag detection
        var fov = Camera.main.fieldOfView * Mathf.Deg2Rad;
        _detector.ProcessImage(image, fov, _tagSize);

        Debug.Log("_detector.DetectedTags = " + _detector.DetectedTags.Count);

        // detected tag visualization
        foreach (var tag in _detector.DetectedTags)
            _drawer.Draw(tag.ID, ARCamera.transform.TransformPoint(tag.Position), ARCamera.transform.rotation * tag.Rotation, _tagSize);

        // visualize via marker mask 
        if (_detector.DetectedTags.Count > 0)
        {
            marker.transform.position = ARCamera.transform.TransformPoint(_detector.DetectedTags[0].Position);
            marker.transform.rotation = ARCamera.transform.rotation * _detector.DetectedTags[0].Rotation;
        }


        // align 
        if (enableAlignment && _detector.DetectedTags.Count > 0)
        {
            var tag = _detector.DetectedTags[0];
            ARObjectOrigin.transform.position = ARCamera.transform.TransformPoint(tag.Position);
            ARObjectOrigin.transform.rotation = ARCamera.transform.rotation * tag.Rotation;
        }

        // Profile data output (with 30 frame interval)
        //if (Time.frameCount % 30 == 0)
        //    _debugText.text = _detector.ProfileData.Aggregate
        //      ("Profile (usec)", (c, n) => $"{c}\n{n.name} : {n.time}");
    }
    public void toggleAlignmentEnabled()
    {
        enableAlignment = !enableAlignment;
    }
}
