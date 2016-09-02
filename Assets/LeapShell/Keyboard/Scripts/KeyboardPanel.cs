using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ButtonManager))]
[RequireComponent(typeof(CurvedRect))]
public class KeyboardPanel : MonoBehaviour {
  private string[] Characters = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "<--",
                                 "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
                                 "a", "s", "d", "f", "g", "h", "j", "k", "l", "<<",
                                 "z", "x", "c", "v", "b", "n", "m", ",", ".", "/"};

  public KeyboardKey _button;
  public KeyboardKey shiftKey;
  public KeyboardKey spaceBar;
  public Text TextArea;
  float spacing = 0.04f;
  CurvedRect _rect;
  ButtonManager _manager;
  List<ButtonBase> keys = new List<ButtonBase>(43);
  bool shiftBeingHeld = false;
  private string letterBuffer;

	void Start () {
    _manager = GetComponent<ButtonManager>();
    _rect = GetComponent<CurvedRect>();

    int curColumn = 0, curRow = 0;
    int[] rowLengths = { 11, 10, 10, 10};
    for (int currentKey = 0; currentKey < Characters.Length; currentKey++) {
      if (curColumn > rowLengths[curRow] - 1) {
        curColumn = 0;
        curRow++;
      }

      GameObject key = Instantiate(_button.gameObject, transform) as GameObject;
      Vector2 keyPosition = new Vector2((curColumn * spacing) - (_rect.Width * 0.4f) + (curRow * spacing * 0.3f), (curRow * -spacing) + (_rect.Height * 0.35f));
      key.transform.localPosition = _rect.Space.RectToLocal(keyPosition, _rect.RadiusOffset);
      key.transform.localRotation = _rect.Space.RectToLocal(Quaternion.identity, keyPosition);
      key.name = Characters[currentKey].ToString();
      key.GetComponent<KeyboardKey>().keyLabel.text = Characters[currentKey];
      key.SetActive(true);
      keys.Add(key.GetComponent<KeyboardKey>());

      if (Characters[currentKey].Equals("z")){
        keyPosition -= new Vector2(spacing * 1.7f, 0f);
        shiftKey.transform.localPosition = _rect.Space.RectToLocal(keyPosition, _rect.RadiusOffset);
        shiftKey.transform.localRotation = _rect.Space.RectToLocal(Quaternion.identity, keyPosition);
        shiftKey.gameObject.SetActive(true);
      }else if (Characters[currentKey].Equals("n")){
        keyPosition -= new Vector2(0f, spacing);
        spaceBar.transform.localPosition = _rect.Space.RectToLocal(keyPosition, _rect.RadiusOffset);
        spaceBar.transform.localRotation = _rect.Space.RectToLocal(Quaternion.identity, keyPosition);
        spaceBar.gameObject.SetActive(true);
      }
      curColumn++;
    }

    _manager.AddButtons(keys);
    _manager.AddButton(spaceBar);
    _manager.AddButton(shiftKey);

    SymSpell.CreateDictionary(Application.dataPath + "/../dictionary.txt", "");
	}

  public void typeKey(ButtonBase button) {
    if (button.name.Equals("SHIFT")) {
      if (shiftBeingHeld) {
        shiftBeingHeld = !shiftBeingHeld;
        for (int i = 0; i < keys.Count; i++) {
          keys[i].GetComponent<KeyboardKey>().keyLabel.text = Characters[i];
        }
      } else {
        shiftBeingHeld = !shiftBeingHeld;
        for (int i = 0; i < keys.Count; i++) {
          keys[i].GetComponent<KeyboardKey>().keyLabel.text = shiftLetter(Characters[i]);
        }
      }
    } else if (button.name.Equals("<--")) {
      if (TextArea.text.Length > 0) {
        TextArea.text = TextArea.text.Substring(0, TextArea.text.Length - 1);
      }
    } else if (button.name.Equals("<<")) {
      TextArea.text += "\n";
    } else if (button.name.Equals(" ")) {
      string[] typedText = TextArea.text.Split(' ');
      string correctedWord = SymSpell.Correct(typedText[typedText.Length - 1], "");
      if (correctedWord != null) {
        string correctedText = "";
        typedText[typedText.Length - 1] = SymSpell.Correct(typedText[typedText.Length - 1], "");
        for (int i = 0; i < typedText.Length; i++) {
          correctedText += typedText[i] + ' ';
        }
        TextArea.text = correctedText;
      } else {
        TextArea.text += " ";
      }
    }else {
      if (!shiftBeingHeld) {
        TextArea.text += button.name;
      } else {
        TextArea.text += shiftLetter(button.name);
      }
    }
  }

  string shiftLetter(string key) {
    string upper = key.ToUpper();
    if (key.Equals(upper)) {
      if (key.Equals("1")) {
        upper = "!";
      } else if (key.Equals("2")) {
        upper = "@";
      } else if (key.Equals("3")) {
        upper = "#";
      } else if (key.Equals("4")) {
        upper = "$";
      } else if (key.Equals("5")) {
        upper = "%";
      } else if (key.Equals("6")) {
        upper = "^";
      } else if (key.Equals("7")) {
        upper = "&";
      } else if (key.Equals("8")) {
        upper = "*";
      } else if (key.Equals("9")) {
        upper = "(";
      } else if (key.Equals("0")) {
        upper = ")";
      } else if (key.Equals(",")) {
        upper = "<";
      } else if (key.Equals(".")) {
        upper = ">";
      } else if (key.Equals("/")) {
        upper = "?";
      }
    }
    return upper;
  }
}
