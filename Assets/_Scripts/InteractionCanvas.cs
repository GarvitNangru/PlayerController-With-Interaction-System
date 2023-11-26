using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionCanvas : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private Vector3 Offset;
    private Transform targetObject;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text ItemDescriptionText;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        ResetCanvas();
    }

    void LateUpdate()
    {
        if (targetObject == null)
        {
            ResetCanvas();
            return;
        }

        Vector3 pos = mainCam.WorldToScreenPoint(targetObject.position + Offset);
        if (transform.position != pos)
            transform.position = pos;
    }

    public void MoveToThisObject(Transform itemTransform, string itemName, string ItemDescription)
    {
        gameObject.SetActive(true);
        targetObject = itemTransform;
        itemNameText.text = itemName;
        ItemDescriptionText.text = ItemDescription;
    }
    public void ResetCanvas()
    {
        gameObject.SetActive(false);
        targetObject = null;
        itemNameText.text = "";
        ItemDescriptionText.text = "";
    }
}
