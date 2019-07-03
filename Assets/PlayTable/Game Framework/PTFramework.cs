﻿using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.UI;

namespace PlayTable
{
    #region PlayTable Delegates
    public delegate void PTDelegateGraphNode(List<PTGraphNode> node);
    public delegate void PTDelegateGraphEdge(List<PTGraphEdge> edge);
    public delegate void PTDelegateGraphEdgeTile(List<PTGraphEdgeTile> tile);
    public delegate void PTDelegateTouch(PTTouch touch);
    public delegate void PTDelegateExclusiveTouch(PTTouch touch, int count);
    public delegate void PTDelegateColliderMultiTouch(Collider collider, params PTTouch[] touches);
    public delegate void PTDelegateMultiTouch(params PTTouch[] touches);
    public delegate void PTDelegateTouchCollider(PTTouch touch, Collider collider);
    public delegate void PTDelegateTouchDraggable(PTTouch touch, PTTouchFollower draggable);
    public delegate void PTDelegateTransform(PTTransform obj);
    public delegate void PTDelegateListZone(List<PTZone> zonesHit, List<PTZone> zonesAccepted);
    public delegate void PTDelegateZone(PTZone zone);
    public delegate void PTDelegateFollower(PTTouchFollower follower);
    #endregion

    public static class PTFramework
    {
        public const string version = "1.1.1";
        public static Dictionary<string, string> versions
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "1.1.1", "06/21/2019 SDK player blueprint. Tabltop seating screen." },
                    { "1.1.0", "04/04/2019 Extensions: Migrated most of PTTransform api to UnityEngine.Transform. " +
                        "Removed Accepts from PTZone. PTZone can take UnityEngine.Transform and is more focusing on managing content children" },
                    { "1.0.3", "03/26/2019 PTTouch: added hitPointBegin" },
                    { "1.0.2", "03/21/2019 ptTransform: SnapToRotation, SnapToGrid and set world and local rotation" },
                    { "1.0.1", "03/20/2019 Removed collider requirement from PTTransform" },
                    { "1.0.0", "" }
                };
            }
        }

        #region Extensions
        #region UnityEngine.Transform
        public static Vector2 GetScreenPosition(this Transform trans)
        {
            return Camera.main.WorldToScreenPoint(trans.position);
        }
        public static Vector3 GetWorldScale(this Transform trans)
        {
            Vector3 productParentLocalScale = trans.GetParentLocalScaleProduct();
            return new Vector3(
                productParentLocalScale.x * trans.localScale.x,
                productParentLocalScale.y * trans.localScale.y,
                productParentLocalScale.z * trans.localScale.z);
        }
        public static IEnumerator SetLocalPositionCoroutine(this Transform trans, Vector3 target, float timer)
        {
            Vector3 init = trans.localPosition;
            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                coveredTime += Time.deltaTime;
                float frac = coveredTime / timer;
                frac = frac < 1 ? frac : 1;
                trans.localPosition = init + (target - init) * frac;
            }
            trans.localPosition = target;
        }
        public static void SetLocalPosition(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetLocalPositionCoroutine(target, timer));
        }
        public static IEnumerator SetWorldPositionCoroutine(this Transform trans, Vector3 target, float timer)
        {
            Vector3 posInit = trans.position;
            Vector3 difference = target - posInit;
            float coveredTime = 0;
            while (coveredTime < timer && trans)
            {
                coveredTime += Time.deltaTime;
                float frac = coveredTime / timer;
                frac = frac < 1 ? frac : 1;
                trans.position = posInit + difference * frac;
                yield return new WaitForEndOfFrame();
            }
            if (trans)
            {
                trans.position = target;
            }
        }
        public static void SetWorldPosition(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetWorldPositionCoroutine(target, timer));
        }
        public static IEnumerator SetLocalScaleCoroutine(this Transform trans, Vector3 target, float timer)
        {
            Vector3 init = trans.localScale;
            Vector3 difference = target - init;
            float coveredTime = 0;
            while (coveredTime < timer && trans)
            {
                coveredTime += Time.deltaTime;
                float frac = coveredTime / timer;
                frac = frac < 1 ? frac : 1;
                trans.localScale = init + difference * frac;
                yield return new WaitForEndOfFrame();
            }
            if (trans)
            {
                trans.localScale = target;
            }
        }
        public static void SetLocalScale(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetLocalScaleCoroutine(target, timer));
        }
        public static IEnumerator SetWorldScaleCoroutine(this Transform trans, Vector3 target, float timer)
        {
            Vector3 targetLocalScale = trans.GetLocalScaleByWorldScale(target);
            yield return trans.SetLocalScaleCoroutine(targetLocalScale, timer);
        }
        public static void SetWorldScale(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetWorldScaleCoroutine(target, timer));
        }
        public static IEnumerator PulseCoroutine(this Transform trans, Vector3 maxScale, Vector3 minScale, float timer, bool loop)
        {
            while (true)
            {
                yield return trans.SetLocalScaleCoroutine(maxScale, timer * 0.5f);
                yield return trans.SetLocalScaleCoroutine(minScale, timer * 0.5f);
                if (!loop)
                {
                    break;
                }
            }
        }
        public static void Pulse(this Transform trans, Vector3 maxScale, Vector3 minScale, float timer, bool loop)
        {
            trans.StartCoroutineSelf(trans.PulseCoroutine(maxScale, minScale, timer, loop));
        }
        public static void Pulse(this Transform trans)
        {
            trans.Pulse(1.2f * Vector3.one, 0.8f * Vector3.one, PT.DEFAULT_TIMER * 5, true);
        }
        public static IEnumerator SetSizeDeltaCoroutine(this Transform trans, Vector2 target, float timer)
        {
            RectTransform rectTransform = trans.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 init = rectTransform.sizeDelta;
                Vector2 difference = target - init;
                float coveredTime = 0;
                while (coveredTime < timer)
                {
                    yield return new WaitForEndOfFrame();
                    coveredTime += Time.deltaTime;
                    float frac = coveredTime / timer;
                    frac = frac < 1 ? frac : 1;
                    rectTransform.sizeDelta = init + difference * frac;

                }
                rectTransform.sizeDelta = target;

            }
        }
        public static void SetSizeDelta(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetSizeDeltaCoroutine(target, timer));
        }



        public static IEnumerator SetWorldRotationCoroutine(this Transform trans, Quaternion target, float timer)
        {
            Quaternion init = trans.rotation;

            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                coveredTime += Time.deltaTime;
                float frac = coveredTime / timer;
                trans.rotation = Quaternion.Slerp(init, target, frac);

            }
            trans.rotation = target;

            PTTransform ptTransform = trans.GetComponent<PTTransform>();
            if (ptTransform)
            {
                if (ptTransform.OnRotated != null)
                {
                    ptTransform.OnRotated();
                }
            }
        }
        public static void SetWorldRotation(this Transform trans, Quaternion target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetWorldRotationCoroutine(target, timer));
        }
        public static IEnumerator SetLocalRotationCoroutine(this Transform trans, Quaternion target, float timer)
        {
            Quaternion init = trans.localRotation;

            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                coveredTime += Time.deltaTime;
                float frac = coveredTime / timer;
                trans.localRotation = Quaternion.Slerp(init, target, frac);

            }
            trans.localRotation = target;

            PTTransform ptTransform = trans.GetComponent<PTTransform>();
            if (ptTransform)
            {
                if (ptTransform.OnRotated != null)
                {
                    ptTransform.OnRotated();
                }
            }
        }
        public static void SetLocalRotation(this Transform trans, Quaternion target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetLocalRotationCoroutine(target, timer));
        }
        public static IEnumerator SetWorldRotationCoroutine(this Transform trans, float targetAngle, PTAxis axis, float timer)
        {
            Vector3 eulerTarget = trans.eulerAngles;
            switch (axis)
            {
                case PTAxis.x:
                    eulerTarget = new Vector3(targetAngle, trans.eulerAngles.y, trans.eulerAngles.z);
                    break;
                case PTAxis.y:
                    eulerTarget = new Vector3(trans.eulerAngles.x, targetAngle, trans.eulerAngles.z);
                    break;
                case PTAxis.z:
                    eulerTarget = new Vector3(trans.eulerAngles.x, trans.eulerAngles.y, targetAngle);
                    break;
            }
            Quaternion rotationTarget = Quaternion.Euler(eulerTarget);
            yield return trans.SetWorldRotationCoroutine(rotationTarget, timer);
        }
        public static void SetWorldRotation(this Transform trans, float targetAngle, PTAxis axis, float timer)
        {
            trans.StartCoroutineSelf(trans.SetWorldRotationCoroutine(targetAngle, axis, timer));
        }
        public static IEnumerator SetWorldRotationCoroutine(this Transform trans, Vector3 eularAngle, float timer)
        {
            Quaternion target = Quaternion.Euler(eularAngle);
            yield return trans.SetWorldRotationCoroutine(target, timer);
        }
        public static void SetWorldRotation(this Transform trans, Vector3 eularAngle, float timer)
        {
            trans.StartCoroutineSelf(trans.SetWorldRotationCoroutine(eularAngle, timer));
        }
        public static IEnumerator SetFacingCoroutine(this Transform trans, bool isFaceUp, bool keepTilt, float timer)
        {
            //Quaternion target = Quaternion.LookRotation(isFaceUp ? -Vector3.up : Vector3.up, keepTilt ? transform.up : Vector3.forward);
            Quaternion target = Quaternion.LookRotation(keepTilt ? trans.forward : Vector3.forward, isFaceUp ? Vector3.up : -Vector3.up);
            yield return trans.SetWorldRotationCoroutine(target, timer);
        }
        public static void SetFacing(this Transform trans, bool isFaceUp, bool keepTilt, float timer)
        {
            trans.StartCoroutineSelf(trans.SetFacingCoroutine(isFaceUp, keepTilt, timer));
        }
        public static IEnumerator ShakeCoroutine(this Transform trans, Vector3 angle, Vector3 movement, int times, float timer)
        {
            throw new NotImplementedException();
        }
        public static void Shake(this Transform trans, Vector3 angle, Vector3 movement, int times, float timer)
        {
            trans.StartCoroutineSelf(trans.ShakeCoroutine(angle, movement, times, timer));
        }
        public static IEnumerator LookAtCoroutine(this Transform trans, Vector3 target, float timer)
        {
            Vector3 relativePos = target - trans.position;
            Quaternion rotationTarget = Quaternion.LookRotation(relativePos, Vector3.up);
            yield return trans.SetWorldRotationCoroutine(rotationTarget, timer);
        }
        public static void LookAt(this Transform trans, Vector3 target, float timer)
        {
            trans.StartCoroutineSelf(trans.LookAtCoroutine(target, timer));
        }
        public static bool SetParent(this Transform trans, Transform parent, int targetSiblingIndex)
        {
            if (trans)
            {
                trans.SetParent(parent);
                if (parent)
                {
                    targetSiblingIndex = targetSiblingIndex > 0 ? targetSiblingIndex : 0;
                    targetSiblingIndex = targetSiblingIndex < parent.childCount ? targetSiblingIndex : parent.childCount - 1;
                    trans.SetSiblingIndex(targetSiblingIndex);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public static Vector3 GetLocalScaleByWorldScale(this Transform trans, Vector3 worldScale)
        {
            Vector3 parentsScale = trans.GetParentLocalScaleProduct();
            return new Vector3(
                parentsScale.x != 0 ? worldScale.x / parentsScale.x : 0,
                parentsScale.y != 0 ? worldScale.y / parentsScale.y : 0,
                parentsScale.z != 0 ? worldScale.z / parentsScale.z : 0);
        }
        public static Vector3 GetParentLocalScaleProduct(this Transform trans)
        {
            if (trans)
            {
                Vector3 ret = Vector3.one;
                Transform parent = trans.parent;
                while (parent != null)
                {
                    Vector3 parentScale = parent.transform.localScale;
                    ret = new Vector3(
                        ret.x * parentScale.x, 
                        ret.y * parentScale.y, 
                        ret.z * parentScale.z);
                    parent = parent.parent;
                }
                return ret;
            }
            else
            {
                Debug.LogWarning("Input is null");
                return Vector3.zero;
            }
        }
        /// <summary>
        /// Return if the card is facing up, towards world axis Y
        /// </summary>
        public static bool IsFaceUp(this Transform trans)
        {
            return Vector3.Dot(trans.up, Vector3.up) > 0;
        }
        public static IEnumerator FlipCoroutine(this Transform trans, bool isTargetFaceUp, float timer)
        {
            yield return trans.SetFacingCoroutine(isTargetFaceUp, true, timer);
        }
        /// <summary>
        /// Default flip, flip to target facing using customized timer.
        /// </summary>
        /// <param name="timer">the timer to finish the animation</param>
        /// <param name="isTargetFaceUp">target facing is up</param>
        public static void Flip(this Transform trans, bool isTargetFaceUp, float timer)
        {
            trans.StartCoroutineSelf(trans.FlipCoroutine(isTargetFaceUp, timer));
        }
        /// <summary>
        /// Default flip, flip to the other side using default timer.
        /// </summary>
        public static void Flip(this Transform trans)
        {
            trans.Flip(!trans.IsFaceUp(), PT.DEFAULT_TIMER);
        }
        /// <summary>
        /// Snap this to a multiple of incrementInDegrees (around y axis)
        /// </summary>
        /// <param name="incrementInDegrees">The minimal degree to use for snapping</param>
        /// <param name="timer">The timer for animation</param>
        /// <returns></returns>
        public static int SnapToRotation(this Transform trans, int incrementInDegrees, float timer)
        {
            int myRotation = (int)trans.eulerAngles.y % 360;
            if (myRotation < 0) { myRotation = 360 + myRotation; }

            int remainder = myRotation % incrementInDegrees;
            int newRotation;
            if (remainder < incrementInDegrees / 2)
            {
                newRotation = (myRotation / incrementInDegrees) * incrementInDegrees;
            }
            else
            {
                newRotation = (myRotation / incrementInDegrees + 1) * incrementInDegrees;
            }
            trans.SetWorldRotation(Quaternion.Euler(trans.eulerAngles.x, newRotation, trans.eulerAngles.z), timer);

            return newRotation;
        }
        /// <summary>
        /// The method to snap this to a grid using interval and offset (around y axis)
        /// </summary>
        /// <param name="interval">The size of grid elements</param>
        /// <param name="offset">The offset of the grid</param>
        /// <param name="timer">The timer for animation</param>
        public static void SnapToGrid(this Transform trans, Vector2 interval, Vector2 offset, float timer)
        {
            float nearestX;
            float remainder = (trans.position.x + offset.x) % interval.x;
            if (remainder < 0) { remainder += interval.x; }
            if (remainder < interval.x / 2)
            {
                nearestX = trans.position.x - remainder;
            }
            else
            {
                nearestX = trans.position.x + (interval.x - remainder);
            }

            float nearestZ = trans.position.z;
            remainder = (trans.position.z + offset.y) % interval.y;
            if (remainder < 0) { remainder += interval.y; }
            if (remainder < interval.y / 2)
            {
                nearestZ = trans.position.z - remainder;
            }
            else
            {
                nearestZ = trans.position.z + (interval.y - remainder);
            }
            trans.SetWorldPosition(new Vector3(nearestX, trans.position.y, nearestZ), timer);
        }
        public static IEnumerator SetColorCoroutine(this Transform trans, Color target, float timer)
        {
            Graphic graphic = trans.GetComponent<Graphic>();
            if (graphic)
            {
                graphic.SetColor(target, timer);
            }
            Renderer renderer = trans.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.SetColor(target, timer);
            }
            yield return new WaitForSeconds(timer);
        }
        public static void SetColor(this Transform trans, Color target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetColorCoroutine(target, timer));
        }
        public static IEnumerator SetAlphaCoroutine(this Transform trans, float target, float timer)
        {
            Graphic graphic = trans.GetComponent<Graphic>();
            if (graphic)
            {
                graphic.SetAlpha(target, timer);
            }
            Renderer renderer = trans.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.SetAlpha(target, timer);
            }
            yield return new WaitForSeconds(timer);
        }
        public static void SetAlpha(this Transform trans, float target, float timer)
        {
            trans.StartCoroutineSelf(trans.SetAlphaCoroutine(target, timer));
        }
        public static IEnumerator BlinkCoroutine(this Transform trans, float timer, int times, float minAlpha, float maxAlpha)
        {
            while (times > 0)
            {
                yield return trans.SetAlphaCoroutine(maxAlpha, timer / 2.0f);
                yield return trans.SetAlphaCoroutine(minAlpha, timer / 2.0f);
                times--;
            }
        }
        public static void Blink(this Transform trans, float timer, int times, float minAlpha, float maxAlpha)
        {
            trans.StartCoroutineSelf(trans.BlinkCoroutine(timer, times, minAlpha, maxAlpha));
        }
        public static IEnumerator ToggleVisibilityCoroutine(this Transform trans, bool b, float timer)
        {
            yield return trans.SetAlphaCoroutine(b ? 1 : 0, timer);
        }
        public static void ToggleVisibility(this Transform trans, bool b, float timer)
        {
            trans.StartCoroutineSelf(trans.ToggleVisibilityCoroutine(b, timer));
        }
        public static bool IsInZone(this Transform trans) { return trans.GetComponentInParent<PTZone>(); }
        public static bool IsInZone(this Transform trans, PTZone zone)
        {
            PTZone parentZone = trans.GetComponentInParent<PTZone>();
            return parentZone && parentZone == zone;
        }
        /// <summary>
        /// Mirror using negetive scale. Using this will trigger Unity's negative scaled collider not supported warning.
        /// </summary>
        /// <param name="trans">The transform to be mirrored</param>
        /// <param name="mirrorX"></param>
        /// <param name="mirrorY"></param>
        /// <param name="mirrorZ"></param>
        public static void Mirror(this Transform trans, bool mirrorX, bool mirrorY, bool mirrorZ)
        {
            Vector3 localScale = trans.localScale;
            trans.localScale = new Vector3(
                mirrorX ? -localScale.x : localScale.x,
                mirrorY ? -localScale.y : localScale.y,
                mirrorZ ? -localScale.z : localScale.z);
        }
        private static void AttemptMirror_HardToHandleCollider(this Transform trans, bool mirrorX, bool mirrorY, bool mirrorZ)
        {
            if (trans)
            {
                PTTransform ptTransform = trans.GetComponent<PTTransform>();

                //Mirror sprite renderer
                SpriteRenderer spriteRenderer = trans.GetComponent<SpriteRenderer>();
                if (ptTransform && !ptTransform.ignoreMirrorRotation || !ptTransform)
                {
                    AttemptMirrorSpriteRenderer_HardToHandleCollider(spriteRenderer, mirrorX, mirrorZ);
                }

                //Mirror the children local position
                foreach (Transform child in trans)
                {
                    PTTransform childPtTransform = child.GetComponent<PTTransform>();
                    if (childPtTransform && !childPtTransform.ignoreMirrorLocation || !childPtTransform)
                    {
                        AttemptMirrorLocation_HardToHandleCollider(child, mirrorX, mirrorY, mirrorZ);
                    }
                    Mirror(child, mirrorX, mirrorY, mirrorZ);
                }
            }
        }
        private static void AttemptMirrorLocation_HardToHandleCollider(this Transform trans, bool mirrorX, bool mirrorY, bool mirrorZ)
        {
            if (trans)
            {
                Transform parent = trans.parent;
                Vector3 worldPos = trans.position;
                Vector3 offsetParent = parent ? trans.position - parent.position : Vector3.zero;
                Vector3 target = new Vector3(
                    mirrorX ? worldPos.x - 2 * offsetParent.x : worldPos.x,
                    mirrorY ? worldPos.y - 2 * offsetParent.y : worldPos.y,
                    mirrorZ ? worldPos.z - 2 * offsetParent.z : worldPos.z);
                trans.position = target;
            }
        }
        private static void AttemptMirrorSpriteRenderer_HardToHandleCollider(SpriteRenderer spriteRenderer, bool mirrorX, bool mirrorY)
        {
            if (spriteRenderer)
            {
                spriteRenderer.flipX = mirrorX ? !spriteRenderer.flipX : spriteRenderer.flipX;
                spriteRenderer.flipY = mirrorY ? !spriteRenderer.flipY : spriteRenderer.flipY;
            }
        }
        
        #endregion

        #region UnityEngine.Collider
        public static bool IsBeingDragged(this Collider collider)
        {
            return PTGlobalInput.IsDragging(collider);
        }
        #endregion

        #region System.String
        public static bool Contains(this string source, string value, StringComparison comp)
        {
            return source == null ? false : source.IndexOf(value, comp) >= 0;
        }
        #endregion

        #region UnityEngine.UI.Graphic
        public static IEnumerator SetColorCoroutine(this Graphic graphic, Color target, float timer)
        {
            Color init = graphic.color;
            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                float frac = coveredTime / timer;
                graphic.color = Color.Lerp(init, target, frac);
                coveredTime += Time.deltaTime;
            }
            graphic.color = target;
        }
        public static void SetColor(this Graphic graphic, Color target, float timer)
        {
            graphic.StartCoroutine(graphic.SetColorCoroutine(target, timer));
        }
        public static IEnumerator SetAlphaCoroutine(this Graphic graphic, float target, float timer)
        {
            Color initColor = graphic.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, target);
            yield return graphic.SetColorCoroutine(targetColor, timer);
        }
        public static void SetAlpha(this Graphic graphic, float target, float timer)
        {
            graphic.StartCoroutine(graphic.SetAlphaCoroutine(target, timer));
        }
        #endregion

        #region UnityEngine.Renderer
        public static IEnumerator SetColorCoroutine(this Renderer renderer, Color target, float timer)
        {
            Color init = renderer.material.color;
            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                float frac = coveredTime / timer;
                renderer.material.color = Color.Lerp(init, target, frac);
                coveredTime += Time.deltaTime;
            }
            renderer.material.color = target;
        }
        public static void SetColor(this Renderer renderer, Color target, float timer)
        {
            renderer.StartCoroutineSelf(renderer.SetColorCoroutine(target, timer));
        }
        public static IEnumerator SetAlphaCoroutine(this Renderer renderer, float target, float timer)
        {
            Color initColor = renderer.material.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, target);
            yield return renderer.SetColorCoroutine(targetColor, timer);
        }
        public static void SetAlpha(this Renderer renderer, float target, float timer)
        {
            renderer.StartCoroutineSelf(renderer.SetAlphaCoroutine(target, timer));
        }
        #endregion

        #region UnityEngine.SpriteRenderer
        public static IEnumerator SetColorCoroutine(this SpriteRenderer renderer, Color target, float timer)
        {
            Color init = renderer.color;
            float coveredTime = 0;
            while (coveredTime < timer)
            {
                yield return new WaitForEndOfFrame();
                float frac = coveredTime / timer;
                renderer.color = Color.Lerp(init, target, frac);
                coveredTime += Time.deltaTime;
            }
            renderer.color = target;
        }
        public static void SetColor(this SpriteRenderer renderer, Color target, float timer)
        {
            renderer.StartCoroutineSelf(renderer.SetColorCoroutine(target, timer));
        }
        public static IEnumerator SetAlphaCoroutine(this SpriteRenderer renderer, float target, float timer)
        {
            Color initColor = renderer.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, target);
            yield return renderer.SetColorCoroutine(targetColor, timer);
        }
        public static void SetAlpha(this SpriteRenderer renderer, float target, float timer)
        {
            renderer.StartCoroutineSelf(renderer.SetAlphaCoroutine(target, timer));
        }
        #endregion

        #region UnityEngine.Vector2
        public static HashSet<Collider> GetHitsAsScreenPosition(this Vector2 screenPosition)
        {
            HashSet<Collider> ret = new HashSet<Collider>();
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit[] rayHits;
            rayHits = Physics.RaycastAll(ray, Camera.main.farClipPlane);
            foreach (RaycastHit currhit in rayHits)
            {
                Collider collider = currhit.collider.transform.GetComponent<Collider>();
                if (collider != null)
                {
                    ret.Add(collider);
                }
            }
            return ret;
        }
        #endregion

        #region UnityEngine.Component
        public static void StartCoroutineSelf(this Component component, IEnumerator coroutine)
        {
            MonoBehaviour starter = component.GetComponent<MonoBehaviour>();
            starter = starter != null ? starter : GameObject.FindObjectOfType<PTHelper>();
            starter = starter != null ? starter : GameObject.FindObjectOfType<PTInputManager>();
            starter = starter != null ? starter : Camera.main.GetComponent<MonoBehaviour>();

            if (starter && starter.isActiveAndEnabled)
            {
                starter.StartCoroutine(coroutine);
            }
        }
        #endregion

        #endregion
    }

    public static class PTUtility
    {
        #region API
        /// <summary>
        /// Toggle a game object's activity
        /// </summary>
        /// <param name="obj"></param>
        public static void ToggleActivity(GameObject obj)
        {
            obj.SetActive(!obj.activeSelf);
        }
        /// <summary>
        /// Get the length of total types in an enum
        /// </summary>
        /// <typeparam name="EnumType">The target enum type</typeparam>
        /// <returns></returns>
        public static int EnumLength<EnumType>()
        {
            if (typeof(EnumType).BaseType != typeof(Enum))
            {
                throw new InvalidCastException();
            }
            return Enum.GetNames(typeof(EnumType)).Length;
        }
        /// <summary>
        /// Get all colliders on the direction from a position to another
        /// </summary>
        /// <param name="fromWorldPosition"></param>
        /// <param name="toWorldPosition"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static HashSet<Collider> HitsRealtime(Vector3 fromWorldPosition, Vector3 toWorldPosition, float maxDistance)
        {
            //Debug.Log("HitsRealtime" + fromWorldPosition + toWorldPosition + " " + maxDistance);
            HashSet<Collider> ret = new HashSet<Collider>();
            RaycastHit[] rayHits;
            rayHits = Physics.RaycastAll(fromWorldPosition, toWorldPosition - fromWorldPosition, maxDistance);
            foreach (RaycastHit currhit in rayHits)
            {
                Collider collider = currhit.collider.transform.GetComponent<Collider>();
                if (collider != null)
                {
                    ret.Add(collider);
                }
            }
            return ret;
        }
        /// <summary>
        /// Generate a random name from the preset name pool
        /// </summary>
        /// <returns></returns>
        public static string RandName()
        {
            int countPresetName = Enum.GetNames(typeof(PresetName)).Length;
            return ((PresetName)UnityEngine.Random.Range(0, countPresetName)).ToString();
        }
        /// <summary>
        /// Return by probability (50% for instance)
        /// </summary>
        /// <param name="percentage">The probability of it happening. eg: 60</param>
        /// <returns></returns>
        public static bool Probability(float percentage)
        {
            //return by probablity (50% for instance)
            return UnityEngine.Random.Range(0, 101) <= percentage;
        }
        /// <summary>
        /// Generic method to swap two variables by ref
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="one">one of the parameter to swap</param>
        /// <param name="two">one of the parameter to swap</param>
        public static void Swap<T>(ref T one, ref T two)
        {
            T temp;
            temp = one;
            one = two;
            two = temp;
        }
        /// <summary>
        /// Generic method to swap two variables in a List
        /// </summary>
        /// <typeparam name="T">Type of objects in the list</typeparam>
        /// <param name="list">The list, where to swap two elements</param>
        /// <param name="indexA">One of the parameter index to swap</param>
        /// <param name="indexB">One of the parameter index to swap</param>
        public static void Swap<T>(List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
        /// <summary>
        /// Get relative angle between two Vector2. Up=0, Right=90, Down=180, Left=270
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float Angle(Vector2 origin, Vector2 position)
        {
            //angle and direction
            float rawAngle;
            Vector2 diff = new Vector2(position.x - origin.x, position.y - origin.y);
            rawAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            float ret = (90 - rawAngle) % 360f;
            ret = ret > 0 ? ret : ret + 360;
            return ret;
        }
        #endregion
    }
}
