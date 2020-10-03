using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator)), CanEditMultipleObjects]
public class PlanetEditor : Editor {
    
    PlanetGenerator planetGenerator;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        DrawSettingsEditor(planetGenerator.shapeSettings, planetGenerator.GenerateObject);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsChanged){

        using (var check = new EditorGUI.ChangeCheckScope()){
            Editor editor = CreateEditor(settings);
            editor.OnInspectorGUI();

            if (check.changed){
                if(onSettingsChanged != null){
                    onSettingsChanged();
                }
            }
        }
    }

    private void OnEnable(){
        planetGenerator = (PlanetGenerator)target;
    }

}

