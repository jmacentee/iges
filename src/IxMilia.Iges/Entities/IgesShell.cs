using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        // Added for LaserConvert compatibility
        public List<IgesFace>? Faces { get; set; }
        
        // Store raw pointers for debugging/workaround
        public List<int>? FacePointers { get; set; }
        public List<int>? FaceOrientations { get; set; }

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int faceCount = Integer(parameters, index++);
            Faces = new List<IgesFace>();
            FacePointers = new List<int>();
            FaceOrientations = new List<int>();
            
            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                int orientation = Integer(parameters, index++);  // Read orientation flag (1 or -1, but could be 0)
                
                FacePointers.Add(facePointer);
                FaceOrientations.Add(orientation);
                
                // IGES pointers are directory sequence numbers (1-based)
                binder.BindEntity(facePointer, e => {
                    if (e is IgesFace face)
                    {
                        Faces.Add(face);
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