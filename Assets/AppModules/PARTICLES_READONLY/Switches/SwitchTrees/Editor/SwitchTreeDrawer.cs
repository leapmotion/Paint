using Leap.Unity.Query;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Leap.Unity.Animation {

  [CustomPropertyDrawer(typeof(SwitchTree), true)]
  public class SwitchTreeDrawer : PropertyDrawer {

    #region Pair Class

    public class Pair<T, U> {
      public T first;
      public U second;

      public Pair(T first, U second) {
        this.first = first;
        this.second = second;
      }
    }

    #endregion

    #region GUI Properties & Colors

    private const float EXTRA_HEIGHT = 6f;
    private const float EXTRA_HEIGHT_PER_NODE = 1f;
    private const float INDENT_WIDTH = 17f;

    private const float BUTTON_RECT_INNER_PAD = 3f;

    private const float LINE_SIDE_MARGIN_RATIO = 0.46f;
    private const float LINE_ORIGIN_RATIO = 0.50f;

    private const float GLOW_WIDTH = 1f;
    private const float GLOW_LINE_SIDE_MARGIN_RATIO = 0.44f;
    private const float GLOW_LINE_ORIGIN_RATIO = 0.48f;


    private static Color backgroundColor {
      get {
        return EditorGUIUtility.isProSkin
            ? new Color32(56, 56, 56, 255)
            : new Color32(194, 194, 194, 255);
      }
    }

    private static Color headerBackgroundColor {
      get { return Color.Lerp(backgroundColor, Color.white, 0.4f); }
    }

    private static Color innerBackgroundColor {
      get { return Color.Lerp(backgroundColor, Color.black, 0.15f); }
    }

    private static Color glowBackgroundColor {
      get { return Color.Lerp(Color.cyan, Color.blue, 0.05f); }
    }

    private static Color glowContentColor {
      get { return Color.Lerp(Color.cyan, Color.white, 0.7f); }
    }

    #endregion

    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label) {
      return (EditorGUIUtility.singleLineHeight + EXTRA_HEIGHT_PER_NODE)
             * makeSwitchTree(property).NodeCount
             + EXTRA_HEIGHT
             + EditorGUIUtility.singleLineHeight // "Current node" label
             ;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

      var switchTree = makeSwitchTree(property);
      int nodeCount = switchTree.NodeCount;

      Rect curNodeStateLabelRect;
      position = position.PadTop(EditorGUIUtility.singleLineHeight, out curNodeStateLabelRect);
      drawCurNodeLabel(curNodeStateLabelRect, switchTree);

      var visitedNodesCache = Pool<HashSet<SwitchTree.Node>>.Spawn();
      try {
        bool isEvenRow = true;
        foreach (var nodeRectPair in switchTree.Traverse(visitedNodesCache)
                                               .Query()
                                               .Zip(position.TakeAllLines(nodeCount)
                                                            .Query(),
                                                    (node, rect) => {
                                                      return new Pair<SwitchTree.Node, Rect>
                                                                   (node, rect);
                                                    })) {
          drawNode(nodeRectPair.first, nodeRectPair.second, switchTree, property, isEvenRow);
          isEvenRow = !isEvenRow;
        }
      }
      finally {
        visitedNodesCache.Clear();
        Pool<HashSet<SwitchTree.Node>>.Recycle(visitedNodesCache);
      }

    }

    private SwitchTree makeSwitchTree(SerializedProperty treeProperty) {
      return new SwitchTree((treeProperty.FindPropertyRelative("rootSwitchBehaviour")
                                         .objectReferenceValue as MonoBehaviour).transform,
                            (treeProperty.FindPropertyRelative("_curActiveNodeName")
                                         .stringValue));
    }

    private void drawNode(SwitchTree.Node node, Rect rect, SwitchTree switchTree,
                          SerializedProperty treeProperty,
                          bool isEvenRow = true) {

      if (node.treeDepth == 0) {
        drawControllerBackground(rect);
      }
      else {
        drawTreeBackground(rect, isEvenRow);
      }
      
      Rect indentRect;
      Rect labelRect = rect.PadLeft(INDENT_WIDTH * (node.treeDepth + 1), out indentRect);
      drawNodeLabel(labelRect, node);

      Rect fullButtonRect = indentRect.TakeRight(INDENT_WIDTH, out indentRect);


      #region Debug Rects

      //EditorGUI.DrawRect(indentRect, Color.cyan);
      //EditorGUI.DrawRect(fullButtonRect, Color.magenta);

      #endregion


      // Line drawing.
      if (node.hasChildren) {
        bool anyChildGlowDueToParent = node.isOn && node.children
                                                        .Query()
                                                        .Any(c => c.isOn);
        drawCenteredLine(Direction4.Down, fullButtonRect,
                         anyChildGlowDueToParent ? LineType.Glow : LineType.Default);
      }
      if (node.hasParent) {
        bool glowingDueToParent = node.isOn && node.isParentOn;
        drawCenteredLine(Direction4.Left, fullButtonRect,
                         glowingDueToParent ? LineType.Glow : LineType.Default);
      }

      // Leftward rects.
      var curNode = node;
      bool firstLeftward = true;
      while (curNode.hasParent) {
        Rect leftwardRect = indentRect.TakeRight(INDENT_WIDTH, out indentRect);

        bool glowingDueToParent = curNode.isOn && curNode.isParentOn;

        bool isAnyPrevSiblingOnDueToParent = curNode.prevSiblings
                                                    .Query()
                                                    .Any(n => n.isOn && n.isParentOn);

        bool isSelfOrAnyPrevSiblingOnDueToParent = glowingDueToParent
          || isAnyPrevSiblingOnDueToParent;

        if (firstLeftward) {
          drawCenteredLine(Direction4.Right, leftwardRect,
                           glowingDueToParent ? LineType.Glow : LineType.Default);
          drawCenteredLine(Direction4.Up, leftwardRect,
                           isSelfOrAnyPrevSiblingOnDueToParent ? LineType.Glow
                                                               : LineType.Default);
          firstLeftward = false;
        }
        else if (curNode.hasPrevSibling) {
          drawCenteredLine(Direction4.Up, leftwardRect,
                           isAnyPrevSiblingOnDueToParent ? LineType.Glow
                                                         : LineType.Default);
        }

        if (curNode.hasPrevSibling) {
          drawCenteredLine(Direction4.Down, leftwardRect,
                           isAnyPrevSiblingOnDueToParent ? LineType.Glow
                                                         : LineType.Default);
        }

        curNode = curNode.hasParent ? curNode.parent.node : default(SwitchTree.Node);
      }


      Rect buttonRect = fullButtonRect.PadInner(BUTTON_RECT_INNER_PAD);

      // Support undo history.
      Undo.IncrementCurrentGroup();
      var curGroupIdx = Undo.GetCurrentGroup();

      bool test_isNodeOff = node.isOff;
      if (test_isNodeOff) {
        EditorGUI.DrawRect(buttonRect, Color.red);
      }

      Color origContentColor = GUI.contentColor;
      if (node.isOn) {
        Rect glowRect = buttonRect.PadOuter(GLOW_WIDTH);
        EditorGUI.DrawRect(glowRect, glowBackgroundColor);
        GUI.contentColor = glowContentColor;
      }

      if (GUI.Button(buttonRect, new GUIContent("Switch to this node."))) {

        // Note: It is the responsibility of the IPropertySwitch implementation
        // to perform operations that correctly report their actions in OnNow() to the
        // Undo history!
        switchTree.SwitchTo(node.transform.name, immediately: !Application.isPlaying);
        treeProperty.FindPropertyRelative("_curActiveNodeName").stringValue = node.transform.name;
      }

      Undo.CollapseUndoOperations(curGroupIdx);
      Undo.SetCurrentGroupName("Set Switch Tree State");

      if (node.isOn) {
        GUI.contentColor = origContentColor;
      }

    }

    private void drawCurNodeLabel(Rect labelRect, SwitchTree tree) {
      EditorGUI.LabelField(labelRect, new GUIContent("Current active state: "
                                                     + tree.curActiveNodeName,
                                            "This is the node name that will be returned "
                                          + "by the currentState property."));
    }

    private void drawNodeLabel(Rect labelRect, SwitchTree.Node node) {
      if (Event.current.isMouse && labelRect.Contains(Event.current.mousePosition)
          && Event.current.button == 0
          && Event.current.type == EventType.MouseDown) {
        EditorGUIUtility.PingObject(node.objSwitch as MonoBehaviour);
      }

      EditorGUI.LabelField(labelRect,
                           new GUIContent(node.transform.name
                                          + (node.treeDepth == 0 ? " (root)" : "")));
    }

    private void drawControllerBackground(Rect rect) {
      EditorGUI.DrawRect(rect, headerBackgroundColor);
    }

    private void drawTreeBackground(Rect rect, bool isEvenRow = true) {
      Color color = (isEvenRow ? Color.Lerp(innerBackgroundColor, Color.white, 0.1f) : innerBackgroundColor);
      EditorGUI.DrawRect(rect, color);
    }

    [System.Flags]
    private enum Direction4 {
      Up    = 1 << 0,
      Down  = 1 << 1,
      Left  = 1 << 2,
      Right = 1 << 3
    }

    private enum LineType {
      Default,
      Glow
    }

    private delegate void DrawLineFunc(Rect inRect, float sideRatio, float originRatio, Color color);

    private void drawCenteredLine(Direction4 directions, Rect inRect,
                                  LineType lineType = LineType.Default) {

      var draws = new List<DrawLineFunc>();
      if ((directions & Direction4.Up) > 0) {
        draws.Add(drawCenteredLineUp);
      }
      if ((directions & Direction4.Down) > 0) {
        draws.Add(drawCenteredLineDown);
      }
      if ((directions & Direction4.Left) > 0) {
        draws.Add(drawCenteredLineLeft);
      }
      if ((directions & Direction4.Right) > 0) {
        draws.Add(drawCenteredLineRight);
      }

      switch (lineType) {
        case LineType.Default:
          foreach (var draw in draws) {
            draw(inRect, LINE_SIDE_MARGIN_RATIO, LINE_ORIGIN_RATIO, Color.black);
          }
          break;
        case LineType.Glow:
          foreach (var draw in draws) {
            draw(inRect, GLOW_LINE_SIDE_MARGIN_RATIO, GLOW_LINE_ORIGIN_RATIO, glowBackgroundColor);
          }
          foreach (var draw in draws) {
            draw(inRect, LINE_SIDE_MARGIN_RATIO, LINE_ORIGIN_RATIO, glowContentColor);
          }
          break;
      }

    }

    private void drawCenteredLineUp(Rect rect, float sideRatio, float originRatio, Color color) {
      Rect middle = rect.PadLeftRightPercent(sideRatio);
      Rect line = middle.PadBottomPercent(originRatio);
      EditorGUI.DrawRect(line, color);
    }

    private void drawCenteredLineDown(Rect rect, float sideRatio, float originRatio, Color color) {
      Rect middle = rect.PadLeftRightPercent(sideRatio);
      Rect line = middle.PadTopPercent(originRatio);
      EditorGUI.DrawRect(line, color);
    }

    private void drawCenteredLineLeft(Rect rect, float sideRatio, float originRatio, Color color) {
      Rect middle = rect.PadTopBottomPercent(sideRatio);
      Rect line = middle.PadRightPercent(originRatio);
      EditorGUI.DrawRect(line, color);
    }

    private void drawCenteredLineRight(Rect rect, float sideRatio, float originRatio, Color color) {
      Rect middle = rect.PadTopBottomPercent(sideRatio);
      Rect line = middle.PadLeftPercent(originRatio);
      EditorGUI.DrawRect(line, color);
    }

    #region GUIStyle nonsense

    private static GUIStyle s_TintableStyle;

    private static GUIStyle TintableStyle {
      get {
        if (s_TintableStyle == null) {
          s_TintableStyle = new GUIStyle();
          s_TintableStyle.normal.background = EditorGUIUtility.whiteTexture;
          s_TintableStyle.stretchWidth = true;
        }
        return s_TintableStyle;
      }
    }

    //private static void DrawEmpty(Rect rect, Color color) {
    //  // Only need to perform drawing during repaints!
    //  if (Event.current.type == EventType.Repaint) {
    //    var restoreColor = GUI.color;
    //    GUI.color = color;
    //    TintableStyle.Draw(rect, false, false, false, false);
    //    GUI.color = restoreColor;
    //  }
    //}


    #endregion

  }

}