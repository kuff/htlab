using System;
using System.Linq;
using Leap;
using Leap.Unity;
using UnityEngine;

/*
 * Switch between supported hand tracking providers
 */
public class TrackingManager : MonoBehaviour
{
    private enum TrackingProvider
    {
        Ultraleap,
        Oculus,
        Both
    }
    
    private GameObject[] _ultraleapObjects;
    private GameObject[] _oculusObjects;

    protected void Start()
    {
        _ultraleapObjects   = new GameObject[1];
        _oculusObjects      = new GameObject[2];
        
        // Find and save object(s) related to each hand tracking provider
        // Ultraleap
        var localLeapProvider = FindObjectOfType<LeapXRServiceProvider>()?.gameObject;
        if (localLeapProvider is null) 
            Debug.LogError("Unable to locate Ultraleap GameObjects");
        else 
            _ultraleapObjects[0] = localLeapProvider;
        
        // Oculus
        var localOVRProvider = FindObjectsOfType<OVRHand>();
        if (localOVRProvider is null || localOVRProvider.Length != 2)
            Debug.LogError("Unable to locate Oculus GameObjects");
        else
            _oculusObjects = localOVRProvider.Select(hand => hand.gameObject).ToArray();
    }

    protected void Update()
    {
        // Listen for key presses and change hand tracking provider accordingly
        if (Input.GetKeyDown("b"))
            ChangeTrackingProvider(TrackingProvider.Ultraleap);
        else if (Input.GetKeyDown("n"))
            ChangeTrackingProvider(TrackingProvider.Oculus);
        else if (Input.GetKeyDown("m"))
            ChangeTrackingProvider(TrackingProvider.Both);
        
        // Run continuous check to Ultraleap hands until both are found
        /*if (_ultraleapObjects[0] == null)
        {
            var localLeapProvider = FindObjectOfType<LeapXRServiceProvider>()?.gameObject;
            if (localLeapProvider != null) return;
            _ultraleapObjects[1] = localLeapProvider;
        }*/
    }

    private void ChangeTrackingProvider(TrackingProvider tp)
    {
        // Switch between hand tracking providers
        switch (tp)
        {
            case TrackingProvider.Ultraleap:
                // Disable Oculus and enable Ultraleap
                foreach (var go in _oculusObjects)      go.SetActive(false);
                foreach (var go in _ultraleapObjects)   go.SetActive(true);
                break;
            case TrackingProvider.Oculus:
                // Disable Ultraleap and enable Oculus
                foreach (var go in _ultraleapObjects)   go.SetActive(false);
                foreach (var go in _oculusObjects)      go.SetActive(true);
                break;
            case TrackingProvider.Both:
                // Enable both hand tracking providers
                foreach (var go in _oculusObjects)      go.SetActive(true);
                foreach (var go in _ultraleapObjects)   go.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tp), tp, "Unsupported Tracking Provider given");
        }
    }
}
