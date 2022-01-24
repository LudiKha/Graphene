
//#if ODIN_INSPECTOR
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;

//namespace Graphene.Editor
//{
//  public class ViewRefDrawer : OdinValueDrawer<ViewRef>
//  {
//    protected override void DrawPropertyLayout(GUIContent label)
//    {
//      base.DrawPropertyLayout(label);
//      return;
//      var value = this.ValueEntry.SmartValue;
//      SirenixEditorGUI.BeginBox();
//      SirenixEditorGUI.BeginBoxHeader();
//      SirenixEditorGUI.EndBoxHeader();

//      SirenixEditorGUI.BeginVerticalMenuList("Views");
//      SirenixEditorGUI.BeginListItem();
//      //int selected = SirenixEditorFields.Dropdown(0, value.pl)
//      SirenixEditorGUI.EndListItem();
//      SirenixEditorGUI.EndVerticalMenuList();
//      SirenixEditorGUI.EndBox();
      
//    }
//  }
//    }
//#endif