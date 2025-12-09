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
            int index = 0;
            // Surface pointer
            int surfacePointer = Integer(parameters, index++);
            if (surfacePointer > 0)
            {
                binder.BindEntity(surfacePointer, e => Surface = e);
            }
            // Loop count
            int loopCount = Integer(parameters, index++);
            Loops = new List<IgesLoop>();
            for (int i = 0; i < loopCount; i++)
            {
                int loopPointer = Integer(parameters, index++);
                binder.BindEntity(loopPointer, e => {
                    if (e is IgesLoop loop)
                        Loops.Add(loop);
                });
            }
            return index;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            // Write Surface pointer
            parameters.Add(binder.GetEntityId(Surface));
            // Write Loop count
            parameters.Add(Loops?.Count ?? 0);
            // Write Loop pointers
            if (Loops != null)
            {
                foreach (var loop in Loops)
                {
                    parameters.Add(binder.GetEntityId(loop));
                }
            }
        }
    }
}