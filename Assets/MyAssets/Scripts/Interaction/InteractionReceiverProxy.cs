using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionReceiverProxy : MonoBehaviour
{
    [SerializeField] private InteractionReceiver interactionReceiver;

    public InteractionReceiver InteractionReceiver => interactionReceiver;
}
