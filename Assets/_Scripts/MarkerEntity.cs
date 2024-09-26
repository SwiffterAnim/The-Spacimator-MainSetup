using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerEntity : MonoBehaviour
{
    public Color defaultColor;

    public Color hoveredColor;

    public Color selectedColor;

    public Color ghostColor;

    public Color playingColor;

    public Vector3 hoveredScale;

    public Vector3 defaultScale;

    public int frameNumber;

    public bool isHovered;

    public bool isSelected;

    public bool isGhost;

    public bool isPlaying;
}
