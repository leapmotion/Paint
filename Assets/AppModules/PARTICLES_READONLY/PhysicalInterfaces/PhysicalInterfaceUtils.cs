

namespace Leap.Unity.PhysicalInterfaces {

  public class PhysicalInterfaceUtils {

    /// <summary>
    /// The minimum speed past which a released object should be considered thrown,
    /// and beneath which a released object should be considered placed.
    /// </summary>
    public const float MIN_THROW_SPEED = 0.70f;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 10 m/s represents
    /// about a quarter of the speed of the world's fastest fastball.
    /// </summary>
    public const float MID_THROW_SPEED = 10.00f;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 40 m/s is about the
    /// speed of the fastest fast-ball. (~90 mph.)
    /// </summary>
    public const float MAX_THROW_SPEED = 40.00f;

    /// <summary>
    /// A standard speed for calculating e.g. how much time it should take for an
    /// element to move a given distance.
    /// </summary>
    public const float STANDARD_SPEED = 1.00f;

    /// <summary>
    /// A distance representing being well within arms-reach without being too close to
    /// the head.
    /// </summary>
    public const float OPTIMAL_UI_DISTANCE = 0.60f;

  }

}
