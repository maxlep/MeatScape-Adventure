using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthTickController : MonoBehaviour
{
    [SerializeField] private Image mainImage;

    public void SetColor(Color color)
    {
        mainImage.color = color;
    }
}
