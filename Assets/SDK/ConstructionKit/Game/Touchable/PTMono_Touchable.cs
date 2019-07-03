using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PTMono_Touchable : MonoBehaviour {
    public bool ignoreOverGameObject;
    public bool dragable = true;
    public float gravity = 1;

    //Touch
    Vector3 mouseDownOffset = Vector3.zero;
    float distance = 13.5f;

    public float transitionTimer = 0.2f;

    private void Start()
    {
        Fall();
    }

    private void FollowMouse()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 objPosition = mouseWorldPosition - mouseDownOffset;
        transform.position = objPosition;
    }
    protected void Fall()
    {
        if (gravity <= 0)
            return;
        else
        {
            Vector3 objPosition = new Vector3(transform.position.x, 0, transform.position.z);
            //iTween.MoveTo(gameObject, objPosition, transitionTimer/gravity);
        }
    }

    private void OnMouseEnter()
    {
        if (!ignoreOverGameObject && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        OnTouchEnter();
    }
    private void OnMouseExit()
    {
        if (!ignoreOverGameObject && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        OnTouchExit();
    }
    private void OnMouseDown()
    {
        if (!ignoreOverGameObject && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (dragable)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseDownOffset = mouseWorldPosition - new Vector3(transform.position.x, mouseWorldPosition.y, transform.position.z);

            FollowMouse();
        }

        OnTouchDown();
    }
    private void OnMouseUp()
    {
        if (!ignoreOverGameObject && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (dragable)
        {
            mouseDownOffset = Vector3.zero;
            Fall();
        }
        OnTouchUp();
    }
    private void OnMouseDrag()
    {
        if (!ignoreOverGameObject && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (dragable)
        {
            FollowMouse();
        }
        OnTouchDrag();
    }

    protected abstract void OnTouchDown();
    protected abstract void OnTouchExit();
    protected abstract void OnTouchEnter();
    protected abstract void OnTouchUp();
    protected abstract void OnTouchDrag();
}
