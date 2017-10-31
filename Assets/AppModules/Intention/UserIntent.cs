using System;

namespace Leap.Unity.Intention {

  #region Supporting Types

  public enum IntentionType {
    Free,
    SingleHand,
    BothHands
  }

  public struct IntentDefinition {

    public IntentionType type;

    public bool isSingleHand { get { return type == IntentionType.SingleHand; } }
    public Chirality handedness;

  }

  #endregion

  public class UserIntent {

    #region User Intent Objects

    #region Data

    private IntentDefinition _def;
    public IntentDefinition definition {
      get { return _def; }
      private set { _def = value; }
    }

    private bool _isActive = false;
    public bool isActive { get { return _isActive; } }

    #endregion

    #region Construction

    private UserIntent(IntentDefinition intentDefinition) {
      _def = intentDefinition;
    }

    private static UserIntent intentForSingleHand(Chirality whichHand) {
      return new UserIntent(new IntentDefinition() {
        type = IntentionType.SingleHand,
        handedness = whichHand
      });
    }

    private static UserIntent intentForBothHands() {
      return new UserIntent(new IntentDefinition() {
        type = IntentionType.BothHands
      });
    }

    //public static UserIntent ForFree() {
    //  return new UserIntent(new IntentDefinition() {
    //    type = IntentionType.Free
    //  });
    //}

    #endregion

    #region Events

    public event Action OnIntentGained = () => { };
    public event Action OnIntentLost = () => { };

    #endregion

    #region Methods

    /// <summary>
    /// Tries to receive the user's intention for this intention type. Returns false if
    /// the user already has an active intention that is mutually exclusive with this
    /// intention type, or if this intent is already active.
    /// </summary>
    public bool TryReceive() {
      if (_isActive) return false;

      _isActive = manager_TryReceive(this);
      return _isActive;
    }

    /// <summary>
    /// Drops this intent from the user's active intention(s). Return false if this
    /// intent wasn't active in the first place.
    /// </summary>
    public bool Drop() {
      if (!_isActive) return false;

      manager_Drop(this);
      return true;
    }

    /// <summary>
    /// Returns whether this intent is a SingleHand intent. If so, it's valid to call
    /// ChangeHandedness on this intent.
    /// </summary>
    public bool IsSingleHandIntent() {
      return definition.type == IntentionType.SingleHand;
    }

    /// <summary>
    /// Returns the handedness of this intent, valid only if the intent type is
    /// SingleHand.
    /// </summary>
    public Chirality GetHandedness() {
      return definition.handedness;
    }

    /// <summary>
    /// If this intent is a SingleHand intent, this method will safely switch the
    /// handedness of the intent definition. If the intent is active, e.g., for the right
    /// hand when this method is called, the intent will be dropped first, then it
    /// will attempt to re-gain the intent on the other hand.
    /// 
    /// This method will return true if this intent was a SingleHand intent and the
    /// intent was re-gained on the other hand. It does not that the intent will be
    /// re-gained on the other hand, which could be pre-occupied.
    /// 
    /// The method will also return false if the chirality of the intent definition is
    /// already the specified chirality.
    /// </summary>
    public bool ChangeHandedness(Chirality toWhichHand) {
      if (!definition.isSingleHand) return false;
      
      if (definition.handedness == toWhichHand) return false;

      if (_isActive) {
        manager_Drop(this);
      }

      definition = new IntentDefinition() {
        type = IntentionType.SingleHand,
        handedness = otherChirality(toWhichHand)
      };

      manager_TryReceive(this);

      return true;
    }

    #endregion

    #endregion

    #region Static Intention System

    private static UserIntent _activeLeftHandIntention = null;
    private static UserIntent _activeRightHandIntention = null;
    //private static List<UserIntent> _freeIntents;

    private UserIntent _freeLeftHandIntention  = intentForSingleHand(Chirality.Left);
    private UserIntent _freeRightHandIntention = intentForSingleHand(Chirality.Right);
    private UserIntent _freeBothHandIntention  = intentForBothHands();

    public static bool manager_TryReceive(UserIntent intent) {
      switch (intent.definition.type) {
        case IntentionType.SingleHand:
          if (intent.definition.handedness == Chirality.Left) {
            if (_activeLeftHandIntention == null) {
              _activeLeftHandIntention = intent;
              return true;
            }
            else return false;
          }
          else {
            if (_activeRightHandIntention == null) {
              _activeRightHandIntention = intent;
              return true;
            }
            else return false;
          }
        case IntentionType.BothHands:
          if (_activeLeftHandIntention == null && _activeRightHandIntention == null) {
            _activeLeftHandIntention = intent;
            _activeRightHandIntention = intent;
            return true;
          }
          else return false;
        default:
          return false;
      }
    }

    public static void manager_Drop(UserIntent intent) {
      if (intent == _activeLeftHandIntention)
        _activeLeftHandIntention = null;
      if (intent == _activeRightHandIntention)
        _activeRightHandIntention = null;
    }

    #endregion

    #region Helpers
    
    private static Chirality otherChirality(Chirality handedness) {
      return handedness == Chirality.Left ? Chirality.Right : Chirality.Left;
    }

    #endregion

  }


}
