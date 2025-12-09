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
            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                binder.BindEntity(facePointer, e => {
                    if (e is IgesFace face)
                        Faces.Add(face);
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