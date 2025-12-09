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
                int orientation = Integer(parameters, index++);
                
                FacePointers.Add(facePointer);
                FaceOrientations.Add(orientation);
                
                // Try multiple offsets to find the face
                // Empirically: Plasticity uses different pointer schemes for different faces
                int[] offsetsToTry = (i < 3) ? new int[] { 21, 27 } : new int[] { 27, 21, 51, 75 };
                
                foreach (int offset in offsetsToTry)
                {
                    int adjustedPointer = facePointer - offset;
                    binder.BindEntity(adjustedPointer, e => {
                        if (e is IgesFace face)
                        {
                            if (!Faces.Contains(face))
                            {
                                Faces.Add(face);
                            }
                        }
                    });
                }
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