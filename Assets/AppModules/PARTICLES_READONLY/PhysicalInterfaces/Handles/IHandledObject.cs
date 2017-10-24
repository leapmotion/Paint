using System.Collections.Generic;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandledObject {

    ICollection<SerializeableHandle> handles { get; }

  }

}