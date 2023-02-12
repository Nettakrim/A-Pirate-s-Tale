using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class Settings : MonoBehaviour
{
    public static KeyCodeData up    = new KeyCodeData("up", KeyCode.W);
    public static KeyCodeData left  = new KeyCodeData("left", KeyCode.A);
    public static KeyCodeData down  = new KeyCodeData("down", KeyCode.S);
    public static KeyCodeData right = new KeyCodeData("right", KeyCode.D);

    public static BoolData compass      = new BoolData("compass", false, new UnityAction<bool>(Player.ToggleCompass));
    public static BoolData dangerSensor = new BoolData("dangersensor", false, null);

    private static KeyCode[] illegalControls = new KeyCode[] {
        KeyCode.Tab,
        KeyCode.Return,
        KeyCode.Escape,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.KeypadEnter
    };

    [SerializeField] private Text[] inputButtons;

    [SerializeField] private Toggle[] toggles;

    private int waitingOn = -1;

    private void Start() {
        inputButtons[0].text = up.Init().ToString();
        inputButtons[1].text = left.Init().ToString();
        inputButtons[2].text = down.Init().ToString();
        inputButtons[3].text = right.Init().ToString();
        toggles[0].isOn = compass.Init();
        toggles[1].isOn = dangerSensor.Init();
    }

    public void Update() {
        if (waitingOn != -1) {
            foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode) && !kcode.ToString().StartsWith("JoystickButton") && !illegalControls.Contains(kcode)) {
                    inputButtons[waitingOn].text = kcode.ToString();
                    KeyCodeData data = waitingOn switch {
                        0 => up, 1 => left, 2 => down, 3 => right, _ => null
                    };
                    data.Set(kcode);
                    data.Save();
                    StopRebind();
                }
            }
            if (Input.GetMouseButtonDown(0)) StopRebind();
        }
    }

    public void StartRebind(int button) {
        if (waitingOn != -1) inputButtons[waitingOn].text = inputButtons[waitingOn].text.Replace("*","");
        waitingOn = button;
        inputButtons[waitingOn].text = "*"+inputButtons[waitingOn].text.Replace("*","")+"*";
    }

    public void StopRebind() {
        if (waitingOn != -1) inputButtons[waitingOn].text = inputButtons[waitingOn].text.Replace("*","");
        waitingOn = -1;
    }

    public void Toggle(int toggle) {
        BoolData boolData = toggle == 0 ? compass : dangerSensor;
        boolData.Set(toggles[toggle].isOn);
        boolData.Save();
    }

    public abstract class Data<T> {
        public Data(string id, T defaultValue) {
            this.id = id;
            this.defaultValue = defaultValue;
        }

        public virtual T Init() {
            String data = PlayerPrefs.GetString(id, null);
            if (data == null) {
                Set(defaultValue);
            } else {
                Set(Parse(data));
            }
            return state;
        }

        public T state;
        public string id;
        public T defaultValue;

        public virtual T Get() {
            return state;
        }

        public virtual void Set(T t) {
            state = t;
        }

        public virtual void Save() {
            PlayerPrefs.SetString(id, StateToString());
        }

        protected abstract T Parse(string str);

        protected abstract string StateToString();
    }

    public class KeyCodeData : Data<KeyCode> {
        public KeyCodeData(string id, KeyCode defaultValue) : base(id, defaultValue) {
            
        }

        protected override KeyCode Parse(string str) {
            KeyCode keyCode;
            Enum.TryParse<KeyCode>(str, out keyCode);
            if (keyCode == KeyCode.None) return defaultValue;
            return keyCode;
        }

        protected override string StateToString() {
            return state.ToString();
        }
    }

    public class BoolData : Data<bool> {
        public BoolData(string id, bool defaultValue, UnityAction<bool> listener) : base(id, defaultValue) {
            this.listener = listener;
        }

        private UnityAction<bool> listener;

        protected override bool Parse(string str) {
            return str == "True";
        }

        protected override string StateToString() {
            return state.ToString();
        }

        public override void Set(bool t) {
            base.Set(t);
            if (listener != null) listener.Invoke(t);
        }
    }
}
