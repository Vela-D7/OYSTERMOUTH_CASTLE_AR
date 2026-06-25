using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class OystermouthSimulatorWindow : EditorWindow
{
    private const string ConfessionScenePath = "Assets/CONFESSION_ROOM_AR.unity";
    private const string AreaTargetName = "AreaTarget";
    private const string ArCameraName = "ARCamera";
    private const string SimulatorTitle = "Simulator";

    private Vector2 scrollPosition;

    [MenuItem("Oystermouth/Simulator")]
    [MenuItem("Window/Oystermouth/Simulator")]
    public static void ShowWindow()
    {
        var window = GetWindow<OystermouthSimulatorWindow>(SimulatorTitle);
        window.minSize = new Vector2(320.0f, 360.0f);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawHeader();
        DrawSceneControls();
        DrawVuforiaControls();
        DrawContentControls();
        DrawEditorCameraControls();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField("Oystermouth AR Simulator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Dock this tab beside Scene/Game view for quick Vuforia scene checks. It does not replace device testing.",
            MessageType.Info);
    }

    private void DrawSceneControls()
    {
        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open Confession Room"))
                OpenScene();

            using (new EditorGUI.DisabledScope(!EditorSceneManager.GetActiveScene().isDirty))
            {
                if (GUILayout.Button("Save Scene"))
                    EditorSceneManager.SaveOpenScenes();
            }
        }

        EditorGUILayout.LabelField("Active Scene", EditorSceneManager.GetActiveScene().name);
    }

    private void DrawVuforiaControls()
    {
        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField("Vuforia", EditorStyles.boldLabel);

        GameObject areaTarget = FindSceneObject(AreaTargetName);
        GameObject arCamera = FindSceneObject(ArCameraName);

        DrawObjectRow("Area Target", areaTarget);
        DrawObjectRow("AR Camera", arCamera);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(areaTarget == null))
            {
                if (GUILayout.Button("Select AreaTarget"))
                    SelectAndPing(areaTarget);
            }

            using (new EditorGUI.DisabledScope(arCamera == null))
            {
                if (GUILayout.Button("Select ARCamera"))
                    SelectAndPing(arCamera);
            }
        }
    }

    private void DrawContentControls()
    {
        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField("AR Content", EditorStyles.boldLabel);

        GameObject fresco = FindSceneObject("Curved_Angel_Fresco_UnityOnly");
        GameObject crucifixion = FindSceneObject("Meshy_AI_the_Crucifixion_0331180948_texture");
        GameObject madonna = FindSceneObject("Meshy_AI_Madonna_and_Child_0331183623_texture");
        GameObject shield = FindSceneObject("Azure_Shield_with_a_G_0310021251_texture");

        DrawObjectToggle("Angel Fresco", fresco);
        DrawObjectToggle("Crucifixion", crucifixion);
        DrawObjectToggle("Madonna and Child", madonna);
        DrawObjectToggle("Shield", shield);
    }

    private void DrawEditorCameraControls()
    {
        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField("Scene View", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Frame AreaTarget"))
                FrameObject(FindSceneObject(AreaTargetName));

            if (GUILayout.Button("Frame Fresco"))
                FrameObject(FindSceneObject("Curved_Angel_Fresco_UnityOnly"));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Play"))
                EditorApplication.isPlaying = true;

            if (GUILayout.Button("Stop"))
                EditorApplication.isPlaying = false;
        }
    }

    private static void DrawObjectRow(string label, GameObject target)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(110.0f));

            if (target == null)
            {
                EditorGUILayout.LabelField("Missing", EditorStyles.miniLabel);
                return;
            }

            EditorGUILayout.LabelField(target.activeInHierarchy ? "Active" : "Inactive", EditorStyles.miniLabel);
        }
    }

    private static void DrawObjectToggle(string label, GameObject target)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(140.0f));

            if (target == null)
            {
                EditorGUILayout.LabelField("Missing", EditorStyles.miniLabel);
                return;
            }

            EditorGUI.BeginChangeCheck();
            bool active = EditorGUILayout.Toggle(target.activeSelf, GUILayout.Width(32.0f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Toggle Simulator Object");
                target.SetActive(active);
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(target.scene);
            }

            if (GUILayout.Button("Select", GUILayout.Width(64.0f)))
                SelectAndPing(target);
        }
    }

    private static void OpenScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ConfessionScenePath);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name != objectName)
                continue;

            if (EditorUtility.IsPersistent(obj))
                continue;

            return obj;
        }

        return null;
    }

    private static void SelectAndPing(Object target)
    {
        if (target == null)
            return;

        Selection.activeObject = target;
        EditorGUIUtility.PingObject(target);
    }

    private static void FrameObject(GameObject target)
    {
        if (target == null)
            return;

        SelectAndPing(target);

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
            return;

        Bounds bounds = CalculateBounds(target);
        sceneView.Frame(bounds, false);
    }

    private static Bounds CalculateBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
            return new Bounds(target.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}
