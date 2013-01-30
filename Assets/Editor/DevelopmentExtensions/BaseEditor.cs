using UnityEditor;
using UnityEngine;
using System.Collections;

public class BaseEditor< OCType > : Editor
  where OCType : MonoBehaviour
{

 OCType m_Instance;
 PropertyField[] m_fields;
 
 public void OnEnable()
 {
   m_Instance = target as OCType;
   m_fields = ExposeProperties.GetProperties( m_Instance );
 }
 
 public override void OnInspectorGUI () {
 
   if ( m_Instance == null )
     return;
 
   this.DrawDefaultInspector();
 
   ExposeProperties.Expose( m_fields );
 
 }
}