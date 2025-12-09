using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesFace : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Face;

        // Added for LaserConvert compatibility
        public IgesEntity? Surface { get; set; }
        public List<IgesLoop>? Loops { get; set; }

        public IgesFace() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            // TODO: Implement reading parameters for IgesFace
            return 0;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            // TODO: Implement writing parameters for IgesFace
        }
    }
}