using System;
using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        // Post-binding: populated faces from binder resolution
        public List<IgesFace>? Faces { get; set; }

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int faceCount = Integer(parameters, index++);
            
            Faces = new List<IgesFace>();

            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                int orientation = Integer(parameters, index++);
                
                // Use the binder to defer face resolution until all entities are loaded
                // This is the standard IxMilia pattern for entity pointer resolution
                binder.BindEntity(facePointer, (face) =>
                {
                    if (face is IgesFace igsFace)
                    {
                        Faces.Add(igsFace);
                    }
                });
            }
            
            return index;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(Faces?.Count ?? 0);
            if (Faces != null)
            {
                foreach (var face in Faces)
                {
                    parameters.Add(binder.GetEntityId(face));
                    parameters.Add(1);  // orientation flag (typically 1 for forward, -1 for reverse)
                }
            }
        }
    }
}