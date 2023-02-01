using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    private Layout layout;

    [SerializeField] private float localFollowSpeed;
    [System.NonSerialized] public Transform targetParent;

    [SerializeField] private GameObject prefab;

    private void Update() {
        for (int i = 0; i < layout.amount; i++) {
            Transform child = transform.GetChild(i);
            Vector3 target = layout.getPosition(i);
            if (targetParent != null) {
                float closestDistance = 1;
                Transform closestTarget = null;
                foreach (Transform targetChild in targetParent) {
                    float distance = Vector3.Distance(child.position, targetChild.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestTarget = targetChild;
                    }
                }
                if (closestDistance < 1) {
                    target = Vector3.Lerp(target, closestTarget.position, 1-closestDistance);
                }
                if (closestDistance < 0.1) {
                    Debug.Log("kill!");
                }
            }
            child.localPosition = Vector3.MoveTowards(child.localPosition, target, localFollowSpeed*Time.deltaTime);
        }
    }

    public void AddPeople(int add) {
        int oldAmount = layout == null ? 0 : layout.amount;
        SetLayout(oldAmount+add);
        for (int i = 0; i < add; i++) {
            Instantiate(prefab, transform).transform.localPosition = layout.getPosition(oldAmount+i);
        }
    }

    public int getSize() {
        return layout.amount;
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
