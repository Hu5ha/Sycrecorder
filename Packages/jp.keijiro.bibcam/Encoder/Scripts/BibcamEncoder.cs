using UnityEngine;
using Bibcam.Common;

namespace Bibcam.Encoder {

public sealed class BibcamEncoder : MonoBehaviour
{
    #region Public accessors

    public float minDepth { get => _minDepth; set => _minDepth = value; }
    public float maxDepth { get => _maxDepth; set => _maxDepth = value; }
    public int ishsv { get => _ishsv; set => _ishsv = value; }
    public Texture EncodedTexture => _encoded;
    public Texture EncodedTexture2 => _encodedhue;

    #endregion

    #region Editable attributes

    [SerializeField] BibcamXRDataProvider _xrSource = null;
    [SerializeField] float _minDepth = 0.025f;
    [SerializeField] float _maxDepth = 5;
    [SerializeField] int _ishsv = 1;
    [SerializeField] Transform experienceOrigin;

    #endregion

    #region Hidden asset references

    [SerializeField] Shader _shader = null;
        [SerializeField] Shader _shaderhue = null;
        #endregion

        #region Private objects

        Material _material;
        Material _materialhue;
        RenderTexture _encoded;
        RenderTexture _encodedhue;
        GraphicsBuffer _metadata;
        GraphicsBuffer _metadatahue;
        Metadata[] _tempArray = new Metadata[1];
        Metadata[] _tempArrayhue = new Metadata[1];

        #endregion

        #region MonoBehaviour implementation

        void Start()
    {
        
            _shader = Shader.Find("Hidden/Bibcam/Encoder");
       
            _shaderhue = Shader.Find("Hidden/Bibcam/Encoderhue");
        _material = new Material(_shader);
            _materialhue = new Material(_shader);
            _encoded = GfxUtil.RGBARenderTexture(1920, 1080);
            _encodedhue = GfxUtil.RGBARenderTexture(1920, 1080);
            _metadata = GfxUtil.StructuredBuffer(18, sizeof(float));
            _metadatahue = GfxUtil.StructuredBuffer(18, sizeof(float));
            Application.onBeforeRender += OnBeforeApplicationRender;
    }

    void OnDestroy()
    {
        Destroy(_material);
            Destroy(_materialhue);
            Destroy(_encoded);
            Destroy(_encodedhue);

            _metadata.Dispose();
            _metadatahue.Dispose();
            Application.onBeforeRender -= OnBeforeApplicationRender;
    }

    #endregion

    #region Application level callback

    //
    // ARPoseDriver updates the camera transform in Application.onBeforeRender,
    // so we have to use it too.
    //
    // The current implementation is not perfect because it's not clear which
    // one is called first. We know that ARPoseDriver uses OnEnable to register
    // its event handler, so theirs might be called first...
    //
    // FIXME: To make the execution order clear, we should call ARPoseDriver.
    // PerformUpdate (private) via C# reflection. That's stil a hackish way to
    // solve the problem, though.
    //

    public bool generateTestPatterns = false;

        void OnBeforeApplicationRender()
        {

            //if (generateTestPatterns)
            //  {

            // }
            //else
            //{
            var tex = _xrSource.TextureSet; // we get from call back function AROcclusionFrameEventArgs
            if (tex.y == null) return;

            // Texture planes
            _material.SetTexture(ShaderID.TextureY, tex.y);
            _material.SetTexture(ShaderID.TextureCbCr, tex.cbcr);
            _material.SetTexture(ShaderID.EnvironmentDepth, tex.depth);
            _material.SetTexture(ShaderID.HumanStencil, tex.stencil);

            _materialhue.SetTexture(ShaderID.TextureY, tex.y);
            _materialhue.SetTexture(ShaderID.TextureCbCr, tex.cbcr);
            _materialhue.SetTexture(ShaderID.EnvironmentDepth, tex.depth);
            _materialhue.SetTexture(ShaderID.HumanStencil, tex.stencil);

            // Aspect ratio compensation (camera vs. 16:9)
            var aspectFix = 9.0f / 16 * tex.y.width / tex.y.height;
            _material.SetFloat(ShaderID.AspectFix, aspectFix);
            _materialhue.SetFloat(ShaderID.AspectFix, aspectFix);

            // Projection matrix
            var proj = _xrSource.ProjectionMatrix;
            proj[1, 1] = proj[0, 0] * 16 / 9; // Y-factor overriding (16:9)

            // Depth range
            var range = new Vector2(_minDepth, _maxDepth);
            _material.SetVector(ShaderID.DepthRange, range);
            _materialhue.SetVector(ShaderID.DepthRange, range);

            // Metadata
            _tempArray[0] = new Metadata(_xrSource.CameraTransform, proj, range, experienceOrigin);
            _tempArrayhue[0] = new Metadata(_xrSource.CameraTransform, proj, range, experienceOrigin);
            _metadata.SetData(_tempArray);
            _metadatahue.SetData(_tempArrayhue);
            _material.SetBuffer(ShaderID.Metadata, _metadata);
            _materialhue.SetBuffer(ShaderID.Metadata, _metadatahue);
            // Encoding and multiplexing
            Graphics.Blit(null, _encoded, _material);
            Graphics.Blit(null, _encodedhue, _materialhue);
            //}
        }

        #endregion
    }

} // namespace Bibcam.Encoder
