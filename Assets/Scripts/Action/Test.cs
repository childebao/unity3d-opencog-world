using UnityEngine;
using System.Collections;
using ProtoBuf;

namespace OpenCog
{

/// <summary>
/// The Test.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class Test : MonoBehaviour
{

  /////////////////////////////////////////////////////////

  #region Private Member Data

  /////////////////////////////////////////////////////////

  [HideInInspector]
  [SerializeField]
  private int m_ExamplePrivateVar = 0;

  /////////////////////////////////////////////////////////

  #endregion

  /////////////////////////////////////////////////////////

  #region Accessors and Mutators

  /////////////////////////////////////////////////////////

  [ExposeProperty]
  public int ExamplePublicVar
  {
    get
    {
      return m_ExamplePrivateVar;
    }

    set
    {
      m_ExamplePrivateVar = value;
    }
  }

  /////////////////////////////////////////////////////////

  #endregion

  /////////////////////////////////////////////////////////

  #region Public Member Functions

  /////////////////////////////////////////////////////////

  /// <summary>
  /// Called when the script instance is being loaded.
  /// </summary>
  void Awake()
  {

  }

  /// <summary>
  /// Use this for initialization
  /// </summary>
  void Start()
  {
  }

  /// <summary>
  /// Update is called once per frame.
  /// </summary>
  void Update()
  {
  }

  /// <summary>
  /// Called once per frame after all Update calls
  /// </summary>
  void LateUpdate()
  {

  }

  /// <summary>
  /// Raises the enable event when {Name} is loaded.
  /// </summary>
  void OnEnable()
  {
    Debug.Log
    (
      string.Format
      (
        "MonoBehaviour[{0}].OnEnable"
      , gameObject.name + "\\" + GetType().Name
      )
    );
  }

  /// <summary>
  /// Raises the disable event when {Name} goes out of
  /// scope.
  /// </summary>
  void OnDisable()
  {
    Debug.Log
    (
      string.Format
      (
        "MonoBehaviour[{0}].OnDisable"
      , gameObject.name + "\\" + GetType().Name
      )
    );
  }

  /// <summary>
  /// Raises the destroy event when {Name} is about to be
  /// destroyed.
  /// </summary>
  void OnDestroy()
  {
    Debug.Log
    (
      string.Format
      (
        "MonoBehaviour[{0}].OnDestroy"
      , gameObject.name + "\\" + GetType().Name
      )
    );
  }

  /////////////////////////////////////////////////////////

  #endregion

  /////////////////////////////////////////////////////////

  #region Private Member Functions

  /////////////////////////////////////////////////////////

  /////////////////////////////////////////////////////////

  #endregion

  /////////////////////////////////////////////////////////

}// class {Name}

}// namespace OpenCog



