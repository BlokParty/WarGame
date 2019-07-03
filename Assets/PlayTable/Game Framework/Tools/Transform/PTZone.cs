using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace PlayTable
{
    /// <summary>
    /// The place to organize multiple transforms
    /// </summary>
    public class PTZone : MonoBehaviour
    {
        #region fields
        /// <summary>
        /// The place where contains a bunch of PTObjects instances
        /// </summary>
        public Transform content;
        /// <summary>
        /// Determines animation speed
        /// </summary>
        public float arrangeAnimationTimer = 0.5f;
        /// <summary>
        /// Determines the refresh rate
        /// </summary>
        public float autoArrangeTimer = 0f;
        /// <summary>
        /// The max number of visible objects. Used ToggleVisibility.
        /// </summary>
        public int maxVisable = -1;
        public bool ignoreDisabledChildren = false;

        /// <summary>
        /// True if want to keep all children world scale, children scale will be updated when Arranging
        /// </summary>
        public bool controlChildrenWorldScale = false;
        /// <summary>
        /// The value to keep for children's world scale, children scale will be updated when Arranging
        /// </summary>
        public Vector3 childrenWorldScale = Vector3.one;
        public bool controlChildrenWorldEularAngles = false;
        public Vector3 childrenWorldEularAngles = Vector3.zero;
        /// <summary>
        /// Used for arranging children
        /// </summary>
        public Vector3 firstChildLocalPosition = Vector3.zero;
        /// <summary>
        /// The max num of objects in a dimension
        /// </summary>
        public int[] dimensionLimits = new int[] {};
        /// <summary>
        /// The spacing between children
        /// </summary>
        public Vector3[] dimensionSpacings = new Vector3[1];
        /// <summary>
        /// This determines if transforms mirror in each dimension, in terms of x, y and z.
        /// </summary>
        public Vector3[] dimensionIsSymmetric;
        /// <summary>
        /// This determines if transforms start from center
        /// </summary>
        public bool[] dimensionStartFromCenter;

        /*
        /// <summary>
        /// The maximum number of object this zone can take. Unlimited if value is negative.
        /// </summary>
        public int capacity = -1;
        /// <summary>
        /// Ignores the acceptedTypes list if true
        /// </summary>
        public bool acceptsAnyType = true;
        /// <summary>
        /// The class names of accepted objects
        /// </summary>
        public List<string> acceptedTypes;
        */
        #endregion

        #region Property
        /// <summary>
        /// Get the total number of current objects in the zone.
        /// </summary>
        public int Count { get {
                if (ignoreDisabledChildren)
                {
                    int ret = 0;
                    foreach (Transform child in content)
                    {
                        ret = child.gameObject.activeSelf ? ret + 1 : ret;
                    }
                    return ret;
                }
                else
                {
                    return content.childCount;
                }
            }
        }
        /// <summary>
        /// Return if the zone is arranging.
        /// </summary>
        public bool IsArranging { get; private set; }
        #endregion

        #region delegates
        public delegate void PTDelegateTransformFromTransform(Transform trans, Transform from);
        /// <summary>
        /// Invoked when the newcoming object passed Accepts check
        /// </summary>
        public PTDelegateTransformFromTransform OnAccepted;
        /// <summary>
        /// Invoked when the adding animation is done.
        /// </summary>
        public PTDelegateTransformFromTransform OnAdded;
        /// <summary>
        /// Invoked when a arrange coroutine is done
        /// </summary>
        public PTDelegateVoid OnArranged;
        /// <summary>
        /// Invoked when a ptTransform is dropped on this.
        /// </summary>
        public PTDelegateFollower OnDropped;
        /// <summary>
        /// Invoked when an acceptable ptTransform is dropped on this.
        /// </summary>
        public PTDelegateFollower OnDroppedAcceptable;

        #endregion

        #region Unity built-in
        private void OnEnable()
        {
            IsArranging = false;

            //Run the endless ContinuouslyArrange coroutine
            StartCoroutine(ContinuouslyArrange());
        }
        #endregion

        #region api
        public int IndexOf(Transform transform)
        {
            for(int i = 0; i < Count; ++i)
            {
                if (content.GetChild(i) == transform)
                {
                    return i;
                }
            }
            return -1;
        }
        public bool Contains(Transform trans)
        {
            foreach (Transform child in content)
            {
                if (child == trans)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Get the object by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>null if no legit object found</returns>
        public Transform Get(int index)
        {
            try
            {
                return content.GetChild(index);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Arrange all children by adding all of them
        /// </summary>
        /// <param name="timer"></param>
        /// <returns></returns>
        public IEnumerator ArrangeCoroutine(float timer)
        {
            if (!IsArranging)
            {
                IsArranging = true;
                for (int i = 0; i < Count; i++)
                {
                    Transform currChild = content.GetChild(i);
                    Add(currChild, timer);
                    if (currChild.GetComponent<Collider>())
                    {
                        currChild.GetComponent<Collider>().enabled = false;
                    }
                    currChild.ToggleVisibility(i < maxVisable || maxVisable < 0, PT.DEFAULT_TIMER);
                }
                yield return new WaitForSeconds(timer);
                for(int i = 0; i < Count; ++i)
                {
                    Transform currChild = content.GetChild(i);
                    if (currChild.GetComponent<Collider>())
                    {
                        currChild.GetComponent<Collider>().enabled = true;
                    }
                }
                IsArranging = false;

                if (OnArranged != null)
                {
                    OnArranged();
                }
            }
        }
        public void Arrange(float timer)
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(ArrangeCoroutine(timer));
            }
        }
        public void Arrange()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(ArrangeCoroutine(arrangeAnimationTimer));
            }
        }
        /// <summary>
        /// Add a transform to content. Ignoring Accepts method.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="siblingIndex"></param>
        /// <param name="timer"></param>
        /// <returns></returns>
        public IEnumerator AddCoroutine(Component component, int siblingIndex, float timer)
        {
            if (component != null)
            {
                Transform fromParent = component.transform.parent;
                //transform 's collider is being dragged
                Collider collider = component.GetComponent<Collider>();
                if (collider && collider.IsBeingDragged())
                {
                    yield break;
                }

                if (controlChildrenWorldEularAngles)
                {
                    component.transform.SetWorldRotation(childrenWorldEularAngles, timer);
                }
                component.transform.SetParent(content, siblingIndex);
                component.transform.SetLocalPosition(TargetLocalPositionOf(siblingIndex), timer);
                if (controlChildrenWorldScale)
                {
                    component.transform.SetWorldScale(childrenWorldScale, timer);
                }

                yield return new WaitForSeconds(timer);

                if (OnAdded != null)
                {
                    OnAdded(component.transform, fromParent);
                }
            }
        }
        public IEnumerator AddCoroutine(Component component, float timer)
        {
            int siblingIndex = component.transform.parent == content ? component.transform.GetSiblingIndex() : Int32.MaxValue;
            yield return AddCoroutine(component, siblingIndex, timer);
        }
        public IEnumerator AddCoroutine(Component component)
        {
            int siblingIndex = component.transform.parent == content ? component.transform.GetSiblingIndex() : Int32.MaxValue;
            yield return AddCoroutine(component, siblingIndex, PT.DEFAULT_TIMER);
        }
        public void Add(Component component, int siblingIndex, float timer)
        {
            StartCoroutine(AddCoroutine(component, siblingIndex, timer));
        }
        public void Add(Component component, float timer)
        {
            //Add to the end of children if trans is not child of content.
            if (component != null)
            {
                int siblingIndex = component.transform.parent == content ? component.transform.GetSiblingIndex() : Int32.MaxValue;
                Add(component, siblingIndex, timer);
            }
        }
        public void Add(Component component)
        {
            Add(component, PT.DEFAULT_TIMER);
        }
        /// <summary>
        /// Get the target world position in arrangement
        /// </summary>
        /// <param name="siblingIndex"> The given sibling index </param>
        /// <returns></returns>
        public Vector3 TargetWorldPositionOf(int siblingIndex)
        {
            if (siblingIndex < 0 || siblingIndex >= Count)
            {
                return Vector3.zero;
            }
            else
            {
                return transform.TransformPoint(TargetLocalPositionOf(siblingIndex));
            }
        }
        /// <summary>
        /// Get the target local position in arrangement
        /// </summary>
        /// <param name="siblingIndex"> The given sibling index </param>
        /// <returns></returns>
        public Vector3 TargetLocalPositionOf(int siblingIndex)
        {
            siblingIndex = siblingIndex > 0 ? siblingIndex : 0;
            siblingIndex = siblingIndex < Count ? siblingIndex : Count - 1;

            //Get valid count id limits
            int validCountOfLimits = 0;
            foreach (int i in dimensionLimits)
            {
                if (i <= 0)
                {
                    break;
                }
                validCountOfLimits++;
            }

            //Get dimension
            int totalDimension = validCountOfLimits < dimensionSpacings.Length ?
                validCountOfLimits + 1 : dimensionSpacings.Length;

            //Adjust sibling index by extra spacing
            int totalCountExtraSpacing = 0;
            for (int i = 0; i <= siblingIndex; ++i)
            {
                PTTransform currTrans = Get(i).GetComponent<PTTransform>();
                if (currTrans)
                {
                    totalCountExtraSpacing += currTrans.countExtraSpacing;
                }
            }
            int adjustedSiblingIndex = siblingIndex + totalCountExtraSpacing;

            //Get total offset
            Vector3 offset = Vector3.zero;
            for (int dimension = 0; dimension < totalDimension; dimension++)
            {
                //Get maxLowerDimension
                int capacityLowerDimension = 1;
                for (int j = 0; j < dimension; j++)
                {
                    capacityLowerDimension *= dimensionLimits[j];
                }

                //Calculate offset
                int indexInCurrDimension = (adjustedSiblingIndex / capacityLowerDimension) % 
                    (totalDimension - dimension > 1 ? dimensionLimits[dimension] : Int32.MaxValue);

                int currTotalLowerIncludesDimension = CurrTotalLowerIncludes(dimension);
                int capacityLowerExcludesDimension = CapacityLowerExcludes(dimension);
                int totalInTheSameDimension = capacityLowerExcludesDimension == 0 ? currTotalLowerIncludesDimension : currTotalLowerIncludesDimension / capacityLowerExcludesDimension + (currTotalLowerIncludesDimension % capacityLowerExcludesDimension > 0 ? 1: 0);
                //Debug.Log("capacityLowerExcludesDimension=" + capacityLowerExcludesDimension + " totalInTheSameDimension=" + totalInTheSameDimension);

                //+ currTotalLowerIncludesDimension % capacityLowerExcludesDimension == 0 ? 0 : 1;
                //Debug.Log(" totalInTheSameDimension=" + totalInTheSameDimension);


                /*Debug.Log(name
                    + " indexInCurrDimension=" + indexInCurrDimension
                    + " totalInTheSameDimension=" + totalInTheSameDimension
                    + " CurrTotalLowerIncludes(dimension)" + CurrTotalLowerIncludes(dimension)
                    + " CapacityLowerExcludes(dimension)" + CapacityLowerExcludes(dimension)
                    );
                */
                Vector3 currSpacing = dimensionSpacings.Length > dimension ? dimensionSpacings[dimension] : Vector3.zero;
                offset += GetIncreaseSpacing(IsSymmetricOnDimension(dimension), currSpacing, indexInCurrDimension, totalInTheSameDimension, dimension);
            }

            //Target local position
            return firstChildLocalPosition + offset;
        }
        public Vector3 IsSymmetricOnDimension(int dimension)
        {
            return dimension < dimensionIsSymmetric.Length ? dimensionIsSymmetric[dimension] : Vector3.zero;
        }
        private Vector3 GetIncreaseSpacing(Vector3 isSymmetric, Vector3 spacing, int indexInCurrDimension, int totalInTheSameDimension, int dimensionIndex)
        {
            return new Vector3(
                GetOffsetValue(isSymmetric.x != 0, spacing.x, indexInCurrDimension, totalInTheSameDimension, dimensionIndex),
                GetOffsetValue(isSymmetric.y != 0, spacing.y, indexInCurrDimension, totalInTheSameDimension, dimensionIndex),
                GetOffsetValue(isSymmetric.z != 0, spacing.z, indexInCurrDimension, totalInTheSameDimension, dimensionIndex));
        }
        private float GetOffsetValue(bool isSymmetric, float spacing, int indexInCurrDimension, int totalInTheSameDimension, int dimensionIndex)
        {
            //  |   | . |   |           4
            //  2   0   1   3

            //  |   |   |               3
            //  2   0   1

            //  |   | . |   |           4
            //  0   1   2   3

            //  |   |   |               3
            //  0   1   2

            float ret = 0;
            if (isSymmetric)
            {
                if (dimensionIndex >= dimensionStartFromCenter.Length || !dimensionStartFromCenter[dimensionIndex])
                {
                    //start from one side
                    ret = totalInTheSameDimension == 0 
                        ? 0 : ((float)indexInCurrDimension - ((float)totalInTheSameDimension) / 2.0f + 0.5f) * spacing;
                    /*Debug.Log(
                        "indexInCurrDimension=" + indexInCurrDimension
                        + " totalInTheSameDimension=" + totalInTheSameDimension
                        + " ret=" + ret);
                    */
                }
                else
                {
                    //start from center
                    float positivity = indexInCurrDimension % 2 == 0 ? -1 : 1;
                    float factor = totalInTheSameDimension % 2 == 0 ? (indexInCurrDimension / 2 + 0.5f) : (int)((indexInCurrDimension + 1) / 2);
                    ret = factor * positivity * spacing;
                }
            }
            else
            {
                ret = spacing * indexInCurrDimension;
            }
            return ret;
        }
        public int CapacityLowerExcludes(int dimension)
        {
            if (dimension <= 0)
            {
                return 0;
            }
            else
            {
                int ret = 1;
                for (int j = 0; j < dimension; j++)
                {
                    ret *= j < dimensionLimits.Length ? dimensionLimits[j] : Int32.MaxValue;
                }
                return ret;
            }
        }
        public int CapacityLowerIncludes(int dimension)
        {
            if (dimension < 0)
            {
                return 0;
            }
            else
            {
                int ret = 1;
                for (int j = 0; j <= dimension; j++)
                {
                    ret *= j < dimensionLimits.Length ? dimensionLimits[j] : Int32.MaxValue;
                }
                return ret;
            }

        }
        public int CurrTotalLowerIncludes(int dimension)
        {
            int maxLowerInclude = CapacityLowerIncludes(dimension);

            return Count < maxLowerInclude ? Count : maxLowerInclude;
        }
        /// <summary>
        /// Arrange every autoArrangeTimer seconds
        /// </summary>
        /// <returns>Set current position in interpolation</returns>
        private IEnumerator ContinuouslyArrange()
        {
            const float waitTime = 3;
            while (true)
            {
                if (autoArrangeTimer > 0)
                {
                    yield return ArrangeCoroutine(PT.DEFAULT_TIMER);
                    yield return new WaitForSeconds(autoArrangeTimer);
                }
                else
                {
                    //Check if the autoArrangeTimer is back on
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }
        /// <summary>
        /// Change the children sequence in transform order, but don't change the actual position in the scene
        /// </summary>
        public virtual void Shuffle()
        {
            int count = Count;
            if (count < 2)
            {
                return;
            }
            for (int i = 0; i < count - 1; i++)
            {
                Get(UnityEngine.Random.Range(i + 1, count - 1)).transform.SetSiblingIndex(i);
            }
        }
        /// <summary>
        /// Flip all the children Simultaneously
        /// </summary>
        /// <param name="timer">the entire time ccost for the animation</param>
        /// <param name="faceup">facing target. facing up if true.</param>
        /// <param name="keepTilt">doesn't change rotation aroung world Y</param>
        /// <returns>Set realtime rotation each frame</returns>
        public IEnumerator FlipTogetherCoroutine(float timer, bool faceup, bool keepTilt)
        {
            foreach (Transform child in content)
            {
                child.SetFacing(faceup, keepTilt, timer);
            }

            yield return new WaitForSeconds(timer);
            Arrange(timer);
        }
        /// <summary>
        /// Flip all the children one afer another
        /// </summary>
        /// <param name="timer">the time cost for the animation of each child</param>
        /// <param name="faceup">facing target. facing up if true.</param>
        /// <param name="keepTilt">doesn't change rotation aroung world Y</param>
        /// <returns>Set realtime rotation each frame</returns>
        public IEnumerator FlipOneByOne(float timer, bool faceup, bool keepTilt)
        {
            foreach (Transform child in content)
            {
                yield return child.SetFacingCoroutine(faceup, keepTilt, timer);
            }

            Arrange(timer);
        }
        /// <summary>
        /// Simply starts FlipTogetherCoroutine
        /// </summary>
        /// <param name="faceup">facing target. facing up if true.</param>
        public void FlipTogether(bool faceup)
        {
            FlipTogether( faceup, true, PT.DEFAULT_TIMER);
        }
        /// <summary>
        /// Simply starts FlipTogetherCoroutine
        /// </summary>
        /// <param name="timer">the entire time ccost for the animation</param>
        /// <param name="faceup">facing target. facing up if true.</param>
        /// <param name="keepTilt">doesn't change rotation aroung world Y</param>
        public void FlipTogether( bool faceup, bool keepTilt, float timer)
        {
            StartCoroutine(FlipTogetherCoroutine(timer, faceup, keepTilt));
        }
        /// <summary>
        /// Virtual function that determines if an object is acceptable when trying to add
        /// </summary>
        /// <param name="other">The object attepting to be added in</param>
        /// <returns>Success if accepted</returns>
        /*public virtual bool Accepts(PTTransform other)
        {
            if (acceptsAnyType)
            {
                return true;
            }
            else
            {
                if (!other || capacity > 0 && content.childCount >= capacity)
                {
                    return false;
                }
                else
                {
                    bool transformHasComponentOfAcceptedTypes = false;
                    foreach (string acceptedType in acceptedTypes)
                    {
                        if (other.GetComponent(acceptedType))
                        {
                            transformHasComponentOfAcceptedTypes = true;
                            break;
                        }
                    }

                    return acceptedTypes.Contains(other.TypeName) || transformHasComponentOfAcceptedTypes;
                }
            }
        }*/
        #endregion
    }
}