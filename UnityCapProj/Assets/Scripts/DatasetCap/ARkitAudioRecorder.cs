using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARkitAudioRecorder : MonoBehaviour
{
    
    [SerializeField] private ARFaceManager _arFaceManager;
    private ARFace _arFace;

    private SkinnedMeshRenderer _smr;

    public Button startRecord;

    private bool _isStarted = false;

    private string _microPhoneDevices;

    private string _device;

    private int _freq;
    
    private StringBuilder sb = new StringBuilder(100000);
    // Start is called before the first frame update
    void Start()
    {
        var devices = Microphone.devices;

        foreach (var device in devices)
        {
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
            Debug.Log(string.Format("name = {0}  minfreq = {1} maxfreq = {2}", device, minFreq, maxFreq));
            _device = device;
            _freq = maxFreq;
        }

        if (startRecord != null)
        {
            startRecord.onClick.AddListener(() =>
            {
                _isStarted = !_isStarted ;

                if (_isStarted)
                {
                    Microphone.Start(_device, true, 1000, _freq);
                }
                else
                {
                    Microphone.End(_device);
                }
            });
        }
        
        if (!Directory.Exists(String.Format("{0}/{1}", Application.persistentDataPath,
            "AudioDataset")))
        {
            Directory.CreateDirectory(String.Format("{0}/{1}/", Application.persistentDataPath,
                "AudioDataset"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_arFaceManager.trackables.count == 0)
        {
            return;
        }

        foreach (var trackable in _arFaceManager.trackables)
        {
            _arFace = trackable;
            _smr = _arFace.GetComponentInChildren<SkinnedMeshRenderer>();
            break;
        }

        if (_smr == null)
            return;

        if (_isStarted)
        {
            const string seprator = ",";
            
            for (int i = 0; i < _smr.sharedMesh.blendShapeCount; i++)
            {
                sb.Append(_smr.GetBlendShapeWeight(i) + seprator);
            }

            sb.Append("\n");

            
            File.WriteAllText(String.Format("{0}/{1}/{2}", Application.persistentDataPath,
                "AudioDataset", ""), sb.ToString());
        }
    }
}
