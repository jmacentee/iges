using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        // Added for LaserConvert compatibility
        public List<IgesEntity>? Curves { get; set; }
        public bool IsOuter { get; set; }

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            // TODO: Implement reading parameters for IgesLoop
            return 0;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            // TODO: Implement writing parameters for IgesLoop
        }
    }
}