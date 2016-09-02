using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.Attributes;

[System.Serializable]
public class PoissonDisc {
  [SerializeField]
  public Vector2 area;

  [MinMax(0, 1)]
  [SerializeField]
  public Vector2 radiusRange;

  [MinValue(0)]
  [SerializeField]
  public float minDistApart;

  [MinValue(1)]
  [SerializeField]
  public int k = 20;

  private Stack<Disc> _activeDiscs = new Stack<Disc>();
  private List<Disc> _finalDiscs;

  public List<Disc> Generate() {
    _finalDiscs = new List<Disc>();

    generateFirstDisc();

    while (_activeDiscs.Count > 0) {
      Disc sourceDisc = _activeDiscs.Pop();

      for (int i = 0; i < k; i++) {
        float radius = Random.Range(radiusRange.x, radiusRange.y);
        float distance = Random.Range(minDistApart, minDistApart * 2);
        float angle = Random.value * Mathf.PI * 2;
        float centerToCenter = distance + radius + sourceDisc.radius;

        float x = centerToCenter * Mathf.Cos(angle) + sourceDisc.position.x;
        float y = centerToCenter * Mathf.Sin(angle) + sourceDisc.position.y;

        //If the circle intersects the edge
        if (x < radius || y < radius || x > (area.x - radius) || y > (area.y - radius)) {
          continue;
        }

        Disc newDisc = new Disc(new Vector2(x, y), radius);

        bool tooClose = false;
        for (int j = 0; j < _finalDiscs.Count; j++) {
          Disc otherDisc = _finalDiscs[j];
          float distApart = Vector2.Distance(newDisc.position, otherDisc.position) - newDisc.radius - otherDisc.radius;
          if (distApart < minDistApart) {
            tooClose = true;
            break;
          }
        }

        if (tooClose) {
          continue;
        }

        _activeDiscs.Push(newDisc);
        _finalDiscs.Add(newDisc);
      }
    }

    return _finalDiscs;
  }

  private void generateFirstDisc() {
    float radius = Random.Range(radiusRange.x, radiusRange.y);
    float x = Random.Range(radius, area.x - radius);
    float y = Random.Range(radius, area.y - radius);

    Disc startDisc = new Disc(new Vector2(x, y), radius);
    _activeDiscs.Push(startDisc);
    _finalDiscs.Add(startDisc);
  }

  public struct Disc {
    public Vector2 position;
    public float radius;

    public Disc(Vector2 position, float radius) {
      this.position = position;
      this.radius = radius;
    }
  }
}
