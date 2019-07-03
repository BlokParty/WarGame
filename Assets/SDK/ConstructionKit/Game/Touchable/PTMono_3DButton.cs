using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTMono_3DButton : PTMono_Touchable
{
    Vector3 initLocalScale;
    public bool interactable = true;

    public delegate void Button3DDelegate(PTMono_3DButton button);
    public Button3DDelegate OnClicked;

    private void Awake()
    {
        dragable = false;
        gravity = 0;
    }

    private void Start()
    {
        initLocalScale = transform.localScale;
    }
    protected override void OnTouchDown()
    {
        //iTween.ScaleBy(gameObject, new Vector3(1, 0.5f, 1), transitionTimer);
    }
    protected override void OnTouchEnter()
    {
    }
    protected override void OnTouchExit()
    {
    }
    protected override void OnTouchUp()
    {
        //itween.scaleto(gameobject, initlocalscale, transitiontimer);
        //if (onclicked != null)
        //{
        //    onclicked(this);
        //}
    }
    protected override void OnTouchDrag()
    {
    }
}
