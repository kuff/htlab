using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * Gather data on player hands/head and save to disc
 */
public class HandLogger : MonoBehaviour
{
    private const bool DO_LOGGING = true;
    private const int LOGGING_FREQUENCY = 2;
    
    private enum LogType
    {
        FPS,
        LeftHand,
        RightHand,
        Head,
        Focus,
        Memory,
        Quit
    }

    private OVRPlugin.HandState _hsLeft = new OVRPlugin.HandState();
    private OVRPlugin.HandState _hsRight = new OVRPlugin.HandState();
    private OVRSkeleton[] _skeletonObjects;
    private Transform _mainCameraTransform;
    
    private static readonly List<IEnumerable> _logQueue = new List<IEnumerable>();
#pragma warning disable CS0414
    private static bool _fileSystemOperationInProgress = false;
    private static readonly string _logFileName = $"/LOG_MOVE_ME_{System.DateTime.Now:HHmmss-ffff}.txt";
    private static float _pointOfLastWrite = -1f;
    private static int _frameCounter = 0;
    private static float _frameCountTimestamp = -1f;
    private static int _invokesSinceLastWrite = LOGGING_FREQUENCY - 1;
#pragma warning restore CS0414

    protected void OnEnable()
    {
        //Application.focusChanged += LogFocusChanged;
        Application.lowMemory += LogLowMemory;
        Application.quitting += OnApplicationQuit;
    }

    protected void Start()
    {
        _skeletonObjects = FindObjectsOfType<OVRSkeleton>();

        var cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        _mainCameraTransform = cameraObject!.transform;
    }

    protected void Update()
    {
#pragma warning disable CS0162
        if (!DO_LOGGING) return;
#pragma warning restore CS0162
        
        // Update OVR Hand States
        OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft,  ref _hsLeft);
        OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref _hsRight);

        // Log player hands- and head data
        Log(LogType.LeftHand,   listData: GetSortedValues(_hsLeft));
        Log(LogType.RightHand,  listData: GetSortedValues(_hsRight));
        Log(LogType.Head,       listData: GetSortedValues(_mainCameraTransform));
        
        // Determine if FPS is to be logged in this frame
        _frameCounter++;
        _frameCountTimestamp += Time.deltaTime;
        if (_frameCountTimestamp is -1 or > 1)
        {
            // Log FPS count and reset
            Log(LogType.FPS, listData: new List<int> { _frameCounter }, ignorePrevious:true);
            
            _frameCounter = 0;
            _frameCountTimestamp = 0;
            
            // Write data to disc after x seconds
            _invokesSinceLastWrite++;
            if (_invokesSinceLastWrite >= LOGGING_FREQUENCY && !_fileSystemOperationInProgress)
            {
                WriteToDisc();
                _invokesSinceLastWrite = 0;
            }
        }
    }

    private static IEnumerable<object> GetSortedValues(OVRPlugin.HandState inputHandState)
    {
        //var names = inputHandState.GetType().GetFields().Select(field => field.Name).ToList();
        //var values = inputHandState.GetType().GetFields().Select(field => field.GetValue(inputHandState)).ToList();
        
        var result = new List<object>
        {
            inputHandState.Status,
            inputHandState.HandConfidence,
            inputHandState.FingerConfidences,
            inputHandState.RootPose,
            inputHandState.PointerPose,
            inputHandState.BoneRotations,
            inputHandState.HandScale,
            inputHandState.Pinches,
            inputHandState.PinchStrength,
            inputHandState.RequestedTimeStamp,
            inputHandState.SampleTimeStamp
        };
        
        return result;
    }
    
    private static IEnumerable<object> GetSortedValues(Transform inputTransform)
    {
        var result = new List<object>
        {
            inputTransform.position, 
            inputTransform.rotation, 
            inputTransform.forward
        };
        return result;
    }

    private static void LogLowMemory()
    {
        Log(LogType.Memory);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Log(LogType.Focus, listData: new List<bool> { hasFocus });
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Provide end-of-file signifier and log the remaining data from memory before quitting
        // NOTE: We do this in the pause event because the quit event on Android is unreliable
        Log(LogType.Quit, ignorePrevious:true);
        WriteToDisc();
    }

    private void OnApplicationQuit()
    {
        OnApplicationFocus(true);
    }

    private static void Log(LogType type, IEnumerable listData = default, bool ignorePrevious = false)
    {
        var enumerator = listData?.GetEnumerator();
        switch (type) 
        {
            case LogType.FPS:
                enumerator!.MoveNext();
                _logQueue.Add("" + (int) LogType.FPS + " " + enumerator.Current);
                break;
            case LogType.LeftHand:
                break;
            case LogType.RightHand:
                break;
            case LogType.Head:
                break;
            case LogType.Focus:
                enumerator!.MoveNext();
                _logQueue.Add("" + (int) LogType.Focus + " " + ((bool)enumerator.Current! ? 1 : 0));
                break;
            case LogType.Memory:
                _logQueue.Add("" + (int) LogType.Focus);
                break;
            case LogType.Quit:
                _logQueue.Add("" + (int) LogType.Quit);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static bool WriteToDisc()
    {
        _fileSystemOperationInProgress = true;
#if UNITY_EDITOR
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + _logFileName;
#else  // NOTE: ASSUMING RELEASE BUILDS RUN ON DEVICE
        var path = Application.persistentDataPath + _logFileName;
#endif
        try
        {
            using (var sw = File.AppendText(path))
            {
                foreach (var list in _logQueue)
                    sw.WriteLine(list.ToString());
            }

            _logQueue.Clear();
            _fileSystemOperationInProgress = false;
            _pointOfLastWrite = Time.realtimeSinceStartup;
            return true;
        }
        catch (InvalidDataException e)
        {
            Debug.LogError("Target log path exists but is read-only\n" + e);
        }
        catch (PathTooLongException e)
        {
            Debug.LogError("Target log path name may be too long\n" + e);
        }
        catch (IOException e)
        {
            Debug.LogError("The disk may be full\n" + e);
        }

        // TODO: revert log file if write operations fail...

        _fileSystemOperationInProgress = false;
        _pointOfLastWrite = Time.realtimeSinceStartup;
        return false;
    }
}
