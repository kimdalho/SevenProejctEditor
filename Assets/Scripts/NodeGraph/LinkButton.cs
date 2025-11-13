using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Sprites;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public BaseNode MyNode;
    protected Camera cam;
    public bool isPrev;
    public bool isPress;
    public SpriteRenderer spRender;

    public event Action<Vector3> press;
    public event Action endPress;

    public Color off;
    public Color on;
    private Vector3 _dragOffset;
    private void Awake()
    {
        if (!cam) cam = Camera.main;
        spRender = GetComponent<SpriteRenderer>();
    }


    [SerializeField] protected Vector3 mousePos;

    public void OnMouseDown()
    {
        BeginPress();

        //
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);

        // 오브젝트의 현재 높이를 지나는 평면
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            _dragOffset = transform.position - hit;   // 중심-클릭지점 차이 보존
        }
        else
        {
            _dragOffset = Vector3.zero;
        }
    }

    private void BeginPress()
    {
        isPress = true;
        spRender.color = on;
    }

    private void OnPressing()
    {
        if (!isPress) return;

        // 1) 포인터 → 평면 히트
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            mousePos = hit + _dragOffset;  // 오프셋 적용
            //Debug.Log("xx 1" + mousePos);
        }
        else
        {
            mousePos = transform.position;
            //Debug.Log("xx 2" + mousePos);
        }

        press?.Invoke(mousePos);
    }

    private void EndPress()
    {
        endPress?.Invoke();
        if (!isPress) return;
        isPress = false;
        spRender.color = off;
        
    }




    private void FixedUpdate()
    {
        OnPressing();
    }
    public void OnMouseUp()
    {
        EndPress();
        
    }
}
