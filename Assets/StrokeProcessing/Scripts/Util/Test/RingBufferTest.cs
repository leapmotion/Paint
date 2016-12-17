using UnityEngine;
using System.Collections;

namespace Leap.Paint {

  public class RingBufferTest : MonoBehaviour {

    private const int NUM_OBJECTS = 5;

    public int curObj = 0;
    public GameObject[] _objs = new GameObject[NUM_OBJECTS];

    public RingBuffer<GameObject> objBuffer = new RingBuffer<GameObject>(3);

    protected void Update() {
      if (Input.GetKeyDown(KeyCode.Space)) {
        Color objColor = ROYGBIV(curObj);

        GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newObj.GetComponent<MeshRenderer>().material.color = objColor;
        objBuffer.Add(newObj);
        _objs[curObj] = newObj;

        curObj++;

        UpdateBufferVisuals();
      }
    }

    private void UpdateBufferVisuals() {
      for (int i = 0; i < _objs.Length; i++) {
        if (_objs[i] != null) {
          _objs[i].transform.position = Vector3.one * 1000F;
        }
      }

      Vector3 offset = Vector3.right * 2F;
      for (int i = 0; i < objBuffer.Size; i++) {
        objBuffer.GetFromEnd(i).transform.position = offset * i;
      }
    }

    private Color ROYGBIV(int idx) {
      if (idx == 0) return Color.red;
      if (idx == 1) return new Color(0.9F, 0.6F, 0.2F);
      if (idx == 2) return Color.yellow;
      if (idx == 3) return Color.green;
      if (idx == 4) return Color.blue;
      return Color.blue;
    }

  }


}