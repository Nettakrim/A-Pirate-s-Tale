using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    private Layout layout;

    [SerializeField] private float localFollowSpeed;
    [System.NonSerialized] public List<Transform> targetParents = new List<Transform>();

    [SerializeField] private GameObject prefab;

    private float updateLayoutAt;
    private bool needsUpdate;

    protected virtual void Update() {
        if (needsUpdate && updateLayoutAt - Time.time <= 0) {
            SetLayout(transform.childCount);
            needsUpdate = false;
        }

        Quaternion rotation = Quaternion.Inverse(transform.rotation);
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            Vector3 target = layout.getPosition(i);
            Vector3 worldTarget = transform.position+(rotation*target);
            if (targetParents.Count != 0) {
                float closestDistance = 1;
                Transform closestTarget = null;
                foreach (Transform targetParent in targetParents) {
                    bool pass = false;
                    if (pass) continue;
                    foreach (Transform targetChild in targetParent) {
                        float distance = Vector3.Distance(worldTarget, targetChild.position);
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestTarget = targetChild;
                        } else if (distance > 2) {
                            //if distance is more than 2 then theres no way anything else in the band will be close enough
                            pass = true;
                        }
                    }
                }
                if (closestDistance < 0.5f) {
                    float lerp = 1-(closestDistance*2);
                    target = Vector3.Lerp(target, (rotation*(closestTarget.position-child.position)+child.localPosition), lerp);
                }
                DistanceBehaviour(closestDistance, child, closestTarget);
            }
            child.localPosition = Vector3.MoveTowards(child.localPosition, target, localFollowSpeed*Time.deltaTime);
        }
    }

    protected virtual void DistanceBehaviour(float distance, Transform child, Transform target) {

    }

    public void AddPeople(int add) {
        int oldAmount = layout == null ? 0 : layout.amount;
        SetLayout(oldAmount+add);
        for (int i = 0; i < add; i++) {
            Instantiate(prefab, transform).transform.localPosition = layout.getPosition(oldAmount+i);
        }
    }

    public void KillRandom(int amount) {
        int children = transform.childCount;
        for (int i = 0; i < amount; i++) {
            Destroy(transform.GetChild(Random.Range(0,children-i)).gameObject);
        }
        ScheduleLayoutUpdate(0.5f);
    }

    public int getSize() {
        return layout.amount;
    }

    public void ScheduleLayoutUpdate(float inSeconds) {
        updateLayoutAt = Mathf.Max(Time.time + inSeconds, updateLayoutAt);
        needsUpdate = true;
    }

    public void SetLayout(int amount) {
        layout = getLayout(amount);
    }

    private static Layout[] layouts = new Layout[] {
        new Layout(false, 0, 0),
        new Layout(true, 0, 0),
        new Layout(false, 2, 0),
        new Layout(false, 3, 0),
        new Layout(true, 3, 0),
        new Layout(true, 4, 0),
    };

    private static Layout getLayout(int amount) {
        if (amount < layouts.Length) return layouts[amount];
        return new Layout(amount%2 != 0, amount/2, amount/2);
    }

    private class Layout {
        private bool center;
        private int inner;
        private int outer;
        public int amount;

        private static float innerDistance = 0.2f;
        private static float outerDistance = 0.35f;

        public Layout(bool center, int inner, int outer) {
            this.center = center;
            this.inner = inner;
            this.outer = outer;
            this.amount = inner+outer+(center?1:0);
        }

        public Vector3 getPosition(int index) {
            if (center) {
                if (index == 0) return Vector3.zero;
                index--;
            }
            if (index < inner) {
                //inner starting at 0
                float rad = (((float)index)/inner)*Mathf.PI*2;
                return new Vector3(Mathf.Sin(rad)*innerDistance,0,Mathf.Cos(rad)*innerDistance);
            } else {
                index -= inner;
                //outer starting at 0
                float rad = (((float)index)/outer)*Mathf.PI*2;
                rad += Mathf.PI/outer;
                return new Vector3(Mathf.Sin(rad)*outerDistance,0,Mathf.Cos(rad)*outerDistance);
            }
        }
    }
}
