using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Interface for a component that can provide an indexable sequence of GameObjects.
  /// </summary>
  public interface IGameObjectSequenceProvider {

    int Count { get; }

    GameObject this[int idx] { get; }

  }

  /// <summary>
  /// Enumerator for an IGameObjectSequence.
  /// </summary>
  public struct IGameObjectSequenceEnumerator {

    IGameObjectSequenceProvider sequence;
    int index;

    public IGameObjectSequenceEnumerator(IGameObjectSequenceProvider sequence) {
      this.sequence = sequence;
      index = 0;
    }

    public bool MoveNext() { index++;  return index < sequence.Count - 1; }
    public GameObject Current { get { return sequence[index]; } }

  }

  public static class IGameObjectSequenceProviderExtensions {

    public static IGameObjectSequenceEnumerator GetEnumerator(this IGameObjectSequenceProvider sequence) {
      return new IGameObjectSequenceEnumerator(sequence);
    }

  }

}