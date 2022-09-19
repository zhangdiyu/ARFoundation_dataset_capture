using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class BSAudioRecorder : MonoBehaviour
{
    
    [SerializeField] private ARFaceManager _arFaceManager;
    private ARFace _arFace;

    private SkinnedMeshRenderer _smr;

    public Image startRecord;

    private bool _isStarted = false;

    private string _microPhoneDevices;

    private string _device;

    private int _freq;

    private AudioClip _audio;
    
    private StringBuilder sb = new StringBuilder(100000);

    public float fps = 30;

    private float _elipseTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        var devices = Microphone.devices;

        foreach (var device in devices)
        {
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
            Debug.Log(string.Format("device name = {0}  minfreq = {1} maxfreq = {2}", device, minFreq, maxFreq));
            _device = device;
            _freq = maxFreq;

            if (_freq == 0)
                _freq = 44100;
        }

        if (_audio == null)
        {
            _audio = AudioClip.Create("FaceAudio", _freq, 1, _freq, true);
        }
        
        
        if (!Directory.Exists(String.Format("{0}/{1}", Application.persistentDataPath,
            "AudioDataset")))
        {
            Directory.CreateDirectory(String.Format("{0}/{1}", Application.persistentDataPath,
                "AudioDataset"));
        }
    }

    public void OnClick()
    {
        _isStarted = !_isStarted ;

        if (_isStarted)
        {
            _audio = Microphone.Start(_device, false, 600, _freq);
            startRecord.color = Color.green;
        }
        else
        {

            var data = GetRealAudio(ref _audio);
            Microphone.End(_device);

            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            SaveWav.Save(String.Format("{0}/{1}/{2}", Application.persistentDataPath,
                "AudioDataset", fileName+".wav"), _audio);
            File.WriteAllText(String.Format("{0}/{1}/{2}", Application.persistentDataPath,
                "AudioDataset", fileName+".csv"), sb.ToString());
            startRecord.color = Color.red;

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

            _elipseTime += Time.deltaTime;

            if (_elipseTime > 1 / fps)
            {
                _elipseTime -= 1 / fps;
                for (int i = 0; i < _smr.sharedMesh.blendShapeCount; i++)
                {
                    sb.Append(_smr.GetBlendShapeWeight(i) + seprator);
                }

                sb.Append("\n");
            }
        }
    }

    public static byte[] GetRealAudio(ref AudioClip recordedClip)
    {
        int position = Microphone.GetPosition(null);

        if (position <= 0 || position > recordedClip.samples)
        {
            position = recordedClip.samples;
        }

        float[] soundata = new float[position * recordedClip.channels];
        recordedClip.GetData(soundata, 0);

        recordedClip = AudioClip.Create(recordedClip.name, position,
            recordedClip.channels, recordedClip.frequency, false);
        recordedClip.SetData(soundata, 0);
        int rescaleFactor = 32767;
        byte[] outData = new byte[soundata.Length * 2];

        for (int i = 0; i < soundata.Length; i++)
        {
            short temshort = (short) (soundata[i] * rescaleFactor);
            byte[] temdata = BitConverter.GetBytes(temshort);
            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }

        Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
        return outData;
    }
   
}
