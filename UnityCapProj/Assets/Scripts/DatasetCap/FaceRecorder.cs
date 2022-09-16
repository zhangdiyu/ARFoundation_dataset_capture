using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;

public class FaceRecorder : MonoBehaviour
{

    [SerializeField] private ARCameraBackground _arCameraBackground;

    private ARFace _arFace;

    [SerializeField] private ARCameraManager _arCameraManager;

    [SerializeField] private ARFaceManager _arFaceManager;

    public ARFaceMeshVisualizer arfm;
    private GameObject facePrefab;

    private NativeArray<Vector3> vertices;

    private string v_index = "466;939;937;935;933;989;987;985;1049;982;1051;1053;1055;1057;1059;1008;1011;200;198;335;328;348;850;764;762;646;648;15;13;10;8;308;77;4;526;743;1101;1096;1093;1193;1085;1106;789;1077;1074;1180;1064;1061;188;108;94;21;543;557;639;713;706;28;271;278;406;97;23;546;825;725;25;290";
    // private string v_index = "0; 360; 223; 1196";
    private List<int> indexes;

    private float timeAccu = 0;

    private Texture2D _currentCam;
    private const float shotPerSecond = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        // facePrefab = _arFaceManager.facePrefab;
        indexes = v_index.Split(';').Select(Int32.Parse).ToList();
        // Screen.SetResolution(1080, 1440, true);
        
    }

    public static string MeshToString(MeshFilter mf, Transform t)
    {
        int StartIndex = 0;
        Quaternion r 	= t.localRotation;
 
 
        int numVertices = 0;
        Mesh m = mf.sharedMesh;
        if (!m)
        {
            return "####Error####";
        }
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
 
        StringBuilder sb = new StringBuilder();
 
        foreach(Vector3 vv in m.vertices)
        {
            Vector3 v = t.TransformPoint(vv);
            numVertices++;
            sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,-v.z));
        }
        sb.Append("\n");
        foreach(Vector3 nn in m.normals) 
        {
            Vector3 v = r * nn;
            sb.Append(string.Format("vn {0} {1} {2}\n",-v.x,-v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector3 v in m.uv) 
        {
            sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }
        for (int material=0; material < m.subMeshCount; material ++) 
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");
 
            int[] triangles = m.GetTriangles(material);
            for (int i=0;i<triangles.Length;i+=3) {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
                    triangles[i]+1+StartIndex, triangles[i+1]+1+StartIndex, triangles[i+2]+1+StartIndex));
            }
        }
 
        StartIndex += numVertices;
        return sb.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        timeAccu += Time.deltaTime;
        if (timeAccu - shotPerSecond > 0)
        {
            unsafe
            {
                if (_arFaceManager.trackables.count == 0)
                {
                    return;
                }

                foreach (var trackable in _arFaceManager.trackables)
                {
                    _arFace = trackable;
                    break;
                }
                
                vertices = _arFace.vertices;

                var guid = System.Guid.NewGuid().ToString("N");

                // XRCpuImage image;
                //
                // if (!_arCameraManager.TryAcquireLatestCpuImage(out image))
                //     return;
                //
                // var conversionParams = new XRCpuImage.ConversionParams
                // {
                //     // Get the entire image.
                //     inputRect = new RectInt(0, 0, image.width, image.height),
                //
                //     // Downsample by 2.
                //     outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
                //
                //     // Choose RGBA format.
                //     outputFormat = TextureFormat.RGB24,
                //     
                //     // Flip across the vertical axis (mirror image).
                //     transformation = XRCpuImage.Transformation.MirrorY
                // };
                //
                // int size = image.GetConvertedDataSize(conversionParams);
                //
                // // Allocate a buffer to store the image.
                // var buffer = new NativeArray<byte>(size, Allocator.Temp);
                //
                // // Extract the image data
                // image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
                //
                // image.Dispose();
                //
                //
                // // At this point, you can process the image, pass it to a computer vision algorithm, etc.
                // // In this example, you apply it to a texture to visualize it.
                //
                // // You've got the data; let's put it into a texture so you can visualize it.
                // _currentCam = new Texture2D(
                //     conversionParams.outputDimensions.x,
                //     conversionParams.outputDimensions.y,
                //     conversionParams.outputFormat,
                //     false);
                //
                // _currentCam.LoadRawTextureData(buffer);
                // _currentCam.Apply();
                //
                // // Done with your temporary data, so you can dispose it.
                // buffer.Dispose();
                //
                // var bytes = _currentCam.EncodeToPNG();

                // File.WriteAllBytes(String.Format("{0}/{1}.png",Application.persistentDataPath, guid), bytes);
                ScreenCapture.CaptureScreenshot(String.Format("{0}.png", guid));

                string screenPoint = "";
                string screenPoint3D = "";
                const string seprator = ";";
                const string template = "[{0}],[{1}]";
                foreach (var vertex in indexes)
                // for (int vertex = 0 ; vertex < vertices.Length; ++vertex)
                {
                    var world_pos = _arFace.transform.TransformPoint(vertices[vertex]);
                    // RuntimeDebugDraw.Draw.DrawText(world_pos, vertex.ToString(), Color.red, 10, 
                    // 0.2f);
                    var screen = Camera.current.WorldToScreenPoint(world_pos);
                    screenPoint += screen.ToString() + seprator;
                    screenPoint3D += world_pos.ToString() + seprator;
                }

                // File.WriteAllText(String.Format("{0}/{1}.txt",Application.persistentDataPath, guid), MeshToString(_arFace.GetComponent<MeshFilter>(), _arFace.transform));

                File.WriteAllText(String.Format("{0}/{1}.txt",Application.persistentDataPath, 
                guid), String.Format(template, screenPoint, screenPoint3D));
                
                timeAccu = 0;
            }
        }
    }
}
