using System;
using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        // Added for LaserConvert compatibility
        public List<IgesFace>? Faces { get; set; }
        
        // Store raw pointers for manual binding after all entities are loaded
        public List<int>? FacePointers { get; private set; }
        public List<int>? FaceOrientations { get; private set; }

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int faceCount = Integer(parameters, index++);
            FacePointers = new List<int>();
            FaceOrientations = new List<int>();
            
            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                int orientation = Integer(parameters, index++);
                
                FacePointers.Add(facePointer);
                FaceOrientations.Add(orientation);
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