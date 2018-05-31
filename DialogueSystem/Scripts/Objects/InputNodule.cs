using System;

namespace DialogueSystem {
    [Nodule ("InputNodule", false, "OutputNodule")]
    public class InputNodule : BaseNodule {
        public override NoduleSide DefaultSide { get { return NoduleSide.Left; } }
    }
}