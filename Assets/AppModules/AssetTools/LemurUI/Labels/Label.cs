using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  using UnityObject = UnityEngine.Object;

  #region TextStyle

  public struct TextStyle {
    public Maybe<Font> font;
    public int?        fontSize;
    public FontStyle?  fontStyle;
    public bool?       richText;
    public Color?      color;

    /// <summary>
    /// Returns a new style with the properties of the argument style overlayed onto this
    /// style. Any non-null properties of the argument style will overwrite the
    /// corresponding property on this style.
    /// </summary>
    public TextStyle OverlayedWith(TextStyle other) {
      return new TextStyle() {
        font = other.font.ValueOr(this.font),
        fontSize = other.fontSize.ValueOr(this.fontSize),
        fontStyle = other.fontStyle.ValueOr(this.fontStyle),
        richText = other.richText.ValueOr(this.richText),
        color = other.color.ValueOr(this.color)
      };
    }
  }

  #endregion

  #region TextFlow

  public struct TextFlow {
    public TextAlignment? alignment;
    public TextAnchor?    anchor;
    public float?         lineSpacing;
    public bool?          allowOverflowWidth;
    public bool?          allowOverflowHeight;

    /// <summary>
    /// Returns a new style with the properties of the argument style overlayed onto this
    /// style. Any non-null properties of the argument style will overwrite the
    /// corresponding property on this style.
    /// </summary>
    public TextFlow OverlayedWith(TextFlow other) {
      return new TextFlow() {
        alignment = other.alignment.ValueOr(this.alignment),
        anchor = other.anchor.ValueOr(this.anchor),
        lineSpacing = other.lineSpacing.ValueOr(this.lineSpacing),
        allowOverflowWidth = other.allowOverflowWidth.ValueOr(this.allowOverflowWidth),
        allowOverflowHeight = other.allowOverflowHeight.ValueOr(this.allowOverflowHeight),
      };
    }
  }

  #endregion

  public abstract class LemurUIElement {

    /// <summary>
    /// The Component driving the GameObject associated with this LemurUIElement.
    /// </summary>
    protected abstract IGameObjectDriver gameObjectDriver { get; }

    /// <summary>
    /// Retrieves the current GameObject associated with this LemurUIElement, creating
    /// one if necessary. These GameObjects are pooled; call Recycle() when you are
    /// don't need it anymore.
    /// </summary>
    public GameObject gameObject {
      get { return gameObjectDriver.gameObject; }
    }

    /// <summary>
    /// Call this when you're done with the UI element and it will have its allocated
    /// resources diverted into a pool for future spawns.
    /// </summary>
    public void Recycle() {
      gameObjectDriver.Recycle();
    }

    //public abstract LemurUIElement Duplicate();

  }

  public struct GameObjectComponentDescription : IEquatable<GameObjectComponentDescription> {
    private readonly Type _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7;
    private readonly Hash _typesHash;

    #region Constructors

    public GameObjectComponentDescription(Type t0) {
      _t0 = t0;   _t1 = null; _t2 = null; _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1) {
      _t0 = t0;   _t1 = t1;   _t2 = null; _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2) {
      _t0 = t0;   _t1 = t1;   _t2 = t2;   _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3) {
      _t0 = t0;   _t1 = t1;   _t2 = t2;   _t3 = t3;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4) {
      _t0 = t0; _t1 = t1;   _t2 = t2;   _t3 = t3;
      _t4 = t4; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5) {
      _t0 = t0; _t1 = t1; _t2 = t2;   _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5, Type t6) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = t6; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5, Type t6, Type t7) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = t6; _t7 = t7;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }

    public bool Equals(GameObjectComponentDescription other) {
      throw new NotImplementedException();
    }

    #endregion

    public override int GetHashCode() {
      return _typesHash.GetHashCode();
    }
  }

  public abstract class Label : LemurUIElement {

    #region Static

    /// <summary>
    /// The default label Rect is 10 cm by 4 cm.
    /// </summary>
    public static Rect DefaultRect {
      get { return new Rect(0f, 0f, 0.04f, 0.10f); }
    }

    private const string DEFAULT_TEXT = "New Label";
    /// <summary>
    /// "New Label".
    /// </summary>
    public static string DefaultText {
      get { return DEFAULT_TEXT; }
    }

    #endregion

    /// <summary>
    /// The local 2D bounding box for this label. Label rects are always in local space
    /// with the origin in the lower-left corner of the rect and with dimensions
    /// specified in meters. The default label Rect is 10 cm by 4 cm.
    /// </summary>
    public Rect      rect = DefaultRect;

    /// <summary>
    /// The text contained in this label.
    /// </summary>
    public string    text = DefaultText;

    /// <summary>
    /// Visual style data for how each character should be rendered, e.g. font and color.
    /// </summary>
    public TextStyle textStyle = default(TextStyle);

    /// <summary>
    /// Visual flow data for where the text should be rendered in its bounding rect,
    /// e.g. centering and line spacing.
    /// </summary>
    public TextFlow  textFlowConfig = default(TextFlow);

    /// <summary>
    /// The Type of the renderer being used to render the Label in the scene. May be
    /// null if the Label is not in active use by Unity.
    /// 
    /// For example, if this Label implementation is driving a TextMesh text renderer
    /// component, this type will be TextMesh.
    /// </summary>
    public abstract Type textRenderingType { get; }
  }

  /// <summary>
  /// Lemur UI types declare precisely what combination of components they require on
  /// their GameObjects, which are then generically pooled and managed.
  /// </summary>
  public interface IGameObjectDriver {
    /// <summary>
    /// The components that are required for the IGameObjectDriver to drive a GameObject.
    /// </summary>
    GameObjectComponentDescription requiredComponents { get; }

    // TODO: Make extension method
    //
    // Use DrivenGameObject to handle auto-creation of game objects.....
    // need to figure out the actual life-cycle here
    //
    ///// <summary>
    ///// Gets the GameObject that the IGameObjectDriver is driving; this is a
    ///// pooled resource that can be returned to the pool by calling Recycle(). If there
    ///// is no GameObject being driven yet, this getter will spawn one.
    ///// </summary>
    //GameObject gameObject {
    //  get {
    //    return driven.gameObject;
    //  }
    //}

    DrivenGameObject driven { get; }

    /// <summary>
    /// Recycles any allocated resources into an appropriate pool.
    /// </summary>
    void Recycle();
  }

  public class Label<TextRenderingComponent, Driver>
                 : Label
                 where TextRenderingComponent : Component
                 where Driver : LabelDriver<TextRenderingComponent> {

    public override Type textRenderingType {
      get { return typeof(TextRenderingComponent); }
    }

    private IGameObjectDriver _backingGameObjectDriver = null;
    protected override IGameObjectDriver gameObjectDriver {
      get { return gameObjectDriver; }
    }

    public Label() {
      _backingGameObjectDriver = new Driver();
    }

  }

  public abstract class GameObjectDriver<LemurUIElement>
                        : MonoBehaviour,
                          IGameObjectDriver
                        where LemurUIElement : LemurUI.LemurUIElement {

    /// <summary>
    /// The LemurUI element associated with this GameObjectDriver component. May be null
    /// if the driver is detached from the LemurUI system, otherwise this will be
    /// non-null while the driver is in active use by LemurUI.
    /// </summary>
    public LemurUIElement luie = null;

    /// <summary>
    /// For simple GameObjectDrivers that only need a GameObject and Transform, the
    /// GameObjectDriver implementation of requiredComponents simply returns the
    /// implementing type of this GameObjectDriver, so any new pooled GameObjects can be
    /// spawned with the implementing type as an added component.
    /// </summary>
    public virtual GameObjectComponentDescription requiredComponents {
      get {
        return new GameObjectComponentDescription(this.GetType());
      }
    }

    private DrivenGameObject _backingDrivenObjectWrapper;
    /// <summary>
    /// The DrivenGameObject, a wrapper class around the GameObject driven by this
    /// class that handles pooling.
    /// </summary>
    public DrivenGameObject drivenObjectWrapper {
      get {
        if (_backingDrivenObjectWrapper == null) {
          _backingDrivenObjectWrapper = DrivenGameObjectPool.Spawn(
                                          withComponents: requiredComponents);
        }
        return _backingDrivenObjectWrapper;
      }
    }

    public void Recycle() {
      drivenObjectWrapper.Recycle();
    }
  }

  public abstract class LabelDriver<TextRenderingComponent>
                          : GameObjectDriver<Label<TextRenderingComponent>>
                          where TextRenderingComponent : Component {

    /// <summary>
    /// A LabelDriver for a generic TextRenderingComponent requires only two components
    /// in most cases: the TextRenderingComponent, and this LabelDriver implemenation
    /// component for that renderer.
    /// </summary>
    public override GameObjectComponentDescription requiredComponents {
      get {
        return new GameObjectComponentDescription(
          typeof(TextRenderingComponent),
          this.GetType());
      }
    }
  }

  public class DrivenGameObjectPool {
    /// <summary>
    /// Pools dictionary, keying GameObjectComponentDescription.GetHash() results to
    /// GameObjects
    /// </summary>
    private static Dictionary<GameObjectComponentDescription,
                              DrivenGameObjectPool> _pools
      = new Dictionary<GameObjectComponentDescription, DrivenGameObjectPool>();

    private static DrivenGameObjectPool getOrCreatePool(GameObjectComponentDescription forComponents) {
      DrivenGameObjectPool pool;
      if (!_pools.TryGetValue(forComponents, out pool)) {
        _pools[forComponents] = pool = new DrivenGameObjectPool();
      }
      return pool;
    }

    public static DrivenGameObject Spawn(GameObjectComponentDescription withComponents) {
     return getOrCreatePool(withComponents).Spawn();
    }

    private Stack<DrivenGameObject> _objStack = new Stack<DrivenGameObject>();

    /// <summary>
    /// The DrivenGameObjectPool is itself associated with a GameObject, the parent of
    /// all GameObjects in its pool.
    /// </summary>
    private GameObject _backingPoolGameObject = null;
    public GameObject poolGameObject {
      get {
        if (_backingPoolGameObject == null) {
          _backingPoolGameObject = new GameObject("__Driven Game Object Pool__");
          _backingPoolGameObject.SetActive(false);
        }
        return _backingPoolGameObject;
      }
    }

    public DrivenGameObject Spawn() {
      if (_objStack.Count > 0) {
        return _objStack.Pop();
      }
      return new DrivenGameObject(this);
    }

    public void Recycle(DrivenGameObject drivenGameObject) {
      if (drivenGameObject.pool != this) {
        throw new InvalidOperationException(
          "[DrivenGameObjectPool] Tried to recycle a DrivenGameObject into this pool, but "
        + "it did not originate from this pool.");
      }

      drivenGameObject.gameObject.transform.parent = poolGameObject.transform;

      _objStack.Push(drivenGameObject);
    }

  }

  public class DrivenGameObject {

    private DrivenGameObjectPool _pool = null;
    public DrivenGameObjectPool pool { get { return _pool; } }

    private GameObject _gameObject = null;
    public GameObject gameObject {
      get {
        if (_gameObject == null) {
          _gameObject = new GameObject("__Driven Game Object__");
        }
        return _gameObject;
      }
    }

    public DrivenGameObject(DrivenGameObjectPool pool) {
      if (pool == null) {
        throw new ArgumentNullException(
          "DrivenGameObject instances must belong to a pool.");
      }
      _pool = pool;
    }

    public void Recycle() {
      pool.Recycle(this);
    }

  }

  #region Extensions

  public static class OptionalValueExtensions {
    public static T? ValueOr<T>(this T? foo, T? other) where T : struct {
      if (foo.HasValue) {
        return foo;
      }
      return other;
    }
  }

  #endregion

}