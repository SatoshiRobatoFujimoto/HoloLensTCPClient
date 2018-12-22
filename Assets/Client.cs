using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;
using HoloToolkit.Unity.InputModule;
using System;
using System.Linq;

public class Client : MonoBehaviour, IInputClickHandler
{

    public string IP;
    public int port;
    public Text connectButtonText;
    public TextMesh debugText;

    private PhotoCapture photoCaptureObject = null;
    private Texture2D targetTexture = null;
    private Resolution cameraResolution;

    private TcpNetworkClientManager client = null;
    
    // Use this for initialization
    void Start () {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        debugText.text = cameraResolution.width.ToString() + " " + cameraResolution.height.ToString();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        // targetTexture = new Texture2D(480, 270);
        // InputManager.Instance.PushFallbackInputHandler(gameObject);
        InputManager.Instance.AddGlobalListener(gameObject);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        //byte[] texByte = targetTexture.EncodeToJPG();
        //byte[] image = new byte[texByte.Length];
        //Array.Copy(texByte, image, texByte.Length);

        byte[] image = targetTexture.GetRawTextureData();
        client.SendImage(image);
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (client != null)
        {
            PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject)
            {
                photoCaptureObject = captureObject;
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.9f;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
                {
                    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });
        }
    }

    public void ConnectButtonClicked()
    {
        if(client != null)
        {
            Debug.Log("Disconnected");
            connectButtonText.text = "Connect";
            client = null;
        }
        else
        {
            Debug.Log("Connected");
            client = new TcpNetworkClientManager(IP, port);
            connectButtonText.text = "Disconnect";
        }
    }
}
