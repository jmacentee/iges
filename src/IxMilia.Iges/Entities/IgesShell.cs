using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        // Added for LaserConvert compatibility
        public List<IgesFace>? Faces { get; set; }

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int faceCount = Integer(parameters, index++);
            Faces = new List<IgesFace>();
            System.Console.WriteLine($"[IGESSHELL] Label={EntityLabel} FaceCount={faceCount}");
            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                // IGES pointers are directory sequence numbers (1-based)
                System.Console.WriteLine($"[IGESSHELL] Label={EntityLabel} FacePointer={facePointer}");
                binder.BindEntity(facePointer, e => {
                    System.Console.WriteLine($"[IGESSHELL] Label={EntityLabel} Bound entity type={(e == null ? "null" : e.GetType().Name)} SeqNum={facePointer}");
                    if (e is IgesFace face)
                    {
                        Faces.Add(face);
                        System.Console.WriteLine($"[IGESSHELL] Added face: {face.EntityLabel} SeqNum={facePointer}");
                    }
                    else if (e != null)
                    {
                        System.Console.WriteLine($"[IGESSHELL] Pointer {facePointer} resolved to non-face: {e.GetType().Name}");
                    }
                    else
                    {
                        System.Console.WriteLine($"[IGESSHELL] Pointer {facePointer} did not resolve to any entity.");
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
                }
            }
        }
    }
}