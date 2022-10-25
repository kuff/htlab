using System;
using System.Collections.Generic;
using System.Linq;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Events;

/*
 * Switch between supported hand tracking providers
 */
public class TrackingManager : MonoBehaviour
{
    public enum TrackingProvider
    {
        Ultraleap,
        Oculus,
        Both
    }
    
    [Serializable]
    public class TrackingProviderChangeEvent : UnityEvent<TrackingProvider> { }
    [Tooltip("Invoked whenever the TrackingProvider changes")]
    public TrackingProviderChangeEvent onTrackingProviderChange = new TrackingProviderChangeEvent();

    [Tooltip("TrackingProvider to use on app startup.")]
    public TrackingProvider initialProvider;
    [HideInInspector]
    public TrackingProvider currentProvider;

    [Tooltip("KeyCode for enabling the Ultraleap hand tracking provider.")]
    public KeyCode ultraleapKeyCode;
    [Tooltip("KeyCode for enabling the Oculus hand tracking provider.")]
    public KeyCode oculusKeyCode;
    [Tooltip("KeyCode for enabling both Ultraleap and Oculus hand tracking providers simultaneously.")]
    public KeyCode bothKeyCode;
    
    private GameObject[] _ultraleapObjects;  // The GameObjects which must be enabled/disabled to toggle Ultraleap
    private GameObject[] _oculusObjects;     // The GameObjects which must be enabled/disabled to toggle Oculus

    protected void Start()
    {
        // Check validity of KeyCodes
        var keyCodeSet = new HashSet<KeyCode>
        {
            ultraleapKeyCode,
            oculusKeyCode,
            bothKeyCode
        };
        if (keyCodeSet.Count != 3)
            Debug.LogError("Overlapping KeyCode values given");

        // Find and save object(s) related to each hand tracking provider
        _ultraleapObjects   = new GameObject[1];
        _oculusObjects      = new GameObject[2];
        
        // ...Ultraleap
        var localLeapProvider = FindObjectOfType<LeapXRServiceProvider>()?.gameObject;
        if (localLeapProvider is null) 
            Debug.LogError("Unable to locate Ultraleap GameObjects in scene");
        else 
            _ultraleapObjects[0] = localLeapProvider;
        
        // ...Oculus
        var localOVRProvider = FindObjectsOfType<OVRHand>();
        if (localOVRProvider is null || localOVRProvider.Length != 2)
            Debug.LogError("Unable to locate Oculus GameObjects in scene");
        else
            _oculusObjects = localOVRProvider.Select(hand => hand.gameObject).ToArray();
        
        // Set initial provider
        ChangeTrackingProvider(initialProvider, doNotInvoke:true);
    }

    protected void Update()
    {
        // Listen for key presses and change hand tracking provider accordingly
        if (Input.GetKeyDown(ultraleapKeyCode))
            ChangeTrackingProvider(TrackingProvider.Ultraleap);
        else if (Input.GetKeyDown(oculusKeyCode))
            ChangeTrackingProvider(TrackingProvider.Oculus);
        else if (Input.GetKeyDown(bothKeyCode))
            ChangeTrackingProvider(TrackingProvider.Both);
    }

    private void ChangeTrackingProvider(TrackingProvider tp, bool doNotInvoke = false)
    {
        // Don't update if given provider is already active
        if (tp == currentProvider) return;
        
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
                throw new ArgumentOutOfRangeException(nameof(tp), tp, "Unsupported TrackingProvider given");
        }

        // Update state and invoke event
        currentProvider = tp;
        if (!doNotInvoke) onTrackingProviderChange.Invoke(tp);
    }
}
