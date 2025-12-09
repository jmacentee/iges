using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            // TODO: Implement reading parameters for IgesShell
            return 0;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            // TODO: Implement writing parameters for IgesShell
        }
    }
}