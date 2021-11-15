using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthTickController : MonoBehaviour
{
    [SerializeField] private Image mainImage;
    [SerializeField] private Image outlineImage;

    public Color GetOutlineColor()
    {
        return outlineImage.color;
    }

    public void SetColor(Color color)
    {
        mainImage.color = color;
    }

    public void SetOutlineColor(Color color)
    {
        outlineImage.color = color;
    }

    public void SetFill(bool fill)
    {
        mainImage.gameObject.SetActive(fill);
    }
}
