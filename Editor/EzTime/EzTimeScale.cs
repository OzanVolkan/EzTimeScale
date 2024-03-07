using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EzTimeScale
{
    [InitializeOnLoad]
    public class TimeScaler
    {
        //Since the file path of the image we use in the toolbar and the min and max values of our slider will not change in runtime,
        //we can define these variables using the "const"(constant) keyword.
        //private const string TimeTexturePath = "Assets/EzTimeScale/TimeImg.png";
        private const float MinSliderValue = 0.0f;
        private const float MaxSliderValue = 10.0f;

        //All variables and methods we define using the "static" keyword are shared among all instances of this class,
        //and their values are stated to have the same value in all instances.
        static Texture2D _timeTexture;
        public static Texture2D TimeTexture
        {
            get
            {
                if (_timeTexture == null)
                {
                    return _timeTexture = Resources.Load<Texture2D>("TimeImg");
                }
                return _timeTexture;
            }
        }

        static float _sliderValue;
        static float _tempValue;
        static ScriptableObject m_currentToolbar;
        static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static VisualElement mRoot;

        //Static constructor for TimeScaler class
        static TimeScaler()
        {
            Initialize();
        }

        static void Initialize()
        {
            EditorApplication.update += OnUpdate; //Subscribe to the method that will run at every update time.
        }

        static void OnUpdate()
        {
            if (m_currentToolbar == null)
            {
                FindAndSetCurrentToolbar();
                if (m_currentToolbar != null)
                {
                    SetupToolbarUI();
                }
            }
        }

        static void FindAndSetCurrentToolbar()
        {
            var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
            m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null; //Assign the first toolbar found to the m_currentToolbar variable.
        }

        //Prepare the tools required to add a custom UI to Unity Editor's toolbar and assign a callback function to a specific region of this UI
        static void SetupToolbarUI()
        {
            var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var rawRoot = root.GetValue(m_currentToolbar);
            mRoot = rawRoot as VisualElement;

            RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUI);
        }

        //Add a custom slider to Unity Editor's toolbar and control the time scale through this slider.
        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Box(TimeTexture, GUILayout.Width(20f), GUILayout.Height(20f));
            GUILayout.Label("Time Scaler");
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, MinSliderValue, MaxSliderValue, GUILayout.Width(200f));
            GUILayout.TextField($"Time Scale: {_sliderValue:F1}".Replace(",", "."));
            GUILayout.Space(100f);
            GUILayout.EndHorizontal();


            if (_sliderValue != _tempValue)
            {
                Time.timeScale = _sliderValue;
                _tempValue = _sliderValue;
            }
        }

        //We add a UI element to a specific region in the Unity Editor's toolbar and control the visibility and behavior of this element with a callback function.
        static void RegisterCallback(string root, Action cb)
        {
            var toolbarZone = mRoot.Q(root);
            if (toolbarZone != null)
            {
                var parent = CreateParentElement();
                var container = CreateIMGUIContainer(cb);

                parent.Add(container);
                toolbarZone.Add(parent);
            }
        }

        //This method creates a Visual Element and determines the style of this element.
        static VisualElement CreateParentElement()
        {
            return new VisualElement()
            {
                style = {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                }
            };
        }

        //This method creates an IMGUIContainer instance and sets a callback for this container's style and onGUIHandler.
        static IMGUIContainer CreateIMGUIContainer(Action onGUIHandler)
        {
            var container = new IMGUIContainer();
            container.style.flexGrow = 1;

            container.onGUIHandler += () =>
            {
                onGUIHandler?.Invoke();
            };

            return container;
        }
    }
}
