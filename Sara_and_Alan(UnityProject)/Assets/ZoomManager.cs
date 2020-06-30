using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomManager : MonoBehaviour
{

	private Camera cam;
	private float targetZoom;
	private float zoomFactor = 3F;
	[SerializeField]
	private float zoomLerpSpeed = 10F;

    // Start is called before the first frame update
    void Start()
    {
    	
        cam = Camera.main;
        targetZoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        float scrollData;
        scrollData = Input.GetAxis("Mouse ScrollWheel");

        Debug.Log("jaja");

        targetZoom -= scrollData * zoomFactor;
        cam.orthographicSize = Mathf.Lerp(
        	cam.orthographicSize, 
        	targetZoom, 
        	Time.deltaTime * zoomLerpSpeed
        ); 
    }
}
